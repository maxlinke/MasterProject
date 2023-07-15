using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterProject.GodfieldLight.RatingFunctions {

    public class SmartDefense : RatingFunction {

        public override float RateMove (GodfieldGameState gameState, GodfieldMove move, IReadOnlyList<int> otherPlayersInOrderOfMostHealthToLeast) {
            var attack = gameState.currentAttack;
            if (attack.lethalIfUnblocked) {
                if (move.reflectAttack) {
                    // reflecting low damage instakill moves is not that great
                    // but starting at damage 5, this is as good as perfect blocking
                    // and if the damage is higher, this becomes a lot better
                    return Math.Max(0, attack.damage - 4);
                }
                if (move.bounceAttack) {
                    // not a smart move
                    // but better than certainly dying
                    return -10;
                }
                if (move.defenseValue < attack.damage) {
                    // death is really bad
                    return -100;
                }
                // perfect blocking returns 1, after that it drops off, just to discourage overblocking
                return 1f - ((move.defenseValue - attack.damage) / 100f);
            }
            if (move.reflectAttack) {
                // if i have no other options, this is valid
                // but other than that, this becomes better the higher the damage of the attack is with a certain threshold
                if (attack.damage < 10) {
                    return 0;
                }
                return 1;
            }
            var currentHealth = gameState.playerStates[gameState.CurrentPlayerIndex].health;
            var healthAfterTakingDamage = currentHealth - attack.damage;
            if (move.bounceAttack) {
                if (healthAfterTakingDamage <= 0) {
                    // this is a bad idea, but if it COULD kill someone else, that makes it less bad
                    if (gameState.livingPlayerCount == 2) {
                        var otherLivingPlayerHealth = -1;
                        for (int i = 0; i < gameState.playerStates.Length; i++) {
                            if (i == gameState.CurrentPlayerIndex) {
                                continue;
                            }
                            otherLivingPlayerHealth = Math.Max(otherLivingPlayerHealth, gameState.playerStates[i].health);
                        }
                        if (otherLivingPlayerHealth <= attack.damage) {
                            return -1;
                        }
                    }
                    return -10;
                }
                // if the attack bounces back to us, we won't die
                // so might as well make use of it
                if (attack.damage < 5) {
                    return 0;
                }
                return 1;
            }
            var healthAfterBlocking = currentHealth - (Math.Max(0, attack.damage - move.defenseValue));
            if (healthAfterBlocking <= 0) {
                // death also sucks here
                return -100;
            }
            // we're not dying from this, so minimize the amount of used armor
            return 1f - (move.defenseValue / 100f);
        }

    }

}
