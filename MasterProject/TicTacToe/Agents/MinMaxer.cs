using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterProject.TicTacToe.Agents {

    public class MinMaxer : TTTAgent {

        // uses negamax

        public override Agent Clone () => new MinMaxer();

        public override int GetMoveIndex (TTTGameState gs, IReadOnlyList<TTTMove> moves) {
            var ownIndex = gs.CurrentPlayerIndex;
            var bestMoveIndex = -1;
            var bestScore = float.NegativeInfinity;
            for (int i = 0; i < moves.Count; i++) {
                var newGs = gs.GetResultOfMove(moves[i]);
                var newScore = -GetMoveScoreAllTheWayDown(
                    newGs,
                    (finalGs) => (finalGs.IsDraw ? 0 : ((finalGs.WinnerIndex == ownIndex) ? 1 : -1)),
                    -1
                );
                if (newScore > bestScore) {
                    bestMoveIndex = i;
                    bestScore = newScore;
                }
            }
            return bestMoveIndex;
        }

        static float GetMoveScoreAllTheWayDown (TTTGameState gs, System.Func<TTTGameState, float> evaluate, float multiplier) {
            if (gs.GameOver) {
                return multiplier * evaluate(gs);
            }
            var moves = gs.GetPossibleMovesForCurrentPlayer();
            var maxScore = float.NegativeInfinity;
            foreach (var move in moves) {
                maxScore = Math.Max(maxScore, -GetMoveScoreAllTheWayDown(gs.GetResultOfMove(move), evaluate, -multiplier));
            }
            return maxScore;
        }

    }

}
