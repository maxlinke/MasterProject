using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace MasterProject.TicTacToe.Tests {

    [TestFixture]
    public class TTTGameStateTests {

        [Test]
        public void TestInitialBoard () {
            TestBoardState(null, -1, false);
        }

        [Test]
        public void TestMidGame () {
            TestBoardState(new int[]{
                -1,  0,  0,
                 0, -1, -1,
                -1, -1, -1,
            }, -1, false);
            TestBoardState(new int[]{
                -1, -1,  0,
                -1,  0, -1,
                -1,  0, -1,
            }, -1, false);
            TestBoardState(new int[]{
                 0, -1,  1,
                 1,  0,  1,
                 1,  0, -1,
            }, -1, false);
        }

        [Test]
        public void TestDraw () {
            TestBoardState(new int[]{
                0, 0, 1,
                1, 0, 0,
                0, 1, 1,
            }, -1, true);
        }

        [Test]
        public void TestHorizontals () {
            TestBoardState(new int[]{
                 0,  0,  0,
                -1, -1, -1,
                -1, -1, -1,
            }, 0, true);
            TestBoardState(new int[]{
                -1, -1, -1,
                 0,  0,  0,
                -1, -1, -1,
            }, 0, true);
            TestBoardState(new int[]{
                -1, -1, -1,
                -1, -1, -1,
                 0,  0,  0,
            }, 0, true);
        }

        [Test]
        public void TestVerticals () {
            TestBoardState(new int[]{
                0, -1, -1,
                0, -1, -1,
                0, -1, -1,
            }, 0, true);
            TestBoardState(new int[]{
                -1, 0, -1,
                -1, 0, -1,
                -1, 0, -1,
            }, 0, true);
            TestBoardState(new int[]{
                -1, -1, 0,
                -1, -1, 0,
                -1, -1, 0,
            }, 0, true);
        }

        [Test]
        public void TestDiagonals () {
            TestBoardState(new int[]{
                0, -1, -1,
               -1,  0, -1,
               -1, -1,  0,
            }, 0, true);
            TestBoardState(new int[]{
                -1, -1,  0,
                -1,  0, -1,
                 0, -1, -1,
            }, 0, true);
            TestBoardState(new int[]{
                -1,  0, -1,
                -1, -1,  0,
                 0, -1, -1,
            }, -1, false);
            TestBoardState(new int[]{
                -1, -1,  0,
                 0, -1, -1,
                -1,  0, -1,
            }, -1, false);
        }

        static void TestBoardState (int[]? inputBoard, int expectedWinner, bool expectedGameOver) {
            var tttgs = new TTTGameState();
            tttgs.Initialize();
            if (inputBoard != null) {
                Array.Copy(inputBoard, tttgs.board, 9);
            }
            tttgs.CheckBoard();
            Assert.AreEqual(tttgs.winnerIndex, expectedWinner);
            Assert.AreEqual(tttgs.GameOver, expectedGameOver);
        }

    }

}
