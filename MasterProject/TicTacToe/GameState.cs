using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterProject.TicTacToe {
    
    public class GameState {

        public const int BOARD_SIZE = 3;
        public const int BOARD_FIELD_COUNT = BOARD_SIZE * BOARD_SIZE;
        public const int EMPTY_FIELD = -1;
        public const int PLAYER_COUNT = 2;

        public int currentPlayerIndex;
        public int winnerIndex;
        public bool gameOver;
        public int[] board;

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
        public GameState ApplyMove (Move move) {
            if (gameOver) {
                throw new System.Exception("game is already over");
            }
            if (board[move.fieldIndex] != EMPTY_FIELD) {
                throw new System.Exception("illegal move");
            }
            var output = new GameState();
            output.board = new int[BOARD_FIELD_COUNT];
            Array.Copy(this.board, output.board, BOARD_FIELD_COUNT);
            output.board[move.fieldIndex] = currentPlayerIndex;
            output.CheckBoard();
            output.currentPlayerIndex = (this.currentPlayerIndex + 1) % PLAYER_COUNT;
            return output;
        }

        // TODO test this

        void CheckBoard () {
            int newWinner;
            if(CheckStraights(out newWinner, (i, j) => ((i * BOARD_SIZE) + j))) {       // straights in one direction
                gameOver = true;
                winnerIndex = newWinner;
                return;
            }
            if (CheckStraights(out newWinner, (i, j) => (i + (j * BOARD_SIZE)))) {      // straights in the other direction
                gameOver = true;
                winnerIndex = newWinner;
                return;
            }
            if(CheckDiagonal(out newWinner, (i) => (i * (BOARD_SIZE + 1)))) {   // one diagonal
                gameOver = true;
                winnerIndex = newWinner;
                return;
            }
            if (CheckDiagonal(out newWinner, (i) => (2 + i * (BOARD_SIZE - 1)))) {   // the other diagonal
                gameOver = true;
                winnerIndex = newWinner;
                return;
            }
            if (CheckAllFieldsFilled()) {   // draw
                gameOver = true;
                winnerIndex = -1;
            }

            bool CheckStraights (out int winner, System.Func<int, int, int> getPos) {
                for (int i = 0; i < BOARD_SIZE; i++) {
                    int? latest = null;
                    var matching = true;
                    for (int j = 0; j < BOARD_FIELD_COUNT; j++) {
                        var pos = getPos(i, j);
                        var curr = board[pos];
                        matching &= ((latest ?? curr) == curr);
                        latest = curr;
                    }
                    if (matching && latest.Value != EMPTY_FIELD) {
                        winner = latest.Value;
                        return true;
                    }
                }
                winner = -1;
                return false;
            }

            bool CheckDiagonal (out int winner, System.Func<int, int> getPos) {
                int? latest = null;
                var matching = true;
                for(int i=0; i<BOARD_SIZE; i++) {
                    var pos = getPos(i);
                    var curr = board[pos];
                    matching &= ((latest ?? curr) == curr);
                    latest = curr;
                }
                if(matching && latest.Value != EMPTY_FIELD) {
                    winner = latest.Value;
                    return true;
                }
                winner = -1;
                return false;
            }

            bool CheckAllFieldsFilled () {
                for(int i=0; i<BOARD_FIELD_COUNT; i++) {
                    if (board[i] == EMPTY_FIELD)
                        return false;
                }
                return true;
            }
        }

    }

}
