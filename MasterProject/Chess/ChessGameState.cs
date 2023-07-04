using System.Collections.Generic;
using System.Text.Json.Serialization;
using static MasterProject.Chess.BitMaskUtils;

namespace MasterProject.Chess {
    
    public class ChessGameState : GameState<ChessGameState, ChessMove, ChessPlayerState> {

        public const int BOARD_SIZE = 8;

        public const int INDEX_WHITE = 0;
        public const int INDEX_BLACK = 1;

        public const int PLAYER_COUNT = 2;

        // https://en.wikipedia.org/wiki/Rules_of_chess

        public ChessPiece[] board { get; set; }
        public long pieceHasMoved { get; set; }
        public ChessPlayerState[] playerStates { get; set; }
        public int currentPlayerIndex { get; set; }
        public int movesSinceLastCaptureOrPawnMove { get; set; }

        [JsonIgnore]
        public ChessGameState previousState { get; set; }

        [JsonIgnore]
        public override IReadOnlyList<ChessPlayerState> PlayerStates => playerStates;

        [JsonIgnore]
        public override int CurrentPlayerIndex => currentPlayerIndex;

        public ChessGameState GetResultOfMove (ChessMove move) {
            var output = new ChessGameState();
            output.board = this.CloneBoard();
            output.playerStates = this.ClonePlayerStates();
            output.pieceHasMoved = this.pieceHasMoved;
            output.movesSinceLastCaptureOrPawnMove = this.movesSinceLastCaptureOrPawnMove;
            output.currentPlayerIndex = this.currentPlayerIndex;
            output.ApplyMove(move);
            output.UpdatePlayerAttackMapsAndCheckStates();
            output.currentPlayerIndex = (this.currentPlayerIndex + 1) % PLAYER_COUNT;
            output.UpdateGameIsOver();
            output.previousState = this;
            return output;
        }

        public ChessPiece[] CloneBoard () {
            return (ChessPiece[])(board.Clone());   // this should work. shallow copying primitives and such...
        }

        // i could make playerstates structs
        // this would prevent inheritance
        // but i could make the structs generic with a payload
        // the payload would also have to be value-type
        // which would be a bit of a pain in the ass as i can't do playerstate[0].customdata.something = value
        public ChessPlayerState[] ClonePlayerStates () {
            var output = new ChessPlayerState[this.playerStates.Length];
            for (int i = 0; i < PLAYER_COUNT; i++) {
                output[i] = this.playerStates[i].Clone();
            }
            return output;
        }

        public void ApplyMove (ChessMove move) {
            var srcPiece = board[move.srcCoord];
            var dstPiece = board[move.dstCoord];
            board[move.srcCoord] = ChessPiece.None;
            SetPositionHasBeenMoved(move.srcCoord, true);
            if (move.castle) {
                CoordToXY(move.srcCoord, out var kingX, out var kingY);
                CoordToXY(move.dstCoord, out var castleX, out var castleY);
                int rookSrcX, rookDstX;
                if (castleX < kingX) {  // queenside
                    rookSrcX = 0;
                    rookDstX = castleX + 1;
                } else {                // kingside
                    rookSrcX = BOARD_SIZE - 1;
                    rookDstX = castleX - 1;
                }
                var rookSrcCoord = XYToCoord(rookSrcX, kingY);
                var rookDstCoord = XYToCoord(rookDstX, kingY);
                board[rookDstCoord] = board[rookSrcCoord];
                board[rookSrcCoord] = ChessPiece.None;
                SetPositionHasBeenMoved(rookSrcCoord, true);
                SetPositionHasBeenMoved(rookDstCoord, true);
                playerStates[currentPlayerIndex].HasCastled = true;
            }
            if (move.enPassantCapture) {
                CoordToXY(move.srcCoord, out _, out var srcY);
                CoordToXY(move.dstCoord, out var dstX, out _);
                var passantCoord = XYToCoord(dstX, srcY);
                board[passantCoord] = ChessPiece.None;
            }
            board[move.dstCoord] = ((move.promoteTo != ChessPiece.None) ? move.promoteTo : srcPiece);
            SetPositionHasBeenMoved(move.dstCoord, true);   // important if we're capturing a piece that hasn't moved
            if (srcPiece.IsKing()) {
                playerStates[currentPlayerIndex].KingCoord = move.dstCoord;
            }
            playerStates[currentPlayerIndex].EnPassantableColumn = -1;  // the "reset" as en passant only works if the move was made the previous turn
            if (srcPiece.IsPawn()) {
                CoordToXY(move.srcCoord, out var srcX, out var srcY);
                CoordToXY(move.dstCoord, out var dstX, out var dstY);
                if ((srcX == dstX) && (Math.Abs(srcY - dstY) > 1)) {
                    playerStates[currentPlayerIndex].EnPassantableColumn = srcX;    // the set here is deep down in two ifs, but the reset is always done beforehand
                }
                movesSinceLastCaptureOrPawnMove = 0;
            } else if ((dstPiece != ChessPiece.None) && (srcPiece.IsWhite() != dstPiece.IsWhite())) {
                movesSinceLastCaptureOrPawnMove = 0;
            } else {
                movesSinceLastCaptureOrPawnMove++;
            }
        }

        public void UpdatePlayerAttackMapsAndCheckStates () {
            var whiteAttackMap = 0L;
            var blackAttackMap = 0L;
            for (int i = 0; i < board.Length; i++) {
                if (board[i] != ChessPiece.None) {
                    if (board[i].IsWhite()) {
                        whiteAttackMap |= ChessMoveUtils.GetAttackMap(this, i);
                    } else {
                        blackAttackMap |= ChessMoveUtils.GetAttackMap(this, i);
                    }
                }
            }
            playerStates[INDEX_WHITE].AttackMap = whiteAttackMap;
            playerStates[INDEX_BLACK].AttackMap = blackAttackMap;
            playerStates[INDEX_WHITE].IsInCheck = GetLongBit(blackAttackMap, playerStates[INDEX_WHITE].KingCoord);
            playerStates[INDEX_BLACK].IsInCheck = GetLongBit(whiteAttackMap, playerStates[INDEX_BLACK].KingCoord);
        }

        public void UpdateGameIsOver () {
            if (!AnyLegalMovesForPlayer(currentPlayerIndex)) {
                if (playerStates[currentPlayerIndex].IsInCheck) {
                    SetVictoryForPlayer((currentPlayerIndex + 1) % PLAYER_COUNT);
                    return;
                }
                SetDraw();
                return;
            }
            // the following two rules are only draws if a player claims them as such, but for the sake of not drawing out tournaments even longer i'll just auto-claim the draw
            if (movesSinceLastCaptureOrPawnMove > 100) {    // - There has been no capture or pawn move in the last fifty moves (move = both players get to play) by each player, if the last move was not a checkmate (see fifty-move rule).
                SetDraw();
                return;
            }
            if (DetermineHasRepeatedMultipleTimes(3)) {     // - The same board position has occurred three times with the same player to move and all pieces having the same rights to move, including the right to castle or capture en passant(see threefold repetition rule).
                SetDraw();
                return;
            }
            if (DetermineIfBoardIsDeadPosition()) {
                SetDraw();
                return;
            }

            void SetDraw () {
                for (int i = 0; i < PLAYER_COUNT; i++) {
                    playerStates[i].HasDrawn = true;
                }
            }

            void SetVictoryForPlayer (int playerIndex) {
                for (int i = 0; i < PLAYER_COUNT; i++) {
                    playerStates[i].HasWon = (i == playerIndex);
                    playerStates[i].HasLost = (i != playerIndex);
                }
            }
        }

        public static int XYToCoord (int x, int y) {
            return (y * BOARD_SIZE) + x;
        }

        public static void CoordToXY (int coord, out int x, out int y) {
            y = coord / BOARD_SIZE;
            x = coord % BOARD_SIZE;
        }

        public static string CoordToString (int coord) {
            CoordToXY(coord, out var x, out var y);
            return $"{(char)('a' + x)}{y + 1}";
        }

        public static int CoordFromString (string value) {
            if (string.IsNullOrWhiteSpace(value)) {
                return -1;
            }
            if (value.Length != 2) {
                return -1;
            }
            
            var x = (char.ToLower(value[0]) - (int)('a'));
            var y = (value[1] - (int)('1'));
            if (x < 0 || x >= BOARD_SIZE || y < 0 || y >= BOARD_SIZE) {
                return -1;
            }
            return XYToCoord(x, y);
        }

        public static bool CheckIsInbounds (int x, int y) {
            return (x >= 0 && x < BOARD_SIZE && y >= 0 && y < BOARD_SIZE);
        }

        public bool GetPositionHasBeenMoved (int coord) => GetLongBit(pieceHasMoved, coord);
        public bool GetPositionHasBeenMoved (int x, int y) => GetPositionHasBeenMoved(XYToCoord(x, y));
        public void SetPositionHasBeenMoved (int coord, bool value) => pieceHasMoved = SetLongBit(pieceHasMoved, coord, value);
        public void SetPositionHasBeenMoved (int x, int y, bool value) => SetPositionHasBeenMoved(XYToCoord(x, y), value);

        public ChessPiece GetPieceAtPosition (int x, int y) {
            return GetPieceAtCoordinate(XYToCoord(x, y));
        }

        public ChessPiece GetPieceAtCoordinate (int coord) {
            return board[coord];
        }

        public int CountTotalPiecesOfColor (int colorId) {
            var output = 0;
            for (int i = 0; i < board.Length; i++) {
                if ((((int)(board[i])) & ChessPieceUtils.MASK_COLOR) == colorId) {
                    output++;
                }
            }
            return output;
        }

        public int CountNumberOfPieces (ChessPiece piece) {
            var output = 0;
            for (int i = 0; i < board.Length; i++) {
                if (board[i] == piece) {
                    output++;
                }
            }
            return output;
        }

        public void Initialize () {
            this.board = ChessPieceUtils.GetInitialBoard();
            for (int i = 0; i < board.Length; i++) {
                SetPositionHasBeenMoved(i, board[i] == ChessPiece.None);
            }
            this.playerStates = new ChessPlayerState[PLAYER_COUNT];
            for (int i = 0; i < PLAYER_COUNT; i++) {
                playerStates[i] = new ChessPlayerState();
                playerStates[i].IsInCheck = false;
                playerStates[i].HasCastled = false;
                playerStates[i].EnPassantableColumn = -1;
            }
            playerStates[INDEX_WHITE].KingCoord = Array.IndexOf(board, ChessPiece.WhiteKing);
            playerStates[INDEX_BLACK].KingCoord = Array.IndexOf(board, ChessPiece.BlackKing);
            UpdatePlayerAttackMapsAndCheckStates();
            this.currentPlayerIndex = INDEX_WHITE;
            this.movesSinceLastCaptureOrPawnMove = 0;
        }

        public string ToPrintableString (bool includeRowAndColumnLabels = true) {
            return GetPrintableString((coord) => board[coord].ToShortString()[0], includeRowAndColumnLabels);
        }

        public static string MakePrintableAttackMap (int pieceCoord, long map, bool includeRowAndColumnLabels = true) {
            return GetPrintableString((coord) => {
                if (coord == pieceCoord) {
                    return '@';
                } else if (BitMaskUtils.GetLongBit(map, coord)) {
                    return '#';
                } else {
                    return '-';
                }
            }, includeRowAndColumnLabels);
        }

        private static string GetPrintableString (System.Func<int, char> getCharacterForCoordinate, bool includeRowAndColumnLabels) {
            var sb = new System.Text.StringBuilder();
            for (int y = BOARD_SIZE - 1; y >= 0; y--) {
                if (includeRowAndColumnLabels) {
                    sb.Append($"{y + 1}   ");
                }
                for (int x = 0; x < BOARD_SIZE; x++) {
                    sb.Append($"{getCharacterForCoordinate(XYToCoord(x, y))} ");
                }
                sb.Append(System.Environment.NewLine);
            }
            if (includeRowAndColumnLabels) {
                sb.Append(System.Environment.NewLine);
                sb.Append($"    ");
                for (int x = 0; x < BOARD_SIZE; x++) {
                    sb.Append($"{(char)('a' + x)} ");
                }
            }
            return sb.ToString();
        }

        public override IReadOnlyList<ChessMove> GetPossibleMovesForCurrentPlayer () {
            return new List<ChessMove>(EnumerateLegalMovesForPlayer(currentPlayerIndex));
        }

        private bool AnyLegalMovesForPlayer (int playerIndex) {
            foreach (var move in EnumerateLegalMovesForPlayer(playerIndex)) {
                return true;
            }
            return false;
        }

        // leaves out a few things that would happen if i'd just check the result of a move
        // notably it ignores the attack map for the player we're determining check for
        // and enables an early return if we quickly find the checking piece
        // although the search could perhaps be optimized, maybe by checking the pieces that were previously checking the king first
        // on the other hand, to be sure that the king is not in check we DO need to iterate over every piece
        // so i'm not sure if that really would give any notable improvement overall
        public bool DetermineIfMoveLeavesKingInCheck (ChessMove move, int playerIndex) {
            var temp = new ChessGameState();
            temp.board = this.CloneBoard();
            temp.playerStates = this.ClonePlayerStates();
            temp.currentPlayerIndex = this.currentPlayerIndex;
            temp.ApplyMove(move);
            var ownKingCoord = temp.playerStates[playerIndex].KingCoord;
            var otherPlayerColorId = (playerIndex == INDEX_WHITE ? ChessPieceUtils.ID_BLACK : ChessPieceUtils.ID_WHITE);
            for (int i = 0; i < temp.board.Length; i++) {
                var pieceColorId = (int)(temp.board[i]) & ChessPieceUtils.MASK_COLOR;
                if (pieceColorId == otherPlayerColorId) {
                    var attackMap = ChessMoveUtils.GetAttackMap(temp, i);
                    if (GetLongBit(attackMap, ownKingCoord)) {
                        return true;
                    }
                }
            }
            return false;
        }

        private IEnumerable<ChessMove> EnumerateLegalMovesForPlayer (int playerIndex) {
            foreach (var coord in CoordsWithPiecesOfPlayer(playerIndex)) {
                foreach (var move in ChessMoveUtils.GetLegalMovesForPiece(this, coord)) {
                    yield return move;
                }
            }
        }

        public bool DetermineHasRepeatedMultipleTimes (int targetNumberOfTimes) {
            var tempParent = this.previousState;
            var numberOfRepeats = 0;
            while(true){
                if (tempParent == null) {
                    return false;
                }
                var checkState = tempParent.previousState;
                if (checkState == null) {
                    return false;
                }
                if (CompareBoardAndAvailableMovesIdentical(this, checkState)) {
                    numberOfRepeats++;
                    if (numberOfRepeats >= targetNumberOfTimes) {
                        return true;
                    }
                }
                tempParent = checkState.previousState;
            }
        }

        // TODO if this part takes too long, i could speed it up with a hashcode
        // precalculate and cache moves in the result-gamestate-getter (and initialization of course) (and then return that cache when asked for it)
        // only if hashes match, do a more in-depth comparison
        // on the other hand, the boards already do need to be identical for there to be a chance of moves matching too
        // and that requires both players to be repeating everything
        // in which case the game will be ended
        // eh. just see how long a random vs random match takes
        // if it's in the order of a second, maybe that's a problem
        // then i can see what's taking so long
        // and if it is this part, which will take longer and longer the longer the game goes on, i can work on this
        // but assuming even one player is trying to do something, then the game should not take forever
        // ...
        // i hope...
        public static bool CompareBoardAndAvailableMovesIdentical (ChessGameState a, ChessGameState b) {
            if (a.currentPlayerIndex != b.currentPlayerIndex) {
                return false;
            }
            for (int i = 0; i < a.board.Length; i++) {
                if (a.board[i] != b.board[i]) {
                    return false;
                }
            }
            foreach (var coord in a.CoordsWithPiecesOfPlayer(a.currentPlayerIndex)) {
                var aMoves = new List<ChessMove>(ChessMoveUtils.GetLegalMovesForPiece(a, coord));
                var bMoves = new List<ChessMove>(ChessMoveUtils.GetLegalMovesForPiece(b, coord));
                if (aMoves.Count != bMoves.Count) {
                    return false;
                }
                for (int i = 0; i < aMoves.Count; i++) {
                    if (aMoves[i] != bMoves[i]) {   // this comparison can be made as moves are returned in a deterministic order
                        return false;
                    }
                }
            }
            return true;
        }

        public bool DetermineIfBoardIsDeadPosition () {
            int lastWhiteBishopCoord = -1;
            int lastBlackBishopCoord = -1;
            int lastKnightCoord = -1;
            for (int i = 0; i < board.Length; i++) {
                if (board[i] != ChessPiece.None) {
                    var colorId = (int)(board[i]) & ChessPieceUtils.MASK_COLOR;
                    var pieceId = (int)(board[i]) & ~ChessPieceUtils.MASK_COLOR;
                    switch (pieceId) {
                        case ChessPieceUtils.ID_KING:
                            break;
                        case ChessPieceUtils.ID_KNIGHT:
                            if (lastKnightCoord >= 0) {
                                return false;
                            }
                            lastKnightCoord = i;
                            break;
                        case ChessPieceUtils.ID_BISHOP:
                            if (colorId == ChessPieceUtils.ID_WHITE) {
                                if (lastWhiteBishopCoord >= 0) {
                                    return false;
                                }
                                lastWhiteBishopCoord = i;
                            } else {
                                if (lastBlackBishopCoord >= 0) {
                                    return false;
                                }
                                lastBlackBishopCoord = i;
                            }
                            break;
                        default:
                            return false;
                    }
                }
            }
            if (lastWhiteBishopCoord >= 0 && lastBlackBishopCoord >= 0) {
                CoordToXY(lastWhiteBishopCoord, out var whiteX, out var whiteY);
                CoordToXY(lastBlackBishopCoord, out var blackX, out var blackY);
                var whiteFieldCol = ((whiteX + whiteY) % 2);
                var blackFieldCol = ((blackX + blackY) % 2);
                return whiteFieldCol == blackFieldCol;
            }
            return true;
        }

        public IEnumerable<int> CoordsWithPiecesOfPlayer (int playerIndex) {
            var playerId = (playerIndex == INDEX_WHITE ? ChessPieceUtils.ID_WHITE : ChessPieceUtils.ID_BLACK);
            for (int i = 0; i < board.Length; i++) {
                var pieceId = (int)(board[i]);
                if ((pieceId & playerId) == playerId) {
                    yield return i;
                }
            }
        }

        public override IReadOnlyList<PossibleOutcome<ChessGameState>> GetPossibleOutcomesForMove (ChessMove move) {
            return new PossibleOutcome<ChessGameState>[]{
                new PossibleOutcome<ChessGameState>(){
                    GameState = this.GetResultOfMove(move),
                    Probability = 1
                }
            };
        }

        public override ChessGameState GetVisibleGameStateForPlayer (int playerIndex) {
            return this;
        }

        public static ChessGameState RunSeriesOfMovesAsStrings (ChessGameState src, bool printDebugStuff, params string[] moves) {
            var gs = src;
            for (int i = 0; i < moves.Length; i++) {
                if (printDebugStuff) {
                    Console.WriteLine($" > {moves[i]}");
                }
                var moveSplit = moves[i].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var srcCoord = CoordFromString(moveSplit[0]);
                var dstCoord = CoordFromString(moveSplit[1]);
                var nextMoveFound = false;
                var nextMove = default(ChessMove);
                foreach (var move in gs.GetPossibleMovesForCurrentPlayer()) {
                    if (move.srcCoord == srcCoord && move.dstCoord == dstCoord) {
                        nextMoveFound = true;
                        nextMove = move;
                        break;
                    }
                }
                if (!nextMoveFound) {
                    throw new System.Exception($"Something went wrong at move index {i} (\"{moves[i]}\")");
                } else {
                    gs = gs.GetResultOfMove(nextMove);
                }
                if (printDebugStuff) {
                    Console.WriteLine(gs.ToPrintableString());
                }
            }
            return gs;
        }

    }

}
