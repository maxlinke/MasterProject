using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterProject.GodfieldLight.RatingFunctions {

    public class EconomizeDefense : RatingFunction {

        // TODO for "stronger" (?) variants
        // - bounce/reflect primarily strong attacks
        // - when hp is high, become more likely to just take the damage if the damage is low

        public override float RateMove (GodfieldGameState gameState, GodfieldMove move, IReadOnlyList<int> otherPlayersInOrderOfMostHealthToLeast) {
            if (move.reflectAttack) {
                // expected damage
                return 0;
            } else if (move.bounceAttack) {
                // expected damage
                return gameState.currentAttack.damage / gameState.livingPlayerCount;
            } else {
                // expected damage, but punish overdefense. best outcome here is 0
                return -Math.Abs(gameState.currentAttack.damage - move.defenseValue);
            }
        }

    }

}
