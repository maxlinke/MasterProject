using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterProject.GodfieldLight.RatingFunctions {

    public class MaximizeDamageAgainstWeakest : MaximizeDamage {

        public override float RateMove (GodfieldGameState gameState, GodfieldMove move, IReadOnlyList<int> otherPlayersInOrderOfMostHealthToLeast) {
            var output = base.RateMove(gameState, move, otherPlayersInOrderOfMostHealthToLeast);
            if (output > 0) {
                var weakestPlayerIndex = otherPlayersInOrderOfMostHealthToLeast[otherPlayersInOrderOfMostHealthToLeast.Count - 1];
                if (!move.attack.remainingTargetPlayerIndices.Contains(weakestPlayerIndex)) {
                    return 0;
                }
            }
            return output;
        }

    }

}
