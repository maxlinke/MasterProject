using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace MasterProject.GodfieldLight.Tests {

    [TestFixture]
    public class DevilsTests {

        [Test]
        public void TestRatiosApproximately () {
            var rng = new System.Random();
            var smallCounter = 0;
            var mediumCounter = 0;
            var largeCounter = 0;
            var fairyCounter = 0;
            var noDevilCounter = 0;
            for (int i = 0; i < 100000; i++) {
                if (Devils.CheckIfDevilAppears(rng, out var id, out var dmg)) {
                    switch (id) {
                        case "SmallDevil":
                            smallCounter++;
                            Assert.AreEqual(Devils.SMALL_DEVIL_DMG, dmg);
                            break;
                        case "MediumDevil":
                            mediumCounter++;
                            Assert.AreEqual(Devils.MEDIUM_DEVIL_DMG, dmg);
                            break;
                        case "LargeDevil":
                            largeCounter++;
                            Assert.AreEqual(Devils.LARGE_DEVIL_DMG, dmg);
                            break;
                        case "CharityFairy":
                            fairyCounter++;
                            Assert.AreEqual(Devils.FAIRY_HEAL, -dmg);
                            break;
                        default:
                            Assert.Fail($"Unknown id \"{id}\"!");
                            break;
                    }
                } else {
                    noDevilCounter++;
                }
            }
            var devilCount = (smallCounter + mediumCounter + largeCounter + fairyCounter);
            var devilChance = devilCount / ((float)(devilCount + noDevilCounter));
            ComparePercentagesRoughly("Devils", devilChance, Devils.DEVIL_CHANCE);
            var occTotal = Devils.SMALL_DEVIL_OCC + Devils.MEDIUM_DEVIL_OCC + Devils.LARGE_DEVIL_OCC + Devils.FAIRY_OCC;
            CompareOccurrance("Small Devil", smallCounter, Devils.SMALL_DEVIL_OCC);
            CompareOccurrance("Medium Devil", mediumCounter, Devils.MEDIUM_DEVIL_OCC);
            CompareOccurrance("Large Devil", largeCounter, Devils.LARGE_DEVIL_OCC);
            CompareOccurrance("Charity Fairy", fairyCounter, Devils.FAIRY_OCC);

            void ComparePercentagesRoughly (string id, float measured, float ideal) {
                Console.WriteLine($"{id} appeared {(measured * 100):F2}% of the time, ideal value is {(ideal * 100):F2}%");
                Assert.LessOrEqual(Math.Abs(ideal - measured), 0.01f);  // 1% deviation is okay
            }

            void CompareOccurrance (string id, int count, int occReference) {
                var measuredPercentage = count / (float)devilCount;
                var idealPercentage = occReference / (float)occTotal;
                ComparePercentagesRoughly(id, measuredPercentage, idealPercentage);
            }
        }

    }

}
