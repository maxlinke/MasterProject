using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterProject.GodfieldLight {

    public static class GameStateUtils {

        private static int[] NO_CARDS = new int[0];

        public static IReadOnlyList<PossibleOutcome<GodfieldGameState>> GetPossibleBounceMoveResults (GodfieldGameState gs, GodfieldMove move) {
            var output = new List<PossibleOutcome<GodfieldGameState>>();
            var baseState = gs.CloneWithFirstAttackTargetRemovedAndUsedCardsUnresolved(move.usedCardIndices);
            var bounceResultProbability = 1f / gs.livingPlayerCount;
            for (int i = 0; i < gs.playerStates.Length; i++) {
                if (gs.playerStates[i].health > 0) {
                    var bounceAttack = gs.currentAttack.Redirected(gs.CurrentPlayerIndex, i);
                    var bounceState = baseState.Clone();
                    bounceState.attacks.Insert(0, bounceAttack);
                    bounceState.currentPlayerWasHit = true;
                    output.Add(new PossibleOutcome<GodfieldGameState>() {
                        Probability = bounceResultProbability,
                        GameState = bounceState
                    });
                }
            }
            return output;
        }

        public static IReadOnlyList<PossibleOutcome<GodfieldGameState>> GetReflectResult (GodfieldGameState gs, GodfieldMove move) {
            var outcomeState = gs.CloneWithFirstAttackTargetRemovedAndUsedCardsUnresolved(move.usedCardIndices);
            var reflectedAttack = gs.currentAttack.Redirected(gs.CurrentPlayerIndex, gs.currentAttack.instigatorPlayerIndex);
            outcomeState.attacks.Insert(0, reflectedAttack);
            outcomeState.currentPlayerWasHit = true;
            return PossibleOutcome<GodfieldGameState>.CertainOutcome(outcomeState);
        }

        private static IReadOnlyList<PossibleOutcome<GodfieldGameState>> GetOutcomesOfOngoingAttack (GodfieldGameState gs) {
            var output = new List<PossibleOutcome<GodfieldGameState>>();
            var hitProbability = gs.currentAttack.hitProbability;
            if (hitProbability < 1) {
                var missState = gs.Clone();
                missState.currentPlayerWasHit = false;
                output.Add(new PossibleOutcome<GodfieldGameState>() {
                    Probability = 1f - gs.currentAttack.hitProbability,
                    GameState = missState
                });
            }
            if (hitProbability > 0) {
                var hitState = gs.Clone();
                hitState.currentPlayerWasHit = true;
                output.Add(new PossibleOutcome<GodfieldGameState>() {
                    Probability = hitProbability,
                    GameState = hitState
                });
            }
            return output;
        }

        public static IReadOnlyList<PossibleOutcome<GodfieldGameState>> GetOutcomesOfTakenHit (GodfieldGameState gs, GodfieldMove move) {
            if (move.bounceAttack) {
                // if bounce, that's more possible outcomes with a redirected attack as the first attack
                return GetPossibleBounceMoveResults(gs, move);
            }
            if (move.reflectAttack) {
                // if reflect, that's one possible outcome with a redirected attack as the first attack
                return GetReflectResult(gs, move);
            }
            // otherwise we do armor, check damage, kill if lethal, all that stuff and need to 
            var baseResult = gs.CloneWithFirstAttackTargetRemovedAndUsedCardsUnresolved(move.usedCardIndices);
            var remainingDamage = gs.currentAttack.damage - move.defenseValue;
            if (remainingDamage > 0) {
                if (gs.currentAttack.lethalIfUnblocked) {
                    baseResult.UpdatePlayerHealth(gs.CurrentPlayerIndex, 0);
                } else {
                    var hp = gs.PlayerStates[gs.CurrentPlayerIndex].health;
                    baseResult.UpdatePlayerHealth(gs.CurrentPlayerIndex, hp - remainingDamage);
                }
                baseResult.UpdateGameOver();
            }
            if (baseResult.GameOver || baseResult.currentAttack == null) {
                if (!baseResult.GameOver) {
                    baseResult.turnNumber++;
                    baseResult.currentPlayerWasHit = false;
                }
                // either way, no other probabilities here
                return PossibleOutcome<GodfieldGameState>.CertainOutcome(baseResult);
            }
            // currentattack is not null, so someone is possibly getting hit. or not. those are options.
            return GetOutcomesOfOngoingAttack(baseResult);
        }

        public static IReadOnlyList<PossibleOutcome<GodfieldGameState>> GetOutcomeOfMissedAttack (GodfieldGameState gs) {
            var output = gs.CloneWithFirstAttackTargetRemovedAndUsedCardsUnresolved(NO_CARDS);
            if (output.currentAttack == null) {
                output.turnNumber++;
                output.currentPlayerWasHit = false;
                return PossibleOutcome<GodfieldGameState>.CertainOutcome(output);
            } else {
                return GetOutcomesOfOngoingAttack(output);
            }
        }

        public static IReadOnlyList<PossibleOutcome<GodfieldGameState>> GetAttackMoveOutcomes (GodfieldGameState gs, GodfieldMove move) {
            var output = gs.Clone();
            var ps = output.PlayerStates[gs.CurrentPlayerIndex];
            foreach (var cardIndex in move.usedCardIndices) {
                ps.cards[cardIndex] = Card.Unresolved;
                ps.cardIds[cardIndex] = Card.Unresolved.id;
            }
            output.attacks.Insert(0, move.attack);
            return GetOutcomesOfOngoingAttack(output);
        }

        public static IReadOnlyList<PossibleOutcome<GodfieldGameState>> GetHealMoveResult (GodfieldGameState gs, GodfieldMove move) {
            var output = gs.Clone();
            output.turnNumber++;
            var ps = output.PlayerStates[gs.CurrentPlayerIndex];
            foreach (var cardIndex in move.usedCardIndices) {
                ps.cards[cardIndex] = Card.Unresolved;
                ps.cardIds[cardIndex] = Card.Unresolved.id;
            }
            output.UpdatePlayerHealth(gs.CurrentPlayerIndex, ps.health + move.healValue);
            return PossibleOutcome<GodfieldGameState>.CertainOutcome(output);
        }

        public static IReadOnlyList<PossibleOutcome<GodfieldGameState>> GetDiscardResult (GodfieldGameState gs, GodfieldMove move) {
            var output = gs.Clone();
            output.turnNumber++;
            var ps = output.PlayerStates[gs.CurrentPlayerIndex];
            foreach (var cardIndex in move.usedCardIndices) {
                ps.cards[cardIndex] = Card.Unresolved;
                ps.cardIds[cardIndex] = Card.Unresolved.id;
            }
            return PossibleOutcome<GodfieldGameState>.CertainOutcome(output);
        }

    }

}
