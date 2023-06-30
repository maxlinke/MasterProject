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

        // TODO test whether castling works properly
        //      - set up a specific board (also needs the "has moved" stuff)
        //      - (in the setup, set all fields that match the initial board to not have moved, otherwise they have moved)
        //      - setup should also setup playerstates (optionally?)
        // TODO test whether all the moves work as intended
        // TODO test whether a move that would normally work is not included because it would put the king in check
        // TODO test checkmate

        // TODO also i still need to implement the other two draw conditions (same state, no captures)

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
            Console.WriteLine($"{gs.ToPrintableString()}\n");
            foreach (var move in gs.GetPossibleMovesForCurrentPlayer()) {
                Console.WriteLine($"Can move {gs.board[move.srcCoord]} from {ChessGameState.CoordToString(move.srcCoord)} to {ChessGameState.CoordToString(move.dstCoord)}");
            }
            TestFullySymmetric(0, 0, ChessPiece.WhiteRook);
            TestFullySymmetric(1, 0, ChessPiece.WhiteKnight);
            TestFullySymmetric(2, 0, ChessPiece.WhiteBishop);
            TestVerticallySymmetric(3, 0, ChessPiece.WhiteQueen);
            TestVerticallySymmetric(4, 0, ChessPiece.WhiteKing);
            for (int x = 0; x < 8; x++) {
                TestVerticallySymmetric(x, 1, ChessPiece.WhitePawn);
            }
            for (int i = ChessGameState.XYToCoord(0, 2); i < ChessGameState.XYToCoord(0, 6); i++) {
                Assert.AreEqual(ChessPiece.None, gs.GetPieceAtCoordinate(i));
            }
            Assert.AreEqual(16, gs.CountTotalPiecesOfColor(ChessPieceUtils.ID_WHITE));
            Assert.AreEqual(16, gs.CountTotalPiecesOfColor(ChessPieceUtils.ID_BLACK));
            CountSymmetric(ChessPiece.WhiteRook, 2);
            CountSymmetric(ChessPiece.WhiteKnight, 2);
            CountSymmetric(ChessPiece.WhiteBishop, 2);
            CountSymmetric(ChessPiece.WhiteQueen, 1);
            CountSymmetric(ChessPiece.WhiteKing, 1);
            CountSymmetric(ChessPiece.WhitePawn, 8);
            CountNumberOfMovesForPieceType(ChessPiece.WhiteRook, 0);
            CountNumberOfMovesForPieceType(ChessPiece.WhiteKnight, 4);
            CountNumberOfMovesForPieceType(ChessPiece.WhiteBishop, 0);
            CountNumberOfMovesForPieceType(ChessPiece.WhiteQueen, 0);
            CountNumberOfMovesForPieceType(ChessPiece.WhiteKing, 0);
            CountNumberOfMovesForPieceType(ChessPiece.WhitePawn, 16);
            foreach (var move in gs.GetPossibleMovesForCurrentPlayer()) {
                Assert.AreEqual(false, gs.GetPositionHasBeenMoved(move.srcCoord));
                var result = gs.GetResultOfMove(move);
                Assert.AreEqual(true, result.GetPositionHasBeenMoved(move.srcCoord));
            }

            void TestVerticallySymmetric (int whiteX, int whiteY, ChessPiece whitePiece) {
                Assert.AreEqual(whitePiece, gs.GetPieceAtPosition(whiteX, whiteY));
                var blackPiece = whitePiece.GetOppositeColor();
                Assert.AreEqual(blackPiece, gs.GetPieceAtPosition(whiteX, 7 - whiteY));
            }

            void TestFullySymmetric (int whiteX, int whiteY, ChessPiece whitePiece) {
                TestVerticallySymmetric(whiteX, whiteY, whitePiece);
                TestVerticallySymmetric(7 - whiteX, whiteY, whitePiece);
            }

            void CountSymmetric (ChessPiece whitePiece, int count) {
                Assert.AreEqual(count, gs.CountNumberOfPieces(whitePiece));
                Assert.AreEqual(count, gs.CountNumberOfPieces(whitePiece.GetOppositeColor()));
            }

            void CountNumberOfMovesForPieceType (ChessPiece piece, int targetCount) {
                var count = 0;
                foreach (var move in gs.GetPossibleMovesForCurrentPlayer()) {
                    if (gs.board[move.srcCoord] == piece) {
                        count++;
                    }
                }
                Assert.AreEqual(targetCount, count);
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
