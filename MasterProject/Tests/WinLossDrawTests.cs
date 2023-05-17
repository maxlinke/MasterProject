using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace MasterProject.Tests {

    [TestFixture]
    public class WinLossDrawTests {

        static WinLossDrawRecord GetRecordA () {
            var output = WinLossDrawRecord.New(new string[]{
                "Peter",
                "Bob"
            }, 2);
            for (int i = 0; i < 3; i++)
                output.RecordWin(new string[] { "Peter", "Bob" }, 0);   // 3 wins for peter
            for (int i = 0; i < 1; i++)
                output.RecordWin(new string[] { "Peter", "Bob" }, 1);   // 1 win for bob
            for (int i = 0; i < 1; i++)
                output.RecordWin(new string[] { "Bob", "Peter" }, 0);   // 1 win for bob
            for (int i = 0; i < 2; i++)
                output.RecordWin(new string[] { "Bob", "Peter" }, 1);   // 2 wins for peter
            for (int i = 0; i < 1; i++)
                output.RecordDraw(new string[] { "Peter", "Bob" });     // 1 draw for both
            return output;
        }

        static WinLossDrawRecord GetRecordB () {
            var output = WinLossDrawRecord.New(new string[]{
                "Jim",
                "Mary",
                "Bob",
                "Peter"
            }, 2);
            for (int i = 0; i < 6; i++)
                output.RecordWin(new string[] { "Peter", "Bob" }, 0);   // 6 wins for peter
            for (int i = 0; i < 2; i++)
                output.RecordWin(new string[] { "Peter", "Jim" }, 0);   // 2 wins for peter
            for (int i = 0; i < 1; i++)
                output.RecordWin(new string[] { "Peter", "Jim" }, 1);   // 1 win for jim
            for (int i = 0; i < 2; i++)
                output.RecordWin(new string[] { "Peter", "Peter" }, 0); // 2 wins + losses for peter
            for (int i = 0; i < 1; i++)
                output.RecordWin(new string[] { "Peter", "Peter" }, 1); // 1 win + loss for peter
            for (int i = 0; i < 3; i++)
                output.RecordDraw(new string[] { "Peter", "Peter" });   // 6 draws for peter
            for (int i = 0; i < 1; i++)
                output.RecordDraw(new string[] { "Jim", "Mary" });      // 1 draw for jim and mary
            return output;
        }

        [Test]
        public void TestWinTotalRedundancy () {
            var a = GetRecordA();
            var b = GetRecordB();
            var c = WinLossDrawRecord.Merge(a, b);
            var peterA = Array.IndexOf(a.playerIds, "Peter");
            var peterB = Array.IndexOf(b.playerIds, "Peter");
            var peterC = Array.IndexOf(c.playerIds, "Peter");
            TallyUpResults(a, "Peter", out var peterAWins, out _, out _);
            TallyUpResults(b, "Peter", out var peterBWins, out _, out _);
            TallyUpResults(c, "Peter", out var peterCWins, out _, out _);
            Assert.AreEqual(a.totalWins[peterA], peterAWins);
            Assert.AreEqual(b.totalWins[peterB], peterBWins);
            Assert.AreEqual(c.totalWins[peterC], peterCWins);
        }

        [Test]
        public void TestLossTotalRedundancy () {
            var a = GetRecordA();
            var b = GetRecordB();
            var c = WinLossDrawRecord.Merge(a, b);
            var peterA = Array.IndexOf(a.playerIds, "Peter");
            var peterB = Array.IndexOf(b.playerIds, "Peter");
            var peterC = Array.IndexOf(c.playerIds, "Peter");
            TallyUpResults(a, "Peter", out _, out var peterALosses, out _);
            TallyUpResults(b, "Peter", out _, out var peterBLosses, out _);
            TallyUpResults(c, "Peter", out _, out var peterCLosses, out _);
            Assert.AreEqual(a.totalLosses[peterA], peterALosses);
            Assert.AreEqual(b.totalLosses[peterB], peterBLosses);
            Assert.AreEqual(c.totalLosses[peterC], peterCLosses);
        }

        [Test]
        public void TestDrawTotalRedundancy () {
            var a = GetRecordA();
            var b = GetRecordB();
            var c = WinLossDrawRecord.Merge(a, b);
            var peterA = Array.IndexOf(a.playerIds, "Peter");
            var peterB = Array.IndexOf(b.playerIds, "Peter");
            var peterC = Array.IndexOf(c.playerIds, "Peter");
            TallyUpResults(a, "Peter", out _, out _, out var peterADraws);
            TallyUpResults(b, "Peter", out _, out _, out var peterBDraws);
            TallyUpResults(c, "Peter", out _, out _, out var peterCDraws);
            Assert.AreEqual(a.totalDraws[peterA], peterADraws);
            Assert.AreEqual(b.totalDraws[peterB], peterBDraws);
            Assert.AreEqual(c.totalDraws[peterC], peterCDraws);
        }

        [Test]
        public void TestTotalEquivalency () {
            var a = GetRecordA();
            var b = GetRecordB();
            var c = WinLossDrawRecord.Merge(a, b);
            var peterA = Array.IndexOf(a.playerIds, "Peter");
            var peterB = Array.IndexOf(b.playerIds, "Peter");
            TallyUpResults(c, "Peter", out var peterCWins, out var peterCLosses, out var peterCDraws);
            Assert.AreEqual(a.totalWins[peterA] + b.totalWins[peterB], peterCWins);
            Assert.AreEqual(a.totalLosses[peterA] + b.totalLosses[peterB], peterCLosses);
            Assert.AreEqual(a.totalDraws[peterA] + b.totalDraws[peterB], peterCDraws);
        }

        [Test]
        public void TestCopyViaMerge () {
            var a = GetRecordA();
            var b = GetRecordB();
            var c = WinLossDrawRecord.Merge(a, b);
            var d = WinLossDrawRecord.Merge(c, WinLossDrawRecord.Empty(c.matchupSize));
            for (int i = 0; i < c.playerIds.Length; i++) {
                Assert.AreEqual(c.playerIds[i], d.playerIds[i]);
                Assert.AreEqual(c.totalWins[i], d.totalWins[i]);
                Assert.AreEqual(c.totalLosses[i], d.totalLosses[i]);
                Assert.AreEqual(c.totalDraws[i], d.totalDraws[i]);
            }
            for (int i = 0; i < c.matchupWinners.Length; i++) {
                Assert.AreEqual(c.matchupWinners[i].Count, d.matchupWinners[i].Count);
                for (int j = 0; j < c.matchupWinners[i].Count; j++) {
                    Assert.AreEqual(c.matchupWinners[i][j], d.matchupWinners[i][j]);
                }
            }
        }

        [Test]
        public void TestBasicEquals () {
            var a1 = GetRecordA();
            var a2 = GetRecordA();
            var b = GetRecordB();
            Assert.AreEqual(a1, a2);
            Assert.AreNotEqual(a1, b);
        }

        [Test]
        public void TestEqualityAfterSerialization () {
            var c1 = WinLossDrawRecord.Merge(GetRecordA(), GetRecordB());
            var json = c1.ToJson();
            var c2 = WinLossDrawRecord.FromJson(json);
            Assert.AreEqual(c1, c2);
            var jsonBytes = c1.ToJsonBytes();
            var c3 = WinLossDrawRecord.FromJsonBytes(jsonBytes);
            Assert.AreEqual(c1, c3);
        }

        [Test]
        public void TestRemove () {
            var c = WinLossDrawRecord.Merge(GetRecordA(), GetRecordB());
            var d = c.Remove("Peter");
            for (int i = 0; i < c.matchupWinners.Length; i++) {
                var participants = c.GetMatchupFromIndex(i);
                if (!participants.Contains("Peter")) {;
                    var prevWinners = c.matchupWinners[i];
                    var newWinners = d.matchupWinners[d.GetMatchupIndex(participants)];
                    Assert.AreEqual(prevWinners.Count, newWinners.Count);
                    for (int j = 0; j < prevWinners.Count; j++) {
                        if (prevWinners[j] != newWinners[j]) {
                            Assert.Fail();
                        }
                    }
                }
            }
        }

        static void TallyUpResults (WinLossDrawRecord record, string player, out int wins, out int losses, out int draws) {
            wins = 0;
            losses = 0;
            draws = 0;
            foreach (var matchupResult in record.GetMatchupResultsForPlayer(player)) {
                for (int i = 0; i < matchupResult.participants.Count; i++) {
                    if (matchupResult.participants[i] == player) {
                        if (matchupResult.winnerIndex == WinLossDrawRecord.DRAW) {
                            draws++;
                        } else if (i == matchupResult.winnerIndex) {
                            wins++;
                        } else {
                            losses++;
                        }
                    }
                }
            }
        }

    }
}
