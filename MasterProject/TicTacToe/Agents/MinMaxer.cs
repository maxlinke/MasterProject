using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterProject.TicTacToe.Agents {

    public class MinMaxer : TTTAgent {

        public override void OnGameStarted (TTTGame game) { }

        public override int GetMoveIndex (TTTGame game, IReadOnlyList<TTTMove> moves) {
            var gs = game.GetCurrentGameStateVisibleForAgent(this);
            var ownIndex = gs.CurrentPlayerIndex;
            var bestMoveIndex = -1;
            var bestScore = float.NegativeInfinity;
            for (int i = 0; i < moves.Count; i++) {
                var newGs = gs.GetResultOfMove(moves[i]);
                var newScore = -GetMoveScoreAllTheWayDown(ownIndex, newGs, -1);
                if (newScore > bestScore) {
                    bestMoveIndex = i;
                    bestScore = newScore;
                }
            }
            return bestMoveIndex;
        }

        static float GetMoveScoreAllTheWayDown (int ownIndex, TTTGameState gs, float multiplier) {
            if (gs.GameOver) {
                if (gs.winnerIndex < 0) {
                    return 0;
                }
                return multiplier * ((gs.winnerIndex == ownIndex) ? 1 : -1);
            }
            var moves = gs.GetPossibleMovesForCurrentPlayer();
            var maxScore = float.NegativeInfinity;
            foreach (var move in moves) {
                maxScore = Math.Max(maxScore, -GetMoveScoreAllTheWayDown(ownIndex, gs.GetResultOfMove(move), -multiplier));
            }
            return maxScore;
        }

    }

}
