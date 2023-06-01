using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterProject.G44P {

    public class G44PGameState : GameState<G44PGameState, G44PMove, G44PPlayerState> {

        // TODO make tests to see if all of this works
        // start with the basics
        //  - place a piece for a given player and see how many times it moves before it's gone
        //  - test the ranking code, i think i can just set the things
        //  - test the pushing code
        //      - provide a starting board
        //      - compare with outcome board
        //      - also compare scores
        //  - test that the empty move stuff works

        // TODO are games longer with the reduced board size but same win score?
        public const int WIN_SCORE = 44;
        public const int PLAYER_COUNT = 4;
        public const int BOARD_SIZE = 6;        // reduced from the original 8-size board
        public const byte EMPTY_FIELD = 255;
        private static readonly int[][] playerHomeRows;
        private static readonly G44PMove[][] playerMoves;
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
            playerMoves = new G44PMove[PLAYER_COUNT][];
            moveIndexOffsets = new int[PLAYER_COUNT];
            for (int i = 0; i < PLAYER_COUNT; i++) {
                var rightCornerCoords = FieldIndexToCoord(corners[i]);
                var leftCornerCoords = FieldIndexToCoord(corners[(i + 1) % corners.Length]);
                var dx = Math.Sign(leftCornerCoords.x - rightCornerCoords.x);
                var dy = Math.Sign(leftCornerCoords.y - rightCornerCoords.y);
                playerHomeRows[i] = new int[BOARD_SIZE - 2];
                playerMoves[i] = new G44PMove[BOARD_SIZE - 2];
                for (int j = 0; j < playerHomeRows[i].Length; j++) {;
                    playerHomeRows[i][j] = CoordToFieldIndex(
                        rightCornerCoords.x + ((j + 1) * dx),
                        rightCornerCoords.y + ((j + 1) * dy)
                    );
                    playerMoves[i][j] = new G44PMove() { fieldIndex = playerHomeRows[i][j] };
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

        static bool CheckOffsetLeadsOutOfBounds (int fieldIndex, int offset, out int newFieldIndex) {
            newFieldIndex = fieldIndex + offset;
            if (newFieldIndex < 0) {
                return true;
            }
            if (newFieldIndex >= (BOARD_SIZE * BOARD_SIZE)) {
                return true;
            }
            var horizontalMove = Math.Abs(offset) < BOARD_SIZE;
            if (horizontalMove) {
                var onLeftEdge = (fieldIndex % BOARD_SIZE) == 0;
                if (onLeftEdge && offset < 0) {
                    return true;
                }
                var onRightEdge = ((fieldIndex + 1) % BOARD_SIZE) == 0;
                if (onRightEdge && offset > 0) {
                    return true;
                }
            }
            return false;
        }

        public override IReadOnlyList<G44PPlayerState> PlayerStates => playerStates;
        public override int CurrentPlayerIndex => currentPlayerIndex;

        // public for json
        public G44PPlayerState[] playerStates { get; set; }
        public int currentPlayerIndex { get; set; }
        public byte[] board { get; set; }

        private string[] playerNames;   // just to make the printable string output a bit more readable ("player 0...3" is a bit hard to gauge when they're different ais and such)

        public void Initialize (IReadOnlyList<string> inputPlayerNames) {
            currentPlayerIndex = 0;
            playerStates = new G44PPlayerState[PLAYER_COUNT];
            playerNames = new string[PLAYER_COUNT];
            for (int i = 0; i < playerStates.Length; i++) {
                playerStates[i] = new G44PPlayerState() {
                    Points = 0,
                    HasWon = false,
                    HasLost = false,
                    HasDrawn = false
                };
                playerNames[i] = $"Player {i}{((inputPlayerNames == null || i >= inputPlayerNames.Count) ? "" : $" \"{inputPlayerNames[i]}\"")}";
                ;
            }
            board = new byte[BOARD_SIZE * BOARD_SIZE];
            Array.Fill(board, EMPTY_FIELD);
        }

        public G44PGameState Clone () {
            var output = new G44PGameState {
                playerStates = new G44PPlayerState[this.playerStates.Length],
                playerNames = this.playerNames, // no need to make a proper clone of this
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
            return playerMoves[currentPlayerIndex];
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

        public G44PGameState GetResultOfMove (G44PMove move) {
            var clone = this.Clone();
            clone.MovePieces(currentPlayerIndex);
            if (move != default(G44PMove)) {
                clone.PlacePiece(move.fieldIndex, moveIndexOffsets[currentPlayerIndex], currentPlayerIndex);
            }
            clone.RecalculatePlayerRanksAndUpdateWinnerIfApplicable();
            clone.currentPlayerIndex = (this.currentPlayerIndex + 1) % PLAYER_COUNT;
            return clone;
        }

        public void MovePieces (int movingPlayer) {
            var moveIndexOffset = moveIndexOffsets[movingPlayer];
            if (moveIndexOffset > 0) {  // going in reverse order prevents moving pieces twice. it doesn't matter whether the moves are horizontal or vertical in this case, only whether the offset is positive or negative
                for (int i = board.Length-1; i >= 0; i--) {
                    if (board[i] == movingPlayer) {
                        MovePiece(i, moveIndexOffset);
                    }
                }
            } else {
                for (int i = 0; i < board.Length; i++) {
                    if (board[i] == movingPlayer) {
                        MovePiece(i, moveIndexOffset);
                    }
                }
            }
        }

        public void RecalculatePlayerRanksAndUpdateWinnerIfApplicable () {
            var rankScores = new List<int>(PLAYER_COUNT);
            var rankScoreCounters = new List<int>(PLAYER_COUNT);
            foreach (var playerState in PlayerStates) {
                var processed = false;
                for(int i=0; i<rankScores.Count; i++){
                    if (playerState.Points > rankScores[i]) {
                        rankScores.Insert(i, playerState.Points);
                        rankScoreCounters.Insert(i, 1);
                        processed = true;
                        break;
                    } else if (playerState.Points == rankScores[i]) {
                        rankScoreCounters[i]++;
                        processed = true;
                        break;
                    }
                }
                if (!processed) {
                    rankScores.Add(playerState.Points);
                    rankScoreCounters.Add(1);
                }
            }
            var rank = 0;
            for (int i = 0; i < rankScores.Count; i++) {
                foreach (var playerState in PlayerStates) {
                    if (playerState.Points == rankScores[i]) {
                        playerState.Rank = rank;
                    }
                }
                rank += rankScoreCounters[i];
            }
            if (rankScores[0] >= WIN_SCORE) {
                var winnerScore = rankScores[0];
                var winnerCount = rankScoreCounters[0];
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

        public void PlacePiece ((int x, int y) coord, int offset, int playerIndex) {
            PlacePiece(CoordToFieldIndex(coord.x, coord.y), offset, playerIndex);
        }

        public void PlacePiece (int fieldIndex, int offset, int playerIndex) {
            if (board[fieldIndex] != EMPTY_FIELD) {
                if (board[fieldIndex] != playerIndex) {
                    playerStates[playerIndex].Points++;
                }
                MovePiece(fieldIndex, offset);
            }
            board[fieldIndex] = (byte)playerIndex;
        }

        public void MovePiece (int fieldIndex, int offset) {
            var piecePlayer = board[fieldIndex];
            if (piecePlayer == EMPTY_FIELD) {
                var coord = FieldIndexToCoord(fieldIndex);
                throw new InvalidOperationException($"Can't move piece at ({coord.x}, {coord.y}) as there's nothing to move at that location!");
            }
            board[fieldIndex] = EMPTY_FIELD;
            if (!CheckOffsetLeadsOutOfBounds(fieldIndex, offset, out var newFieldIndex)) {
                PlacePiece(newFieldIndex, offset, piecePlayer);
            }
        }

        // piece counting is done this way as it means less work when calculating new game states
        // piece counts are only evaluated by SOME bots at leaf nodes in the game tree anyways
        public int CountNumberOfPiecesByPlayerIndex (int playerIndex) {
            var output = 0;
            for (int i = 0; i < board.Length; i++) {
                if (board[i] == playerIndex) output++;
            }
            return output;
        }

        public string ToPrintableString (bool appendScores) {
            return ToPrintableString(-1, -1, appendScores);
        }

        public string ToPrintableString (int moveMarkPlayer, int moveMarkField, bool appendScores) {
            var sb = new System.Text.StringBuilder();
            var outputLineCount = Math.Max(BOARD_SIZE + 1, (appendScores ? PLAYER_COUNT : 0));
            var sortedPlayers = new List<(G44PPlayerState state, string name)>();
            var longestNameLength = 0;
            for (int i = 0; i < PLAYER_COUNT; i++) {
                sortedPlayers.Add((PlayerStates[i], playerNames[i]));
                longestNameLength = Math.Max(longestNameLength, playerNames[i].Length);
            }
            sortedPlayers.Sort((a, b) => a.state.Rank - b.state.Rank);
            var preRow = GetMarkerArray();
            var postRow = GetMarkerArray();
            var preColumn = GetMarkerArray();
            var postColumn = GetMarkerArray();
            if (moveMarkPlayer != -1 && moveMarkField != -1) {
                var coords = FieldIndexToCoord(moveMarkField);
                switch (moveMarkPlayer) {
                    case 0: preRow[coords.x] =     'v'; break;
                    case 1: postColumn[coords.y] = '<'; break;
                    case 2: postRow[coords.x] =    '^'; break;
                    case 3: preColumn[coords.y] =  '>'; break;
                }
            }
            sb.Append("  ");
            foreach (var c in preRow) sb.Append($"{c} ");
            sb.AppendLine();
            for (int i = 0; i < outputLineCount; i++) {
                if (i < preColumn.Length) sb.Append($"{preColumn[i]} ");
                else sb.Append("  ");
                if (i < BOARD_SIZE) {
                    for (int j = 0; j < BOARD_SIZE; j++) {
                        var fieldIndex = CoordToFieldIndex(j, i);
                        if (board[fieldIndex] == EMPTY_FIELD) {
                            sb.Append("- ");
                        } else {
                            sb.Append($"{board[fieldIndex]} ");
                        }
                    }
                    if (i < postColumn.Length) sb.Append($"{postColumn[i]} ");
                    else sb.Append("  ");
                } else {
                    if (i == BOARD_SIZE) {
                        foreach (var c in postRow) sb.Append($"{c} ");
                    } else {
                        sb.Append(new string(' ', BOARD_SIZE * 2));
                    }
                    sb.Append("  ");
                }
                if (i < PLAYER_COUNT && appendScores) {
                    var player = sortedPlayers[i];
                    sb.Append($"#{player.state.Rank + 1} {player.name}{new string(' ', longestNameLength - player.name.Length)} : {player.state.Points} Pts.");
                }
                sb.AppendLine();
            }
            return sb.ToString();

            char[] GetMarkerArray () {
                var output = new char[BOARD_SIZE];
                for (int i = 0; i < BOARD_SIZE; i++) {
                    output[i] = ' ';
                }
                return output;
            }
        }

    }

}
