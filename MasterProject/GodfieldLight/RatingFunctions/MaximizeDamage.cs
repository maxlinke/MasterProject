using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterProject.GodfieldLight.RatingFunctions {

    public class MaximizeDamage : RatingFunction {

        public override float RateMove (GodfieldGameState gameState, GodfieldMove move, IReadOnlyList<int> otherPlayersInOrderOfMostHealthToLeast) {
            if (move.attack == null) {
                return 0;
            }
            var output = 0f;
            foreach (var targetIndex in move.attack.remainingTargetPlayerIndices) {
                if (move.attack.lethalIfUnblocked) {
                    output += gameState.playerStates[targetIndex].health * move.attack.hitProbability;
                } else {
                    output += move.attack.damage * move.attack.hitProbability;
                }
            }
            return output;
        }

    }

}
