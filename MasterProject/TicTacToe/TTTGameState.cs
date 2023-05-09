using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterProject.TicTacToe {
    
    public class TTTGameState : GameState<TTTGameState, TTTMove> {

        static readonly int[][] _lines = new int[][]{
            new int[]{0, 1, 2 },
            new int[]{3, 4, 5 },
            new int[]{6, 7, 8 },
            new int[]{0, 3, 6 },
            new int[]{1, 4, 7 },
            new int[]{2, 5, 8 },
            new int[]{0, 4, 8 },
            new int[]{2, 4, 6 }
        };

        public static IReadOnlyList<IReadOnlyList<int>> lines => _lines;

        public const int BOARD_SIZE = 3;
        public const int BOARD_FIELD_COUNT = BOARD_SIZE * BOARD_SIZE;
        public const int EMPTY_FIELD = -1;
        public const int PLAYER_COUNT = 2;

        private int currentPlayerIndex;
        private bool gameOver;

        public int winnerIndex { get; private set; }
        public int[] board;

        public override int CurrentPlayerIndex => currentPlayerIndex;
        public override bool GameOver => gameOver;
        public bool IsDraw => gameOver && winnerIndex < 0;

        public override TTTGameState GetVisibleGameStateForPlayer (int playerIndex) {
            return this;
        }

        public override IReadOnlyList<TTTMove> GetPossibleMovesForCurrentPlayer () {
            var output = new List<TTTMove>();
            for (int i = 0; i < BOARD_FIELD_COUNT; i++) {
                if (board[i] == EMPTY_FIELD) {
                    output.Add(new TTTMove() { fieldIndex = i });
                }
            }
            return output;
        }

        public override IReadOnlyList<PossibleOutcome<TTTGameState>> GetPossibleOutcomesForMove (TTTMove move) {
            var output = new List<PossibleOutcome<TTTGameState>>();
            output.Add(new PossibleOutcome<TTTGameState>() {
                Probability = 1,
                GameState = this.GetResultOfMove(move)
            });
            return output;
        }

        // 0 1 2
        // 3 4 5
        // 6 7 8

        public void Initialize () {
            board = new int[BOARD_FIELD_COUNT];
            Array.Fill(board, EMPTY_FIELD);
            currentPlayerIndex = 0;
            winnerIndex = -1;
            gameOver = false;
        }

        // this won't work with godfield and other nondeterministic games, as there maybe multiple possible outcomes for a given move and it's not the gamestate's job to decide which one to choose
        public TTTGameState GetResultOfMove (TTTMove move) {
            if (gameOver) {
                throw new Exception("game is already over");
            }
            if (board[move.fieldIndex] != EMPTY_FIELD) {
                throw new Exception("illegal move");
            }
            var output = new TTTGameState();
            output.board = new int[BOARD_FIELD_COUNT];
            Array.Copy(this.board, output.board, BOARD_FIELD_COUNT);
            output.board[move.fieldIndex] = currentPlayerIndex;
            output.CheckBoard();
            output.currentPlayerIndex = (this.currentPlayerIndex + 1) % PLAYER_COUNT;
            return output;
        }

        public void CheckBoard () {
            foreach (var line in lines) {
                if (CheckLine(line, out var newWinner)) {
                    gameOver = true;
                    winnerIndex = newWinner;
                    return;
                }
            }
            if (CheckAllFieldsFilled()) {
                gameOver = true;
                winnerIndex = -1;
            }

            bool CheckLine (IReadOnlyList<int> line, out int winner) {
                var prev = board[line[0]];
                for (int i = 1; i < line.Count; i++) {
                    var curr = board[line[i]];
                    if (curr != prev) {
                        winner = default;
                        return false;
                    }
                }
                winner = prev;
                return winner != EMPTY_FIELD;
            }

            bool CheckAllFieldsFilled () {
                for(int i=0; i<BOARD_FIELD_COUNT; i++) {
                    if (board[i] == EMPTY_FIELD)
                        return false;
                }
                return true;
            }
        }

        public string GetPrintableBoardWithXsAndOs () {
            return GetPrintableBoard((i) => board[i] == EMPTY_FIELD ? ' ' : GetSymbolForPlayer(board[i]));
        }

        public string GetPrintableBoard (System.Func<int, char> getFieldSymbol) {
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < 3; i++) {
                for (int j = 0; j < 3; j++) {
                    var s = getFieldSymbol((3 * i) + j);
                    sb.Append($" {s} ");
                    if (j + 1 < 3) {
                        sb.Append('|');
                    }
                }
                sb.AppendLine();
                if (i + 1 < 3) {
                    sb.AppendLine("---+---+---");
                }
            }
            return sb.ToString();
        }

        public static char GetSymbolForPlayer (int playerIndex) {
            switch (playerIndex) {
                case 0:
                    return 'X';
                case 1:
                    return 'O';
                default:
                    return '?';
            }
        }

    }

}
