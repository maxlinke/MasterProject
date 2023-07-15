using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterProject.GodfieldLight.RatingFunctions {

    public class AvoidHurtingOthers : MaximizeDamage {

        public override float RateMove (GodfieldGameState gameState, GodfieldMove move, IReadOnlyList<int> otherPlayersInOrderOfMostHealthToLeast) {
            var output = base.RateMove(gameState, move, otherPlayersInOrderOfMostHealthToLeast);
            if (move.attack != null) {
                return -output;
            }
            return move.healValue;
        }

    }

}
