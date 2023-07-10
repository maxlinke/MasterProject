using System.Collections.Generic;

namespace MasterProject.GodfieldLight {

    public class Card {

        static Card () {
            Unresolved = new Card("Unresolved");
            var total = 0;
            var damageCardOccurrences = 0;
            var totalDamage = 0f;
            var defenseCardOccurrences = 0;
            var totalDefense = 0;
            var healCardOccurrences = 0;
            var totalHeal = 0;
            var proportionalList = new List<Card>();
            foreach (var card in allCards) {
                total += card.occurrences;
                for (int i = 0; i < card.occurrences; i++) {
                    proportionalList.Add(card);
                }
                if (card.attackValue > 0) {
                    damageCardOccurrences += card.occurrences;
                    totalDamage += (card.attackValue * card.hitProbability * card.occurrences);
                }
                if (card.usableWhileDefending) {
                    defenseCardOccurrences += card.occurrences;                 // does count reflect and bounce
                    totalDefense += (card.defenseValue * card.occurrences);     // doesn't count reflect and bounce (because 0 defense value)
                }
                if (card.healValue > 0) {
                    healCardOccurrences += card.occurrences;
                    totalHeal += (card.healValue * card.occurrences);
                }
            }
            occurrenceTotal = total;
            allCardsOccurringProportionallyOften = proportionalList.ToArray();
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"There are {allCards.Count} cards in total");
            sb.AppendLine($"The proportional list contains {allCardsOccurringProportionallyOften.Count} elements");
            sb.AppendLine($"Cards dealing damage appear {damageCardOccurrences} times with a total damage of {totalDamage}");
            sb.AppendLine($"Cards blocking damage appear {defenseCardOccurrences} times with a total defense of {totalDefense}");
            sb.AppendLine($"Cards healing appear {healCardOccurrences} times with a total heal of {totalHeal}");
            Console.WriteLine(sb.ToString());
        }

        public static Card GetRandomCard () => allCardsOccurringProportionallyOften[rng.Next(occurrenceTotal)];

        private static readonly Random rng = new Random();

        private static readonly int occurrenceTotal;

        public static readonly IReadOnlyList<Card> allCardsOccurringProportionallyOften;

        public static readonly IReadOnlyList<Card> allCards = new Card[]{
            // i left out all the elemental weapons and half of the normal/bonusdmg weapons
            SimpleWeapon("Club",            3, 1),  // added up all three clubs for occurrences
            SimpleWeapon("Punch",           5, 3),
            SimpleWeapon("ChainSickle",     7, 5),
            SimpleWeapon("GlaiveClassic",   8, 7),
            SimpleWeapon("PowerHalberd",    7, 9),
            SimpleWeapon("GravityMace",     4, 11),
            SimpleWeapon("SpearInFineView", 4, 13),
            SimpleWeapon("DragonClaws",     3, 15),
            SimpleWeapon("GodSword",        1, 30),
            BonusDamageWeapon("Blowgun",        1, 1),
            BonusDamageWeapon("Boomerang",      1, 3),
            BonusDamageWeapon("WarriorsBow",    1, 5),
            BonusDamageWeapon("UnknownFeather", 1, 7),
            BonusDamageWeapon("SkyHarpoon",     1, 9),  // would also bounce a miracle normally
            BonusDamageWeapon("HorrorWheel",    1, 11),
            BonusDamageWeapon("TopOfCombat",    1, 13),
            BonusDamageWeapon("AngelBow",       1, 15),
            DualPurposeWeapon("SwordShield",     1, 10, 10, false, false),
            DualPurposeWeapon("BounceSword",     7,  5,  0, true,  false),
            DualPurposeWeapon("ReflectingSword", 3, 10,  0, false, true),
            DarknessWeapon("PriPriPricker", 1, 1),
            DarknessWeapon("Cobra",         1, 3),
            DarknessWeapon("KillerFork",    1, 5),
            DarknessWeapon("DeathsScythe",  1, 10),
            ChanceWeapon("Cup",   8, 4,  0.75f),
            ChanceWeapon("Horse", 4, 8,  0.75f),
            ChanceWeapon("Top",   2, 20, 0.25f),
            // i left out all the elemental armor and also higher-level armor
            SimpleArmor("LeatherCap",     10, 1),
            SimpleArmor("LeatherClothes", 10, 2),
            SimpleArmor("IronGauntlet",    8, 3),
            SimpleArmor("IronShield",      8, 4),
            SimpleArmor("IronArmor",       8, 5),
            SimpleArmor("SteelGauntlet",   6, 6),
            SimpleArmor("SteelHelmet",     6, 7),
            SimpleArmor("SteelShield",     6, 8),
            SimpleArmor("GodShield",       1, 30),
            ReflectingArmor("SuperMirror", 1),
            // mana-items aren't needed in this reduced version
            HealItem("SmileDew",     10, 5),
            HealItem("HeartDew",     6, 10),
            HealItem("RomanceWater", 3, 15),
            HealItem("GalaxyGeyser", 1, 20),
        };

        public static readonly Card Unresolved;

        public string id { get; private set; }

        public readonly int occurrences;

        public bool usableInOwnTurn => (attackValue > 0 || healValue > 0);
        public readonly int attackValue;
        public readonly float hitProbability;
        public readonly bool lethalIfUnblocked;
        public readonly bool isBonusDamage;
        public readonly int healValue;

        public bool usableWhileDefending => (defenseValue > 0 || bouncesAttack || reflectsAttack);
        public readonly int defenseValue;
        public readonly bool bouncesAttack;
        public readonly bool reflectsAttack;

        private Card (string id) {
            this.id = id;
        }

        private Card (string id, int occurrences, int attackValue, float hitProbability, bool lethalIfUnblocked, bool isBonusDamage, int healValue, int defenseValue, bool bouncesAttack, bool reflectsAttack) : this(id) {
            this.occurrences = occurrences;
            this.attackValue = attackValue;
            this.hitProbability = hitProbability;
            this.lethalIfUnblocked = lethalIfUnblocked;
            this.isBonusDamage = isBonusDamage;
            this.healValue = healValue;
            this.defenseValue = defenseValue;
            this.bouncesAttack = bouncesAttack;
            this.reflectsAttack = reflectsAttack;
        }

        private static Card SimpleWeapon (string id, int occurrences, int damage) {
            return new Card(
                id: id,
                occurrences: occurrences,
                attackValue: damage,
                hitProbability: 1,
                lethalIfUnblocked: false,
                isBonusDamage: false,
                healValue: 0,
                defenseValue: 0,
                bouncesAttack: false,
                reflectsAttack: false
            );
        }

        private static Card DualPurposeWeapon (string id, int occurrences, int damage, int defense, bool bounce, bool reflect) {
            return new Card(
                id: id,
                occurrences: occurrences,
                attackValue: damage,
                hitProbability: 1,
                lethalIfUnblocked: false,
                isBonusDamage: false,
                healValue: 0,
                defenseValue: defense,
                bouncesAttack: bounce,
                reflectsAttack: reflect
            );
        }

        private static Card BonusDamageWeapon (string id, int occurrences, int damage) {
            return new Card(
                id: id,
                occurrences: occurrences,
                attackValue: damage,
                hitProbability: 1,
                lethalIfUnblocked: false,
                isBonusDamage: true,
                healValue: 0,
                defenseValue: 0,
                bouncesAttack: false,
                reflectsAttack: false
            );
        }

        private static Card DarknessWeapon (string id, int occurrences, int damage) {
            return new Card(
                id: id,
                occurrences: occurrences,
                attackValue: damage,
                hitProbability: 1,
                lethalIfUnblocked: true,
                isBonusDamage: false,
                healValue: 0,
                defenseValue: 0,
                bouncesAttack: false,
                reflectsAttack: false
            );
        }

        private static Card SimpleArmor (string id, int occurrences, int defense) {
            return new Card(
                id: id,
                occurrences: occurrences,
                attackValue: 0,
                hitProbability: 0,
                lethalIfUnblocked: false,
                isBonusDamage: false,
                healValue: 0,
                defenseValue: defense,
                bouncesAttack: false,
                reflectsAttack: false
            );
        }

        private static Card ReflectingArmor (string id, int occurrences) {
            return new Card(
                id: id,
                occurrences: occurrences,
                attackValue: 0,
                hitProbability: 0,
                lethalIfUnblocked: false,
                isBonusDamage: false,
                healValue: 0,
                defenseValue: 0,
                bouncesAttack: false,
                reflectsAttack: true
            );
        }

        private static Card ChanceWeapon (string id, int occurrences, int damage, float hitProbability) {
            return new Card(
                id: id,
                occurrences: occurrences,
                attackValue: damage,
                hitProbability: hitProbability,
                lethalIfUnblocked: false,
                isBonusDamage: false,
                healValue: 0,
                defenseValue: 0,
                bouncesAttack: false,
                reflectsAttack: false
            );
        }

        private static Card HealItem (string id, int occurrences, int healValue) {
            return new Card(
                id: id,
                occurrences: occurrences,
                attackValue: 0,
                hitProbability: 0,
                lethalIfUnblocked: false,
                isBonusDamage: false,
                healValue: healValue,
                defenseValue: 0,
                bouncesAttack: false,
                reflectsAttack: false
            );
        }

    }

}
