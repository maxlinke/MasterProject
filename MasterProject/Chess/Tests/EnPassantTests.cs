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
    public class EnPassantTests {

        [Test]
        public void WhiteEnPassant () {
            var gs = SetupGameState(@"- - - - k - - -
                                      - - - - p - - -
                                      - - - - - - - -
                                      - - - P - - - -
                                      - - - - - - - -
                                      - - - - - - - -
                                      - - - - - - - -
                                      - - - - K - - -", 1);
            Console.WriteLine(gs.ToPrintableString());
            gs = gs.GetResultOfMove(GetMoveFromString(gs, "e7 e5"));
            Console.WriteLine(gs.ToPrintableString());
            var pawnMoves = gs.GetPossibleMovesForCurrentPlayer().Where(move => gs.board[move.srcCoord] == ChessPiece.WhitePawn);
            Console.WriteLine();
            foreach (var pawnMove in pawnMoves) {
                Console.WriteLine($" > move {gs.board[pawnMove.srcCoord]} {pawnMove.CoordinatesToString()}{(pawnMove.enPassantCapture ? " (en passant)" : "")}");
            }
            Console.WriteLine();
            var move = GetMoveFromString(pawnMoves, "d5 e6");
            Assert.AreEqual(true, move.enPassantCapture);
            gs = gs.GetResultOfMove(move);
            Console.WriteLine(gs.ToPrintableString());
            Assert.AreEqual(ChessPiece.WhitePawn, gs.board[CoordFromString("e6")]);
            Assert.AreEqual(ChessPiece.None, gs.board[CoordFromString("e5")]);
        }

        [Test]
        public void WhiteNotEnPassant () {
            var gs = SetupGameState(@"- - - - k - - -
                                      - - - - p - - -
                                      - - - - - - - -
                                      - - - - - - - -
                                      - - - P - - - -
                                      - - - - - - - -
                                      - - - - - - - -
                                      - - - - K - - -", 1);
            Console.WriteLine(gs.ToPrintableString());
            gs = gs.GetResultOfMove(GetMoveFromString(gs, "e7 e5"));
            Console.WriteLine(gs.ToPrintableString());
            gs = gs.GetResultOfMove(GetMoveFromString(gs, "d4 d5"));
            Console.WriteLine(gs.ToPrintableString());
            gs = gs.GetResultOfMove(GetMoveFromString(gs, "e8 f8"));
            Console.WriteLine(gs.ToPrintableString());
            var pawnMoves = gs.GetPossibleMovesForCurrentPlayer().Where(move => gs.board[move.srcCoord] == ChessPiece.WhitePawn);
            foreach (var pawnMove in pawnMoves) {
                Console.WriteLine($" > move {gs.board[pawnMove.srcCoord]} {pawnMove.CoordinatesToString()}");
            }
            Assert.AreEqual(0, pawnMoves.Where(move => move.enPassantCapture).Count());
        }

        [Test]
        public void BlackEnPassant () {
            var gs = SetupGameState(@"- - - - k - - -
                                      - - - - - - - -
                                      - - - - - - - -
                                      - - - - - - - -
                                      - p - - - - - -
                                      - - - - - - - -
                                      P - - - - - - -
                                      - - - - K - - -", 0);
            Console.WriteLine(gs.ToPrintableString());
            gs = gs.GetResultOfMove(GetMoveFromString(gs, "a2 a4"));
            Console.WriteLine(gs.ToPrintableString());
            var pawnMoves = gs.GetPossibleMovesForCurrentPlayer().Where(move => gs.board[move.srcCoord] == ChessPiece.BlackPawn);
            Console.WriteLine();
            foreach (var pawnMove in pawnMoves) {
                Console.WriteLine($" > move {gs.board[pawnMove.srcCoord]} {pawnMove.CoordinatesToString()}{(pawnMove.enPassantCapture ? " (en passant)" : "")}");
            }
            Console.WriteLine();
            var move = GetMoveFromString(pawnMoves, "b4 a3");
            Assert.AreEqual(true, move.enPassantCapture);
            gs = gs.GetResultOfMove(move);
            Console.WriteLine(gs.ToPrintableString());
            Assert.AreEqual(ChessPiece.BlackPawn, gs.board[CoordFromString("a3")]);
            Assert.AreEqual(ChessPiece.None, gs.board[CoordFromString("a4")]);
        }

        [Test]
        public void BlackNotEnPassant () {
            var gs = SetupGameState(@"- - - - k - - -
                                      - - - - - - - -
                                      - - - - - - - -
                                      - p - - - - - -
                                      - - - - - - - -
                                      - - - - - - - -
                                      P - - - - - - -
                                      - - - - K - - -", 0);
            Console.WriteLine(gs.ToPrintableString());
            gs = gs.GetResultOfMove(GetMoveFromString(gs, "a2 a3"));
            Console.WriteLine(gs.ToPrintableString());
            gs = gs.GetResultOfMove(GetMoveFromString(gs, "b5 b4"));
            Console.WriteLine(gs.ToPrintableString());
            gs = gs.GetResultOfMove(GetMoveFromString(gs, "a3 a4"));
            Console.WriteLine(gs.ToPrintableString());
            var pawnMoves = gs.GetPossibleMovesForCurrentPlayer().Where(move => gs.board[move.srcCoord] == ChessPiece.BlackPawn);
            Console.WriteLine();
            foreach (var pawnMove in pawnMoves) {
                Console.WriteLine($" > move {gs.board[pawnMove.srcCoord]} {pawnMove.CoordinatesToString()}{(pawnMove.enPassantCapture ? " (en passant)" : "")}");
            }
            Console.WriteLine();
            Assert.AreEqual(0, pawnMoves.Where(move => move.enPassantCapture).Count());
        }

        [Test]
        public void EnPassantCheckmate () {
            var gs = SetupGameState(@"r - b q - r - -
                                      p p - - n - p -
                                      - - - - p - k -
                                      - - - p P p N - 
                                      - b - n - - Q P
                                      - - N - - - - -
                                      P P - - - P P -
                                      R - B - K - - R", 0);
            Console.WriteLine($"{gs.ToPrintableString()}\n");
            gs = gs.GetResultOfMove(GetMoveFromString(gs, "h4 h5"));    // move rightmost pawn next to knight
            Console.WriteLine($"{gs.ToPrintableString()}\n");
            Assert.AreEqual(true, gs.PlayerStates[1].IsInCheck);
            gs = gs.GetResultOfMove(GetMoveFromString(gs, "g6 h6"));    // move king right to escape check
            Console.WriteLine($"{gs.ToPrintableString()}\n");
            gs = gs.GetResultOfMove(GetMoveFromString(gs, "g5 e6"));    // capture pawn with knight
            Console.WriteLine($"{gs.ToPrintableString()}\n");
            gs = gs.GetResultOfMove(GetMoveFromString(gs, "g7 g5"));    // move pawn to prevent capture by knight
            Console.WriteLine($"{gs.ToPrintableString()}\n");
            var pawnMoves = gs.GetPossibleMovesForCurrentPlayer().Where(move => gs.board[move.srcCoord] == ChessPiece.WhitePawn);
            Assert.AreEqual(1, pawnMoves.Where(move => move.enPassantCapture).Count());
            var winningMove = pawnMoves.First(move => move.enPassantCapture);
            gs = gs.GetResultOfMove(winningMove);
            Console.WriteLine($"{gs.ToPrintableString()}\n");
            Assert.AreEqual(true, gs.GameOver);
            Assert.AreEqual(true, gs.PlayerStates[0].HasWon);
            Assert.AreEqual(true, gs.PlayerStates[1].HasLost);
        }

    }

}
