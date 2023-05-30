using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterProject.G44P {

    public class G44PGameState : GameState<G44PGameState, G44PMove, G44PPlayerState> {

        public const int WIN_SCORE = 44;
        public const int PLAYER_COUNT = 4;
        public const int BOARD_SIZE = 6;        // reduced from the original 8-size board
        public const byte EMPTY_FIELD = 255;
        private static readonly int[][] playerHomeRows;
        private static readonly int[] moveIndexOffsets;

        public static IReadOnlyList<IReadOnlyList<int>> PlayerHomeRows => playerHomeRows;

        static G44PGameState () {
            var corners = new int[]{    // ordered clockwise. this is implied in the other lines below...
                CoordToFieldIndex(0, 0),
                CoordToFieldIndex(BOARD_SIZE - 1, 0),
                CoordToFieldIndex(BOARD_SIZE - 1, BOARD_SIZE - 1),
                CoordToFieldIndex(0, BOARD_SIZE - 1)
            };
            playerHomeRows = new int[PLAYER_COUNT][];
            moveIndexOffsets = new int[PLAYER_COUNT];
            for (int i = 0; i < PLAYER_COUNT; i++) {
                var rightCornerCoords = FieldIndexToCoord(corners[i]);
                var leftCornerCoords = FieldIndexToCoord(corners[(i + 1) % corners.Length]);
                var dx = Math.Sign(leftCornerCoords.x - rightCornerCoords.x);
                var dy = Math.Sign(leftCornerCoords.y - rightCornerCoords.y);
                playerHomeRows[i] = new int[BOARD_SIZE - 2];
                for (int j = 0; j < playerHomeRows[i].Length; j++) {;
                    playerHomeRows[i][j] = CoordToFieldIndex(
                        rightCornerCoords.x + ((j + 1) * dx),
                        rightCornerCoords.y + ((j + 1) * dy)
                    );
                }
                var nextLeftCorner = FieldIndexToCoord(corners[(i + 2) % corners.Length]);
                var moveDx = Math.Sign(nextLeftCorner.x - leftCornerCoords.x);
                var moveDy = Math.Sign(nextLeftCorner.y - leftCornerCoords.y);
                moveIndexOffsets[i] = CoordToFieldIndex(moveDx, moveDy);
            }
        }

        static int CoordToFieldIndex (int x, int y) {
            return (y * BOARD_SIZE) + x;
        }

        static (int x, int y) FieldIndexToCoord (int i) {
            return (i % BOARD_SIZE, i / BOARD_SIZE);
        }

        public override IReadOnlyList<G44PPlayerState> PlayerStates => playerStates;
        public override int CurrentPlayerIndex => currentPlayerIndex;

        // public for json
        public G44PPlayerState[] playerStates { get; set; }
        public int currentPlayerIndex { get; set; }

        public byte[] board;

        public void Initialize () {
            currentPlayerIndex = 0;
            playerStates = new G44PPlayerState[PLAYER_COUNT];
            for (int i = 0; i < playerStates.Length; i++) {
                playerStates[i] = new G44PPlayerState() {
                    Points = 0,
                    HasWon = false,
                    HasLost = false,
                    HasDrawn = false
                };
            }
            board = new byte[BOARD_SIZE * BOARD_SIZE];
            Array.Fill(board, EMPTY_FIELD);
        }

        public G44PGameState Clone () {
            var output = new G44PGameState {
                playerStates = new G44PPlayerState[this.playerStates.Length],
                board = new byte[this.board.Length]
            };
            for (int i = 0; i < output.playerStates.Length; i++) {
                var src = this.playerStates[i];
                output.playerStates[i] = new G44PPlayerState() {
                    Points = src.Points,
                    HasWon = src.HasWon,
                    HasLost = src.HasLost,
                    HasDrawn = src.HasDrawn
                };
            }
            for (int i = 0; i < output.board.Length; i++) {
                output.board[i] = this.board[i];
            }
            return output;
        }

        public override IReadOnlyList<G44PMove> GetPossibleMovesForCurrentPlayer () {
            var output = new List<G44PMove>();
            foreach (var fieldIndex in playerHomeRows[currentPlayerIndex]) {
                if (board[fieldIndex] == EMPTY_FIELD) {
                    output.Add(new G44PMove() { fieldIndex = fieldIndex });
                }
            }
            if (output.Count < 1) {
                output.Add(null);   // gamestate always has to return at least one move
            }
            return output;
        }

        public override IReadOnlyList<PossibleOutcome<G44PGameState>> GetPossibleOutcomesForMove (G44PMove move) {
            return new PossibleOutcome<G44PGameState>[]{
                new PossibleOutcome<G44PGameState>{
                    Probability = 1,
                    GameState = this.GetResultOfMove(move)
                }
            };
        }

        public override G44PGameState GetVisibleGameStateForPlayer (int playerIndex) {
            return this;
        }

        // skip all the rotation code
        // i predict it'll be faster if i just bite the bullet and write a few more lines of code
        // i can also skip all the dq-logic as that was just a consequence of the multiplayer side of things
        // here it's just about the strategy

        public G44PGameState GetResultOfMove (G44PMove? move) {
            var clone = this.Clone();
            if (move != null) {
                clone.PlacePiece(move.fieldIndex, currentPlayerIndex);
            }
            clone.MovePieces(currentPlayerIndex);
            clone.CheckScoresForGameOver();
            clone.currentPlayerIndex = (this.currentPlayerIndex + 1) % PLAYER_COUNT;
            return clone;
        }

        public void MovePieces (int movingPlayer) {
            // TODO
            // how did this work again?
            // start from the base line and push all the way through?
            // do pushed pieces get to move on their own as well?
            // does it go the other way from the other line?
            // does it matter? probably not...
            throw new NotImplementedException();
        }

        public void CheckScoresForGameOver () {
            var winnerCount = 0;
            var winnerScore = WIN_SCORE - 1;
            foreach (var playerState in PlayerStates) {
                if (playerState.Points > winnerScore) {
                    winnerCount = 1;
                    winnerScore = playerState.Points;
                }else if (playerState.Points == winnerScore) {
                    winnerCount ++;
                }
            }
            if (winnerCount > 0) {
                foreach (var playerState in PlayerStates) {
                    if (playerState.Points == winnerScore) {
                        playerState.HasWon = (winnerCount == 1);
                        playerState.HasDrawn = (winnerCount > 1);
                    } else {
                        playerState.HasLost = true;
                    }
                }
            }
        }

        public void PlacePiece ((int x, int y) coord, int playerIndex) {
            PlacePiece(CoordToFieldIndex(coord.x, coord.y), playerIndex);
        }

        // i can use this in the recursive move function
        // where i start the move call with a piece
        // and this call checks if the target tile is empty and if not, it calls move on that tile and afterwards it sets itself where it used to be and removes itself from where it was
        public void PlacePiece (int fieldIndex, int playerIndex) {
            if (board[fieldIndex] != EMPTY_FIELD) {
                var coord = FieldIndexToCoord(fieldIndex);
                throw new InvalidOperationException($"Can't place piece for player \"{playerIndex}\" at ({coord.x}, {coord.y}) as there's already a piece from player \"{board[fieldIndex]}\" at that location!");
            }
            board[fieldIndex] = (byte)playerIndex;
            // could count pieces here
        }

        public void MovePiece (int fieldIndex, int offset) {
            if (board[fieldIndex] == EMPTY_FIELD) {
                var coord = FieldIndexToCoord(fieldIndex);
                throw new InvalidOperationException($"Can't move piece at ({coord.x}, {coord.y}) as there's nothing to move at that location!");
            }
            // recursive loop, manage score in here
            throw new NotImplementedException();
        }

        public string GetPrintableState () {
            var sb = new System.Text.StringBuilder();
            var outputLineCount = Math.Max(BOARD_SIZE, PLAYER_COUNT);
            for (int i = 0; i < outputLineCount; i++) {
                if (i < BOARD_SIZE) {
                    for (int j = 0; j < BOARD_SIZE; j++) {
                        var fieldIndex = CoordToFieldIndex(j, i);
                        if (board[fieldIndex] == EMPTY_FIELD) {
                            sb.Append("- ");
                        } else {
                            sb.Append($"{board[fieldIndex]} ");
                        }
                    }
                } else {
                    sb.Append(new string(' ', BOARD_SIZE * 2));
                }
                if (i < PLAYER_COUNT) {
                    sb.Append($"   Player {i}: {playerStates[i].Points}");
                }
                sb.AppendLine();
            }
            // TODO
            // scores (and dq)
            // board (use the coordtoindex lookup)
            return sb.ToString();
        }

    }

}
