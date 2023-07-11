using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace MasterProject.GodfieldLight.Tests {

    [TestFixture]
    public class MoveGenerationTests {

        [Test]
        public void TestPurelyDefenseMoves () {
            var gs = new GodfieldGameState();
            gs.Initialize(2);
            var p0Cards = gs.playerStates[0].cards;
            var firstWeapon = Card.allCards.First(card => card.id == "Club");
            var firstArmor = Card.allCards.First(card => card.id == "LeatherCap");
            for (int i = 0; i < p0Cards.Count + 1; i++) {
                var armorCount = 0;
                for (int j = 0; j < p0Cards.Count; j++) {
                    if (j < i) {
                        p0Cards[j] = firstWeapon;
                    } else {
                        p0Cards[j] = firstArmor;
                        armorCount++;
                    }
                }
                Console.WriteLine($"With {armorCount} pieces of armor:");
                var moves = MoveUtils.GetDefensiveMovesForCurrentPlayer(gs).ToArray();
                var sb = new System.Text.StringBuilder();
                foreach (var move in moves) {
                    if (move.usedCardIndices.Length < 1) {
                        sb.Append(" - take the hit");
                    } else {
                        sb.Append($" - defend for {move.defenseValue} using cards ");
                        foreach (var index in move.usedCardIndices) {
                            sb.Append($"{index}, ");
                        }
                    }
                    Console.WriteLine(sb.ToString());
                    sb.Clear();
                }
                Assert.AreEqual(1 << armorCount, moves.Length);
                Console.WriteLine();
            }
        }

        class DefenseVariation {
            public Card[] cards;
            public int targetMoveCount;
        }

        [Test]
        public void TestWeirdDefenseMoves () {
            var gs = new GodfieldGameState();
            gs.Initialize(2);
            var p0Cards = gs.playerStates[0].cards;
            var firstWeapon = Card.allCards.First(card => card.id == "Club");
            var firstArmor = Card.allCards.First(card => card.id == "LeatherCap");
            var swordShield = Card.allCards.First(card => card.id == "SwordShield");
            var bounceSword = Card.allCards.First(card => card.id == "BounceSword");
            var superMirror = Card.allCards.First(card => card.id == "SuperMirror");
            var variations = new DefenseVariation[]{
                new DefenseVariation(){
                    cards = new Card[]{
                        firstArmor, firstArmor, firstArmor
                    },
                    targetMoveCount = 1 << 3
                },
                new DefenseVariation(){
                    cards = new Card[]{
                        swordShield, swordShield, swordShield
                    },
                    targetMoveCount = 1 << 3
                },
                new DefenseVariation(){
                    cards = new Card[]{
                        bounceSword, bounceSword, bounceSword
                    },
                    targetMoveCount = 4
                },
                new DefenseVariation(){
                    cards = new Card[]{
                        superMirror, superMirror, superMirror
                    },
                    targetMoveCount = 4
                },
            };
            var sb = new System.Text.StringBuilder();
            foreach (var variation in variations) {
                for (int i = 0; i < p0Cards.Count; i++) {
                    if (i < variation.cards.Length) {
                        p0Cards[i] = variation.cards[i];
                    } else {
                        p0Cards[i] = firstWeapon;
                    }
                }
                sb.Append($"with these cards: ");
                foreach (var card in p0Cards) {
                    sb.Append($"{card.id}, ");
                }
                sb.AppendLine();
                var moves = MoveUtils.GetDefensiveMovesForCurrentPlayer(gs);
                foreach (var move in moves) {
                    if (move.usedCardIndices.Length < 1) {
                        sb.Append(" - take the hit");
                    } else {
                        sb.Append($" - defend for {move.defenseValue} using cards ");
                        foreach (var index in move.usedCardIndices) {
                            sb.Append($"{index}, ");
                        }
                    }
                    sb.AppendLine();
                    Assert.AreEqual(variation.targetMoveCount, moves.Count());
                }
                Console.WriteLine(sb.ToString());
                sb.Clear();
            }
        }

    }

}
