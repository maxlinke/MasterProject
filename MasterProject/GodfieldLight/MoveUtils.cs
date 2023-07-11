using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterProject.GodfieldLight {

    public static class MoveUtils {

        public static IEnumerable<GodfieldMove> GetOwnTurnMovesForCurrentPlayer (GodfieldGameState gs) {
            var ps = gs.playerStates[gs.CurrentPlayerIndex];
            var bonusWeaponIndices = new List<int>();
            var otherPlayerIndices = new List<int>();
            for (int i = 0; i < gs.playerStates.Length; i++) {
                if (i != gs.CurrentPlayerIndex && gs.PlayerStates[i].health > 0) {
                    otherPlayerIndices.Add(i);
                }
            }
            for (int i = 0; i < ps.cards.Count; i++) {
                var card = ps.cards[i];
                yield return GodfieldMove.DiscardMove(i);
                if (card.isBonusDamage) {
                    bonusWeaponIndices.Add(i);
                }
            }
            var bonusWeaponPermutations = BinaryPermutationUtils.GetBinaryPermutations(bonusWeaponIndices);
            for (int i = 0; i < ps.cards.Count; i++) {
                var card = ps.cards[i];
                if (card.attackValue > 0 && !card.isBonusDamage) {  // purely bonus based attacks are processed further below
                    if (card.hitProbability < 1) {
                        yield return GodfieldMove.AttackMove(ps, i, otherPlayerIndices);
                    } else {
                        foreach (var otherPlayerIndex in otherPlayerIndices) {
                            yield return GodfieldMove.AttackMove(ps, i, otherPlayerIndex);
                            if (card.canStackBonusDamage) {
                                foreach (var permutation in bonusWeaponPermutations) {
                                    if (permutation.Count > 0) {
                                        var allCardIndices = new int[1 + permutation.Count];
                                        allCardIndices[0] = i;
                                        for (int j = 0; j < permutation.Count; j++) {
                                            allCardIndices[1+j] = permutation[j];
                                        }
                                        yield return GodfieldMove.AttackMove(ps, allCardIndices, otherPlayerIndex);
                                    }
                                }
                            }
                        }
                    }
                }
                if (card.healValue > 0) {
                    yield return GodfieldMove.HealMove(ps, i);
                }
            }
            // doing the bonus-only-attacks separately saves having to do "contains" checks when iterating over the cards above
            // yes, the list would be a bit nicer, but eh...
            foreach (var otherPlayerIndex in otherPlayerIndices) {
                foreach (var permutation in bonusWeaponPermutations) {
                    if (permutation.Count > 0) {
                        yield return GodfieldMove.AttackMove(ps, permutation.ToArray(), otherPlayerIndex);
                    }
                }
            }
        }

        public static IEnumerable<GodfieldMove> GetDefensiveMovesForCurrentPlayer (GodfieldGameState gs) {
            yield return GodfieldMove.EmptyMove();  // just taking the damage is always an option
            var ps = gs.playerStates[gs.CurrentPlayerIndex];
            var defenseOnlyCardIndices = new List<int>();
            for (int i = 0; i < ps.cards.Count; i++) {
                var mainCard = ps.cards[i];
                if (mainCard.usableWhileDefending) {
                    if (!mainCard.bouncesAttack && !mainCard.reflectsAttack) {
                        defenseOnlyCardIndices.Add(i);
                    } else {
                        yield return GodfieldMove.DefensiveMove(ps, i);
                    }
                }
            }
            var defenseOnlyCardIndexPermutations = BinaryPermutationUtils.GetBinaryPermutations(defenseOnlyCardIndices);
            foreach (var permutation in defenseOnlyCardIndexPermutations) {
                if (permutation.Count > 0) {
                    yield return GodfieldMove.DefensiveMove(ps, permutation);
                }
            }
        }

    }

}
