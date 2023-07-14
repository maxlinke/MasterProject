using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterProject.GodfieldLight.RatingFunctions {
    
    public class MaximizeDamageAgainstStrongest : MaximizeDamage {

        public override float RateMove (GodfieldGameState gameState, GodfieldMove move, IReadOnlyList<int> otherPlayersInOrderOfMostHealthToLeast) {
            var output = base.RateMove(gameState, move, otherPlayersInOrderOfMostHealthToLeast);
            if (output > 0) {
                var strongestPlayerIndex = otherPlayersInOrderOfMostHealthToLeast[0];
                if (!move.attack.remainingTargetPlayerIndices.Contains(strongestPlayerIndex)) {
                    return 0;
                }
            }
            return output;
        }

    }

}
