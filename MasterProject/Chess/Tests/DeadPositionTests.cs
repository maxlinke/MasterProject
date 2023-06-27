using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using static MasterProject.Chess.Tests.ChessTestUtils;

namespace MasterProject.Chess.Tests {

    [TestFixture]
    public class DeadPositionTests {

        [Test]
        public void TestDefaultState () {
            var gs = new ChessGameState();
            gs.Initialize();
            Assert.AreEqual(gs.DetermineIfBoardIsDeadPosition(), false);
        }

        [Test]
        public void TestTwoKings () {
            var gs = SetupGameState(@"K k - - - - - -
                                      - - - - - - - - 
                                      - - - - - - - - 
                                      - - - - - - - - 
                                      - - - - - - - - 
                                      - - - - - - - - 
                                      - - - - - - - - 
                                      - - - - - - - -", 0);
            Assert.AreEqual(gs.DetermineIfBoardIsDeadPosition(), true);
        }

        [Test]
        public void TestTwoKingsAndOneBishop () {
            var initState = @"- - - - - - - -
                              - - - - - - - - 
                              - - - - - - - k 
                              - K - - - - - - 
                              - - - - X - - - 
                              - - - - - - - - 
                              - - - - - - - - 
                              - - - - - - - -";
            Assert.AreEqual(SetupGameState(initState.Replace("X", "B"), 0).DetermineIfBoardIsDeadPosition(), true);
            Assert.AreEqual(SetupGameState(initState.Replace("X", "b"), 0).DetermineIfBoardIsDeadPosition(), true);
        }

        [Test]
        public void TestTwoKingsAndOneKnight () {
            var initState = @"- - - - - - - -
                              - - - - - - - - 
                              - - - - - - - k 
                              - K - - - - - - 
                              - - - - X - - - 
                              - - - - - - - - 
                              - - - - - - - - 
                              - - - - - - - -";
            Assert.AreEqual(SetupGameState(initState.Replace("X", "N"), 0).DetermineIfBoardIsDeadPosition(), true);
            Assert.AreEqual(SetupGameState(initState.Replace("X", "n"), 0).DetermineIfBoardIsDeadPosition(), true);
        }

        [Test]
        public void TestTwoKingsAndTwoBishops () {
            var initStateA = @"- - - - - - - -
                               - - - - - - - - 
                               - - - - - - - k 
                               - K - - - - - - 
                               - - - - X - - - 
                               - - - x - - - - 
                               - - - - - - - - 
                               - - - - - - - -";
            Assert.AreEqual(SetupGameState(initStateA.Replace("X", "B").Replace("x", "b"), 0).DetermineIfBoardIsDeadPosition(), true);
            Assert.AreEqual(SetupGameState(initStateA.Replace("X", "b").Replace("x", "B"), 0).DetermineIfBoardIsDeadPosition(), true);
            var initStateB = @"- - - - - - - -
                               - - - - - - - - 
                               - - - - - - - k 
                               - K - - - - - - 
                               - - - - X - - - 
                               - - - - x - - - 
                               - - - - - - - - 
                               - - - - - - - -";
            Assert.AreEqual(SetupGameState(initStateB.Replace("X", "B").Replace("x", "b"), 0).DetermineIfBoardIsDeadPosition(), false);
            Assert.AreEqual(SetupGameState(initStateB.Replace("X", "b").Replace("x", "B"), 0).DetermineIfBoardIsDeadPosition(), false);
        }

    }
}
