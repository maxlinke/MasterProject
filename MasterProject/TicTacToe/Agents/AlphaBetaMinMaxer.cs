using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterProject.TicTacToe.Agents {

    public class AlphaBetaMinMaxer : TTTAgent {

        public override int GetMoveIndex (TTTGameState gs, IReadOnlyList<TTTMove> moves) {
            var ownIndex = gs.CurrentPlayerIndex;
            var bestMoveIndex = -1;
            var bestScore = float.NegativeInfinity;
            for (int i = 0; i < moves.Count; i++) {
                var newGs = gs.GetResultOfMove(moves[i]);
                if (newGs.GameOver && newGs.winnerIndex == gs.CurrentPlayerIndex) {
                    return i;
                }
                var newScore = AlphaBeta(
                    newGs,
                    (finalGs) => (finalGs.IsDraw ? 0 : ((finalGs.winnerIndex == ownIndex) ? 1 : -1)),
                    float.NegativeInfinity,
                    float.PositiveInfinity,
                    false
                );
                if (newScore > bestScore) {
                    bestMoveIndex = i;
                    bestScore = newScore;
                }
            }
            return bestMoveIndex;
        }

        // https://en.wikipedia.org/wiki/Alpha%E2%80%93beta_pruning
        static float AlphaBeta (TTTGameState gs, System.Func<TTTGameState, float> evaluate, float alpha, float beta, bool maximizing) {
            if (gs.GameOver) {
                return evaluate(gs);
            }
            var moves = gs.GetPossibleMovesForCurrentPlayer();
            if (maximizing) {
                var val = float.NegativeInfinity;
                foreach (var move in moves) {
                    var newGs = gs.GetResultOfMove(move);
                    var newVal = AlphaBeta(newGs, evaluate, alpha, beta, false);
                    val = Math.Max(val, newVal);
                    if (val > beta) {
                        break;
                    }
                    alpha = Math.Max(alpha, val);
                }
                return val;
            } else {
                var val = float.PositiveInfinity;
                foreach (var move in moves) {
                    var newGs = gs.GetResultOfMove(move);
                    var newVal = AlphaBeta(newGs, evaluate, alpha, beta, true);
                    val = Math.Min(val, newVal);
                    if (val < alpha) {
                        break;
                    }
                    beta = Math.Min(beta, val);
                }
                return val;
            }
        }

    }

}
