using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace MasterProject.GodfieldLight.Tests {

    [TestFixture]
    public class GeneralTests {

        [Test]
        public void TestThatTheGameHasMoreDamageThanDamagePreventionOrMitigation () {
            var totalOccurrences = 0;
            var damageCardOccurrences = 0;
            var totalDamage = 0f;
            var defenseCardOccurrences = 0;
            var totalDefense = 0;
            var healCardOccurrences = 0;
            var totalHeal = 0;
            foreach (var card in Card.allCards) {
                totalOccurrences += card.occurrences;
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
            var damagePercent = (100f * damageCardOccurrences) / totalOccurrences;
            var defensePercent = (100f * defenseCardOccurrences) / totalOccurrences;
            var healPercent = (100f * healCardOccurrences) / totalOccurrences;
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"There are {Card.allCards.Count} different cards in total");
            sb.AppendLine($"Cards dealing damage appear {damagePercent:F2}% of the time with a total damage of {totalDamage}");
            sb.AppendLine($"Cards blocking damage appear {defensePercent:F2}% of the time with a total defense of {totalDefense}");
            sb.AppendLine($"Cards healing appear {healPercent:F2}% of the time with a total heal of {totalHeal}");
            sb.AppendLine("(Note that these numbers don't neccessarily add up to 100% as cards can serve multiple purposes)");
            Console.WriteLine(sb.ToString());
            var overallDamageBalance = totalDamage - totalDefense - totalHeal;
            Console.WriteLine($"Summing all damage and subtracting defense and heal gives the value \"{overallDamageBalance}\"");
            Assert.Greater(overallDamageBalance, 0);
        }
    }
}
