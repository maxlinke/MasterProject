using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterProject.GodfieldLight.Agents {

    public class ParametrizedAgent : GodfieldAgent {

        private readonly RatingFunction offensiveFunction;
        private readonly RatingFunction defensiveFunction;

        public ParametrizedAgent (RatingFunction offensiveFunction, RatingFunction defensiveFunction) {
            this.offensiveFunction = offensiveFunction;
            this.defensiveFunction = defensiveFunction;
        }

        public override string Id => $"{base.Id}<{offensiveFunction.Id}, {defensiveFunction.Id}>";

        public override bool IsStateless => true;

        public override bool IsTournamentEligible => true;

        public override Agent Clone () {
            return new ParametrizedAgent(offensiveFunction, defensiveFunction);
        }

        public override int GetMoveIndex (GodfieldGameState gameState, IReadOnlyList<GodfieldMove> moves) {
            var playersByHealth = new List<GodfieldPlayerState>(gameState.playerStates);
            playersByHealth.Sort((a, b) => (Math.Sign(b.health - a.health)));
            var otherPlayersByHealth = playersByHealth.Where(p => p.index != gameState.CurrentPlayerIndex).Select(p => p.index).ToArray();
            var scores = new float[moves.Count];
            if (gameState.currentPlayerWasHit) {
                for (int i = 0; i < moves.Count; i++) {
                    scores[i] = defensiveFunction.RateMove(gameState, moves[i], otherPlayersByHealth);
                }
            } else {
                for (int i = 0; i < moves.Count; i++) {
                    scores[i] = offensiveFunction.RateMove(gameState, moves[i], otherPlayersByHealth);
                }
            }
            return GetIndexOfMaximum(scores, true);
        }
    }

}
