using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using MasterProject.Records;

namespace MasterProject.Tests {

    [TestFixture]
    public class WinLossDrawTests {

        class TestGameState : GameState {

            private int winnerIndex;

            public override bool GameOver => true;
            public override int CurrentPlayerIndex => throw new NotImplementedException();

            public override bool GetPlayerHasDrawn (int index) => winnerIndex < 0;

            public override bool GetPlayerHasLost (int index) => (winnerIndex >= 0 && index != winnerIndex);

            public override bool GetPlayerHasWon (int index) => (winnerIndex >= 0 && index == winnerIndex);

            private TestGameState () { }

            public static TestGameState Draw () {
                var output = new TestGameState();
                output.winnerIndex = -1;
                return output;
            }

            public static TestGameState Victory (int winner) {
                var output = new TestGameState();
                output.winnerIndex = winner;
                return output;
            }

        }

        static WinLossDrawRecord GetRecordA () {
            var output = WinLossDrawRecord.New(new string[]{
                "Peter",
                "Bob"
            }, 2);
            for (int i = 0; i < 3; i++)
                output.RecordResult(new string[] { "Peter", "Bob" }, TestGameState.Victory(0));   // 3 wins for peter
            for (int i = 0; i < 1; i++)
                output.RecordResult(new string[] { "Peter", "Bob" }, TestGameState.Victory(1));   // 1 win for bob
            for (int i = 0; i < 1; i++)
                output.RecordResult(new string[] { "Bob", "Peter" }, TestGameState.Victory(0));   // 1 win for bob
            for (int i = 0; i < 2; i++)
                output.RecordResult(new string[] { "Bob", "Peter" }, TestGameState.Victory(1));   // 2 wins for peter
            for (int i = 0; i < 1; i++)
                output.RecordResult(new string[] { "Peter", "Bob" }, TestGameState.Draw());     // 1 draw for both
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
                output.RecordResult(new string[] { "Peter", "Bob" }, TestGameState.Victory(0));   // 6 wins for peter
            for (int i = 0; i < 2; i++)
                output.RecordResult(new string[] { "Peter", "Jim" }, TestGameState.Victory(0));   // 2 wins for peter
            for (int i = 0; i < 1; i++)
                output.RecordResult(new string[] { "Peter", "Jim" }, TestGameState.Victory(1));   // 1 win for jim
            for (int i = 0; i < 2; i++)
                output.RecordResult(new string[] { "Peter", "Peter" }, TestGameState.Victory(0)); // 2 wins + losses for peter
            for (int i = 0; i < 1; i++)
                output.RecordResult(new string[] { "Peter", "Peter" }, TestGameState.Victory(1)); // 1 win + loss for peter
            for (int i = 0; i < 3; i++)
                output.RecordResult(new string[] { "Peter", "Peter" }, TestGameState.Draw());   // 6 draws for peter
            for (int i = 0; i < 1; i++)
                output.RecordResult(new string[] { "Jim", "Mary" }, TestGameState.Draw());      // 1 draw for jim and mary
            return output;
        }

        [Test]
        public void ManualNumberTestA () {
            var a = GetRecordA();
            var iPeter = Array.IndexOf(a.playerIds, "Peter");
            var iBob = Array.IndexOf(a.playerIds, "Bob");
            Assert.AreEqual(5, a.totalWins[iPeter]);
            Assert.AreEqual(2, a.totalWins[iBob]);
            Assert.AreEqual(2, a.totalLosses[iPeter]);
            Assert.AreEqual(5, a.totalLosses[iBob]);
            Assert.AreEqual(1, a.totalDraws[iPeter]);
            Assert.AreEqual(1, a.totalDraws[iBob]);
        }

        [Test]
        public void ManualNumberTestB () {
            var b = GetRecordB();
            var iJim = Array.IndexOf(b.playerIds, "Jim");
            var iMary = Array.IndexOf(b.playerIds, "Mary");
            var iBob = Array.IndexOf(b.playerIds, "Bob");
            var iPeter = Array.IndexOf(b.playerIds, "Peter");
            Assert.AreEqual(1,  b.totalWins[iJim]);
            Assert.AreEqual(0,  b.totalWins[iMary]);
            Assert.AreEqual(0,  b.totalWins[iBob]);
            Assert.AreEqual(11, b.totalWins[iPeter]);
            Assert.AreEqual(2,  b.totalLosses[iJim]);
            Assert.AreEqual(0,  b.totalLosses[iMary]);
            Assert.AreEqual(6,  b.totalLosses[iBob]);
            Assert.AreEqual(4,  b.totalLosses[iPeter]);
            Assert.AreEqual(1,  b.totalDraws[iJim]);
            Assert.AreEqual(1,  b.totalDraws[iMary]);
            Assert.AreEqual(0,  b.totalDraws[iBob]);
            Assert.AreEqual(6,  b.totalDraws[iPeter]);
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
            Assert.AreEqual(c, d);
            Assert.AreNotEqual(ReferenceEquals(c, d), true);
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
            for (int i = 0; i < c.matchupRecords.Length; i++) {
                var participants = c.GetMatchupFromIndex(i);
                if (!participants.Contains("Peter")) {;
                    var prevRecord = c.matchupRecords[i];
                    var newRecord = d.matchupRecords[d.GetMatchupIndex(participants)];
                    Assert.AreEqual(prevRecord.gameResults.Count, newRecord.gameResults.Count);
                    for (int j = 0; j < prevRecord.gameResults.Count; j++) {
                        if (prevRecord.gameResults[j] != newRecord.gameResults[j]) {
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
            foreach (var matchupRecord in record.GetMatchupRecordsForPlayer(player)) {
                for (int i = 0; i < matchupRecord.playerIds.Length; i++) {
                    if (matchupRecord.playerIds[i] == player) {
                        foreach (var result in matchupRecord.gameResults) {
                            switch (result[i]) {
                                case WinLossDrawRecord.MatchupRecord.DRAW:
                                    draws++;
                                    break;
                                case WinLossDrawRecord.MatchupRecord.LOSS:
                                    losses++;
                                    break;
                                case WinLossDrawRecord.MatchupRecord.WIN:
                                    wins++;
                                    break;
                                default:
                                    throw new ArgumentException($"unknown character '{result[i]}'");
                            }
                        }
                    }
                }
            }
        }

    }
}
