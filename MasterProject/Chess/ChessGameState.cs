using System.Collections.Generic;
using System.Text.Json.Serialization;

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
            output.currentPlayerIndex = (this.currentPlayerIndex + 1) % PLAYER_COUNT;
            output.ApplyMove(move);
            output.UpdatePlayerStates();
            output.UpdateGameIsOver();
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
            board[move.srcCoord] = ChessPiece.None;
            SetPositionHasBeenMoved(move.srcCoord, true);
            if (move.castle) {
                // TODO
                playerStates[currentPlayerIndex].HasCastled = true;
                throw new System.NotImplementedException();
            }
            if (move.enPassant) {
                CoordToXY(move.srcCoord, out _, out var srcY);
                CoordToXY(move.dstCoord, out var dstX, out _);
                var passantCoord = XYToCoord(dstX, srcY);
                board[passantCoord] = ChessPiece.None;
            }
            board[move.dstCoord] = ((move.promoteTo != ChessPiece.None) ? move.promoteTo : srcPiece);
            if (srcPiece.IsKing()) {
                playerStates[currentPlayerIndex].KingCoord = move.dstCoord;
            }
        }

        public void UpdatePlayerStates () {
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
                    SetVictoryForPlayer(this.currentPlayerIndex);
                    return;
                }
                SetDraw();
                return;
            }
            if (DetermineIfBoardIsDeadPosition()) {
                SetDraw();
                return;
            }
            // Other draw conditions (https://en.wikipedia.org/wiki/Rules_of_chess)
            // The player having the move claims a draw by correctly declaring that one of the following conditions exists, or by correctly declaring an intention to make a move which will bring about one of these conditions:
            // - The same board position has occurred three times with the same player to move and all pieces having the same rights to move, including the right to castle or capture en passant(see threefold repetition rule).
            //      > check parent of parent (if that even exists)
            //      > if the boards differ, that's already a fail
            //      > otherwise i'll have to compare all available moves
            //        luckily, as they are generated in a deterministic and fixed order, i can just compare them
            // - There has been no capture or pawn move in the last fifty moves (move = both players get to play) by each player, if the last move was not a checkmate (see fifty-move rule).
            //      > this is easily tracked with a counter that resets if any of the things mentioned above do happen
            //      > remember that 50 moves is 100 chessmoves

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

        public static bool CheckIsInbounds (int x, int y) {
            return (x >= 0 && x < BOARD_SIZE && y >= 0 && y < BOARD_SIZE);
        }

        private static bool GetLongBit (long longValue, int index) {
            return ((longValue >> index) & 1) == 1;
        }

        private static long SetLongBit (long longValue, int index, bool value) {
            return (value ? (longValue | (1L << index)) : (longValue & ~(1L << index)));
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
            }
            playerStates[INDEX_WHITE].KingCoord = Array.IndexOf(board, ChessPiece.WhiteKing);
            playerStates[INDEX_BLACK].KingCoord = Array.IndexOf(board, ChessPiece.BlackKing);
            UpdatePlayerStates();
            this.currentPlayerIndex = INDEX_WHITE;
        }

        public string ToPrintableString (bool includeRowAndColumnLabels = true) {
            var sb = new System.Text.StringBuilder();
            for (int y = BOARD_SIZE - 1; y >= 0; y--) {
                if (includeRowAndColumnLabels) {
                    sb.Append($"{y+1}   ");
                }
                for (int x = 0; x < BOARD_SIZE; x++) {
                    sb.Append($"{board[XYToCoord(x, y)].ToShortString()} ");
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

        private IEnumerable<ChessMove> EnumerateLegalMovesForPlayer (int playerIndex) {
            foreach (var coord in CoordsWithPiecesOfPlayer(playerIndex)) {
                foreach (var move in ChessMoveUtils.GetMovesForPiece(this, coord)) {
                    if (!GetResultOfMove(move).playerStates[playerIndex].IsInCheck) {
                        yield return move;
                    }
                }
            }
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

        private IEnumerable<int> CoordsWithPiecesOfPlayer (int playerIndex) {
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

    }

}
