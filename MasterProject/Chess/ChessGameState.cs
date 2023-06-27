using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            output.board = (ChessPiece[])(this.board.Clone());
            output.currentPlayerIndex = (this.currentPlayerIndex + 1) % PLAYER_COUNT;
            var srcCoord = XYToCoord(move.sourceX, move.sourceY);
            var dstCoord = XYToCoord(move.destinationX, move.destinationY);
            var srcPiece = board[srcCoord];
            if (srcPiece == ChessPiece.None) {
                throw new System.InvalidOperationException($"Can't move nothing (field {(char)('a' + move.sourceX)}{move.sourceY} is empty)");
            }
            output.board[srcCoord] = ChessPiece.None;
            output.SetPositionHasBeenMoved(srcCoord, true);
            if (move.promoteTo != ChessPiece.None) {
                output.board[dstCoord] = move.promoteTo;
            } else {
                // i need to detect things like castling and en-passant and stuff HERE
                // and act on that
                // because a move is just (xy) -> (xy)
                // also the rules for en-passant are more complicated than i thought
                // but i don't actually need to implement any additional tracking here
                // i can just check the previous gamestate
                // but man...
                // aaaaaa
                // am i actually going to implement all of chess?
                // i'll just start off with the basic rules
                // how the pieces move
                // check and checkmate
                // i can add on the fancier rules later
                output.board[dstCoord] = srcPiece;
            }
            output.playerStates = new ChessPlayerState[this.playerStates.Length];
            for (int i = 0; i < PLAYER_COUNT; i++) {
                output.playerStates[i] = this.playerStates[i].Clone();
            }
            // at this point we have merely moved our pieces and have a new gamestate that is otherwise identical (current player and all)
            // so here is the place to check for dead positions (immediate draws)
            // and checks
            if (output.DetermineIfBoardIsDeadPosition()) {
                output.SetDraw();
                return output;
            }
            output.UpdatePlayerCheckStates();
            if (output.playerStates[output.currentPlayerIndex].IsInCheck) {
                if (!output.AnyLegalMovesForPlayer(output.currentPlayerIndex)) {
                    output.SetVictoryForPlayer(this.currentPlayerIndex);
                }
            }
            return output;
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

        void UpdatePlayerCheckStates () {
            for (int i = 0; i < PLAYER_COUNT; i++) {
                playerStates[i].IsInCheck = DetermineIfPlayerChecksOther((i + 1) % PLAYER_COUNT);
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

        public void SetPositionHasBeenMoved (int x, int y, bool value) {
            SetPositionHasBeenMoved(XYToCoord(x, y), value);
        }

        public void SetPositionHasBeenMoved (int coord, bool value) {
            if (value) {
                pieceHasMoved |= (1l << coord);
            } else {
                pieceHasMoved &= ~(1l << coord);
            }
        }

        public bool GetPositionHasBeenMoved (int x, int y) {
            var coord = XYToCoord(x, y);
            return ((pieceHasMoved >> coord) & 1) == 1;
        }

        // TODO test this
        public int CountTotalPiecesOfColor (int colorId) {
            var output = 0;
            for (int i = 0; i < board.Length; i++) {
                if ((((int)(board[i])) & colorId) == colorId) {
                    output++;
                }
            }
            return output;
        }

        // TODO test this
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
            this.board = new ChessPiece[BOARD_SIZE * BOARD_SIZE];
            this.pieceHasMoved = -1;    // everywhere has been moved
            PlacePiecesOnBoard();
            this.playerStates = new ChessPlayerState[PLAYER_COUNT];
            for (int i = 0; i < PLAYER_COUNT; i++) {
                playerStates[i] = new ChessPlayerState();   // TODO any setup required? 
            }
            this.currentPlayerIndex = INDEX_WHITE;

            void PlacePiecesOnBoard () {
                for (int i = 0; i < BOARD_SIZE; i++) {
                    SetPiecesVerticallySymmetric(i, 1, ChessPieceUtils.ID_PAWN);
                }
                for (int i = 0; i < 3; i++) {
                    var pieceId = i switch {
                        0 => ChessPieceUtils.ID_ROOK,
                        1 => ChessPieceUtils.ID_KNIGHT,
                        2 => ChessPieceUtils.ID_BISHOP
                    };
                    SetPiecesVerticallySymmetric(i, 0, pieceId);
                    SetPiecesVerticallySymmetric(BOARD_SIZE - 1 - i, 0, pieceId);
                }
                SetPiecesVerticallySymmetric(3, 0, ChessPieceUtils.ID_QUEEN);
                SetPiecesVerticallySymmetric(4, 0, ChessPieceUtils.ID_KING);

                void SetPiecesVerticallySymmetric (int x, int localY, int rawPieceId) {
                    var wCoord = XYToCoord(x, localY);
                    board[wCoord] = (ChessPiece)(ChessPieceUtils.ID_WHITE | rawPieceId);
                    SetPositionHasBeenMoved(wCoord, false);
                    var bCoord = XYToCoord(x, BOARD_SIZE - 1 - localY);
                    board[bCoord] = (ChessPiece)(ChessPieceUtils.ID_BLACK | rawPieceId);
                    SetPositionHasBeenMoved(bCoord, false);
                }
            }
        }

        public string ToPrintableString () {
            var sb = new System.Text.StringBuilder();
            for (int y = BOARD_SIZE - 1; y >= 0; y--) {
                sb.Append($"{y}   ");
                for (int x = 0; x < BOARD_SIZE; x++) {
                    sb.Append($"{board[XYToCoord(x, y)].ToShortString()} ");
                }
                sb.Append("\n");
            }
            sb.Append("\n");
            sb.Append($"    ");
            for (int x = 0; x < BOARD_SIZE; x++) {
                sb.Append($"{(char)('a' + x)} ");
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
            var evaluateResultForCheck = playerStates[playerIndex].IsInCheck;
            foreach (var coord in CoordsWithPiecesOfPlayer(playerIndex)) {
                foreach (var move in ChessMoveUtils.GetRegularMovesForPiece(this, coord)) {
                    if (!move.checksKing) {
                        if (evaluateResultForCheck) {
                            var result = GetResultOfMove(move);
                            if (!result.GameOver && result.playerStates[playerIndex].IsInCheck) {
                                continue;
                            }
                        }
                        yield return move;
                    }
                }
            }
        }

        private bool DetermineIfPlayerChecksOther (int playerIndex) {
            foreach (var coord in CoordsWithPiecesOfPlayer(playerIndex)) {
                foreach (var move in ChessMoveUtils.GetRegularMovesForPiece(this, coord)) {
                    if (move.checksKing) {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool DetermineIfBoardIsDeadPosition () {
            // TODO
            // king against king
            //  > easy, 0 "other" pieces and 0 knights and bishops in total
            // king against king and bishop
            //  > easy, 0 "other" pieces and 1 bishop in total
            // king against king and knight
            //  > easy, 0 "other" pieces and 1 knight in total
            // king and bishop against king and bishop, with both bishops on squares of the same color (see King and two bishops)
            //  > not so easy
            //  > 0 other pieces (that's the quick lockout condition)
            //  > 0 knights in total
            //  > 1 bishop each, cache bishop coord
            //  > i can return early if any of the above conditions are broken
            //  > so i don't need an "other pieces counter"
            throw new System.NotImplementedException();
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

        //private IReadOnlyList<ChessMove> CalculatePossibleMovesForCurrentPlayer () {
        //    var moves = new List<ChessMove>();
        //    foreach (var coord in CoordsWithPiecesOfPlayer(CurrentPlayerIndex)) {
        //        moves.AddRange(ChessMoveUtils.GetRegularMovesForPiece(this, coord));
        //    }
        //    if (playerStates[currentPlayerIndex].IsInCheck) {
        //        for (int i = moves.Count - 1; i >= 0; i--) {
        //            var newGs = this.GetResultOfMove(moves[i]);
        //            if (newGs.PlayerStates[currentPlayerIndex].IsInCheck) {
        //                moves.RemoveAt(i);
        //            }
        //        }
        //    }
        //    return moves;
        //}

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
