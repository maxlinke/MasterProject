using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using static MasterProject.Chess.Tests.ChessTestUtils;

namespace MasterProject.Chess.Tests {
    
    [TestFixture]
    public class ChessTests {

        [Test]
        public void TestBoardCreationFromString () {
            var initBoard = @"R N B Q K B N R
                              P - P P P P - P
                              - - - - - - P -
                              - P - - - - - -
                              - - - - - - - p
                              - - - p - - p -
                              p p p - p p - -
                              r n b q k b n r";
            var customCleanedParts = initBoard.Split(System.Environment.NewLine).Select(l => $"{l.Trim()} ");
            var customCleanedSb = new System.Text.StringBuilder();
            foreach (var part in customCleanedParts) {
                customCleanedSb.AppendLine(part);
            }
            var cleaned = CleanUpBoardString(initBoard);
            Assert.AreEqual(customCleanedSb.ToString(), cleaned);
            var initState = SetupGameState(initBoard, 0);
            var initStateAsString = initState.ToPrintableString(includeRowAndColumnLabels: false);
            Assert.AreEqual(initStateAsString, cleaned);
        }

        [Test]
        public void TestDefaultBoard () {
            var gs = new ChessGameState();
            gs.Initialize();
            Console.WriteLine(gs.ToPrintableString());
            TestFullySymmetric(0, 0, ChessPiece.WhiteRook);
            TestFullySymmetric(1, 0, ChessPiece.WhiteKnight);
            TestFullySymmetric(2, 0, ChessPiece.WhiteBishop);
            TestVerticallySymmetric(3, 0, ChessPiece.WhiteQueen);
            TestVerticallySymmetric(4, 0, ChessPiece.WhiteKing);
            for (int x = 0; x < 8; x++) {
                TestVerticallySymmetric(x, 1, ChessPiece.WhitePawn);
            }
            for (int x = 0; x < 8; x++) {
                for (int y = 2; y < 6; y++) {
                    Assert.AreEqual(gs.GetPieceAtPosition(x, y), ChessPiece.None);
                }
            }
            Assert.AreEqual(gs.CountTotalPiecesOfColor(ChessPieceUtils.ID_WHITE), 16);
            Assert.AreEqual(gs.CountTotalPiecesOfColor(ChessPieceUtils.ID_BLACK), 16);
            CountSymmetric(ChessPiece.WhiteRook, 2);
            CountSymmetric(ChessPiece.WhiteKnight, 2);
            CountSymmetric(ChessPiece.WhiteBishop, 2);
            CountSymmetric(ChessPiece.WhiteQueen, 1);
            CountSymmetric(ChessPiece.WhiteKing, 1);
            CountSymmetric(ChessPiece.WhitePawn, 8);
            foreach (var move in gs.GetPossibleMovesForCurrentPlayer()) {
                Assert.AreEqual(gs.GetPositionHasBeenMoved(move.srcCoord), false);
                var result = gs.GetResultOfMove(move);
                Assert.AreEqual(result.GetPositionHasBeenMoved(move.srcCoord), true);
            }

            void TestVerticallySymmetric (int whiteX, int whiteY, ChessPiece whitePiece) {
                Assert.AreEqual(gs.GetPieceAtPosition(whiteX, whiteY), whitePiece);
                var blackPiece = whitePiece.GetOppositeColor();
                Assert.AreEqual(gs.GetPieceAtPosition(whiteX, 7 - whiteY), blackPiece);
            }

            void TestFullySymmetric (int whiteX, int whiteY, ChessPiece whitePiece) {
                TestVerticallySymmetric(whiteX, whiteY, whitePiece);
                TestVerticallySymmetric(7 - whiteX, whiteY, whitePiece);
            }

            void CountSymmetric (ChessPiece whitePiece, int count) {
                Assert.AreEqual(gs.CountNumberOfPieces(whitePiece), count);
                Assert.AreEqual(gs.CountNumberOfPieces(whitePiece.GetOppositeColor()), count);
            }
        }

        [Test]
        public void TestCoordConversion () {
            var coordOccurrences = new int[64];
            for (int x = 0; x < 8; x++) {
                for (int y = 0; y < 8; y++) {
                    var coord = ChessGameState.XYToCoord(x, y);
                    ChessGameState.CoordToXY(coord, out var newX, out var newY);
                    Assert.AreEqual(x, newX);
                    Assert.AreEqual(y, newY);
                    coordOccurrences[coord]++;
                }
            }
            for (int i = 0; i < coordOccurrences.Length; i++) {
                Assert.AreEqual(coordOccurrences[i], 1);
            }
        }

        [Test]
        public void TestBitManipulationViaHasBeenMoved () {
            var gs = new ChessGameState();
            var fieldCount = ChessGameState.BOARD_SIZE * ChessGameState.BOARD_SIZE;
            for (int i = 0; i < fieldCount; i++) {
                gs.SetPositionHasBeenMoved(i, false);
            }
            for (int i = 0; i < fieldCount; i++) {
                gs.SetPositionHasBeenMoved(i, true);
                Assert.AreEqual(gs.GetPositionHasBeenMoved(i), true);
                for (int j = 0; j < i; j++) {
                    Assert.AreEqual(gs.GetPositionHasBeenMoved(j), false);
                }
                for (int j = i + 1; j < fieldCount; j++) {
                    Assert.AreEqual(gs.GetPositionHasBeenMoved(j), false);
                }
                gs.SetPositionHasBeenMoved(i, false);
            }
        }

    }

}
