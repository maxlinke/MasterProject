namespace MasterProject.GodfieldLight {

    public class GodfieldMove {

        public int[] usedCardIndices { get; set; }
        public Attack? attack { get; set; }
        public int defenseValue { get; set; }
        public bool bounceAttack { get; set; }
        public bool reflectAttack { get; set; }
        public int healValue { get; set; }

        public static GodfieldMove EmptyMove () {
            return new GodfieldMove() {
                usedCardIndices = new int[0],
                attack = null,
                defenseValue = 0,
                bounceAttack = false,
                reflectAttack = false,
                healValue = 0,
            };
        }

        public static GodfieldMove AttackMove (GodfieldPlayerState ps, int cardIndex, int targetPlayerIndex) {
            return AttackMove(ps, cardIndex, new int[] { targetPlayerIndex });
        }

        public static GodfieldMove AttackMove (GodfieldPlayerState ps, int cardIndex, IReadOnlyList<int> targetPlayerIndices) {
            var card = ps.cards[cardIndex];
            return new GodfieldMove() {
                usedCardIndices = new int[] { cardIndex },
                attack = new Attack() {
                    instigatorPlayerIndex = ps.index,
                    damage = card.attackValue,
                    hitProbability = card.hitProbability,
                    lethalIfUnblocked = card.lethalIfUnblocked,
                    remainingTargetPlayerIndices = targetPlayerIndices
                },
                defenseValue = 0,
                bounceAttack = false,
                reflectAttack = false,
                healValue = 0,
            };
        }

        public static GodfieldMove AttackMove (GodfieldPlayerState ps, int[] cardIndices, int targetPlayerIndex) {
            var totalDamage = 0;
            foreach (var index in cardIndices) {
                totalDamage += ps.cards[index].attackValue;
            }
            return new GodfieldMove() {
                usedCardIndices = cardIndices,
                attack = new Attack() {
                    instigatorPlayerIndex = ps.index,
                    damage = totalDamage,
                    hitProbability = 1,
                    lethalIfUnblocked = false,
                    remainingTargetPlayerIndices = new int[] { targetPlayerIndex },
                },
                defenseValue = 0,
                bounceAttack = false,
                reflectAttack = false,
                healValue = 0,
            };
        }

        public static GodfieldMove DefensiveMove (GodfieldPlayerState ps, int cardIndex) {
            var card = ps.cards[cardIndex];
            return new GodfieldMove() {
                usedCardIndices = new int[] { cardIndex },
                attack = null,
                defenseValue = card.defenseValue,
                bounceAttack = card.bouncesAttack,
                reflectAttack = card.reflectsAttack,
                healValue = 0,
            };
        }

        public static GodfieldMove DefensiveMove (GodfieldPlayerState ps, IEnumerable<int> cardIndices) {
            var totalDefense = 0;
            foreach (var i in cardIndices) {
                totalDefense += ps.cards[i].defenseValue;
            }
            return new GodfieldMove() {
                usedCardIndices = cardIndices.ToArray(),
                attack = null,
                defenseValue = totalDefense,
                bounceAttack = false,
                reflectAttack = false,
                healValue = 0
            };
        }

        public static GodfieldMove HealMove (GodfieldPlayerState ps, int cardIndex) {
            return new GodfieldMove() {
                usedCardIndices = new int[] { cardIndex },
                attack = null,
                defenseValue = 0,
                bounceAttack = false,
                reflectAttack = false,
                healValue = ps.cards[cardIndex].healValue,
            };
        }
    
    }

}
