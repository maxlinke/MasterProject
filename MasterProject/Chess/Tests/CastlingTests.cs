using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using static MasterProject.Chess.ChessGameStateUtils;
using static MasterProject.Chess.Tests.ChessTestUtils;

namespace MasterProject.Chess.Tests {

    [TestFixture]
    public class CastlingTests {

        [Test]
        public void WhiteCanCastle () {
            var gs = SetupGameState(@"- - - - k - - -
                                      - - - - - - - -
                                      - - - - - - - -
                                      - - - - - - - -
                                      - - - - - - - -
                                      - - - - - - - -
                                      - - - - - - - -
                                      R - - - K - - R", 0);
            var moves = gs.GetPossibleMovesForCurrentPlayer();
            Console.WriteLine($"{gs.ToPrintableString()}\n");
            Assert.AreEqual(2, moves.Where(move => move.castle).Count());
            Console.WriteLine("OPTION 1:");
            var castle1 = gs.GetResultOfMove(new List<ChessMove>(moves.Where(move => move.castle))[0]);
            Console.WriteLine($"{castle1.ToPrintableString()}\n");
            Assert.AreEqual(true, castle1.PlayerStates[0].HasCastled);
            Console.WriteLine("OPTION 2:");
            var castle2 = gs.GetResultOfMove(new List<ChessMove>(moves.Where(move => move.castle))[1]);
            Console.WriteLine($"{castle2.ToPrintableString()}\n");
            Assert.AreEqual(true, castle2.PlayerStates[0].HasCastled);
        }

        [Test]
        public void WhiteCannotCastle () {
            var gs = SetupGameState(@"- - - - k - - -
                                      - - - r - - - -
                                      - - - - - - - -
                                      - - - - - - - -
                                      - - - - - - - -
                                      - - - - - - - b
                                      - - - - - - - -
                                      R - - - K - - R", 0);
            Assert.AreEqual(0, gs.GetPossibleMovesForCurrentPlayer().Where(move => move.castle).Count());
            gs = SetupGameState(@"- - - - k - - -
                                  - - - - - - - -
                                  - - - - - - - -
                                  - - - - - - - -
                                  - - - - - - - -
                                  - - - - - - - b
                                  - p - - - - - -
                                  R - - - K - - R", 0);
            Assert.AreEqual(0, gs.GetPossibleMovesForCurrentPlayer().Where(move => move.castle).Count());
            gs = SetupGameState(@"- - - - k - - -
                                  - - - - - - - -
                                  - - - - - - - -
                                  - - - - - - - -
                                  - - - - - - - -
                                  - - - - - - - -
                                  R - - - - - - R
                                  - - - - K - - -", 0);
            Assert.AreEqual(0, gs.GetPossibleMovesForCurrentPlayer().Where(move => move.castle).Count());
            gs = gs.GetResultOfMove(GetMoveFromString(gs, "a2 a1"));
            gs = gs.GetResultOfMove(GetMoveFromString(gs, "e8 d8"));
            Assert.AreEqual(0, gs.GetPossibleMovesForCurrentPlayer().Where(move => move.castle).Count());
            gs = gs.GetResultOfMove(GetMoveFromString(gs, "h2 h1"));
            gs = gs.GetResultOfMove(GetMoveFromString(gs, "d8 e8"));
            Assert.AreEqual(0, gs.GetPossibleMovesForCurrentPlayer().Where(move => move.castle).Count());
        }

        [Test]
        public void BlackCanCastle () {
            var gs = SetupGameState(@"r - - - k - - r
                                      - - - - - - - -
                                      - - - - - - - -
                                      - - - - - - - -
                                      - - - - - - - -
                                      - - - - - - - -
                                      - - - - - - - -
                                      - - - - K - - -", 1);
            var moves = gs.GetPossibleMovesForCurrentPlayer();
            Console.WriteLine($"{gs.ToPrintableString()}\n");
            Assert.AreEqual(2, moves.Where(move => move.castle).Count());
            Console.WriteLine("OPTION 1:");
            var castle1 = gs.GetResultOfMove(new List<ChessMove>(moves.Where(move => move.castle))[0]);
            Console.WriteLine($"{castle1.ToPrintableString()}\n");
            Assert.AreEqual(true, castle1.PlayerStates[1].HasCastled);
            Console.WriteLine("OPTION 2:");
            var castle2 = gs.GetResultOfMove(new List<ChessMove>(moves.Where(move => move.castle))[1]);
            Console.WriteLine($"{castle2.ToPrintableString()}\n");
            Assert.AreEqual(true, castle2.PlayerStates[1].HasCastled);
        }

    }

}
