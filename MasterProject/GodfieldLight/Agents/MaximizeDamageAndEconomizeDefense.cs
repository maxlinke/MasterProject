using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterProject.GodfieldLight.Agents {

    public class MaximizeDamageAndEconomizeDefense : GodfieldAgent {
        
        public override bool IsStateless => true;

        public override bool IsTournamentEligible => true;

        public override Agent Clone () {
            return new MaximizeDamageAndEconomizeDefense();
        }

        public override int GetMoveIndex (GodfieldGameState gameState, IReadOnlyList<GodfieldMove> moves) {
            var scores = new float[moves.Count];
            if (gameState.currentPlayerWasHit) {
                for (int i = 0; i < moves.Count; i++) {
                    var move = moves[i];
                    if (move.reflectAttack) {
                        // expected damage
                        scores[i] = 0;      // a better player might reflect only high damage moves
                    } else if (move.bounceAttack) {
                        // expected damage
                        scores[i] = gameState.currentAttack.damage / gameState.livingPlayerCount;
                    } else {
                        // expected damage, but punish overdefense. best outcome here is 0
                        scores[i] = -Math.Abs(gameState.currentAttack.damage - move.defenseValue);
                    }
                }
            } else {
                for (int i = 0; i < moves.Count; i++) {
                    var move = moves[i];
                    if (move.attack != null) {
                        scores[i] = 0;
                        foreach (var targetIndex in move.attack.remainingTargetPlayerIndices) {
                            if (move.attack.lethalIfUnblocked) {
                                scores[i] += gameState.playerStates[targetIndex].health * move.attack.hitProbability;
                            } else {
                                scores[i] += move.attack.damage * move.attack.hitProbability;
                            }
                        }
                    } else {
                        scores[i] = move.healValue > 0 ? 1 : 0;
                    }
                }
            }
            return GetIndexOfMaximum(scores, true);
        }

    }

}
