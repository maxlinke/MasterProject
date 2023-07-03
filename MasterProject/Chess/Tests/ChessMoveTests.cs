using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using static MasterProject.Chess.Tests.ChessTestUtils;

namespace MasterProject.Chess.Tests {

    [TestFixture]
    public class ChessMoveTests {

        [Test]
        public void TestPromotionOptionsForWhite () {
            var gs = new ChessGameState();
            gs.Initialize();
            gs = ChessGameState.RunSeriesOfMovesAsStrings(
                gs, true,
                "b2 b4", "h7 h6",
                "b4 b5", "g7 g6",
                "b5 b6", "f7 f6",
                "b6 a7", "e7 e6"
            );
            var coord = ChessGameState.CoordFromString("a7");
            var moves = new List<ChessMove>(ChessMoveUtils.GetLegalMovesForPiece(gs, coord));
            var options = new List<ChessPiece>(ChessPieceUtils.WhitePawnPromotionOptions);
            Console.WriteLine("Move options: ");
            for (int i = 0; i < moves.Count; i++) {
                var move = moves[i];
                Console.WriteLine($"{ChessGameState.CoordToString(move.srcCoord)} to {ChessGameState.CoordToString(move.dstCoord)} and promote to {move.promoteTo}");
            }
            for (int i = 0; i < moves.Count; i++) {
                Assert.AreEqual(options[i], moves[i].promoteTo);
            }
        }

        [Test]
        public void TestPromotionOptionsForBlack () {
            var gs = new ChessGameState();
            gs.Initialize();
            gs = ChessGameState.RunSeriesOfMovesAsStrings(
                gs, true,
                "a2 a3", "g7 g5",
                "b2 b3", "g5 g4",
                "c2 c3", "g4 g3",
                "d2 d3", "g3 h2",
                "e2 e3"
            );
            var coord = ChessGameState.CoordFromString("h2");
            var moves = new List<ChessMove>(ChessMoveUtils.GetLegalMovesForPiece(gs, coord));
            var options = new List<ChessPiece>(ChessPieceUtils.BlackPawnPromotionOptions);
            Console.WriteLine("Move options: ");
            for (int i = 0; i < moves.Count; i++) {
                var move = moves[i];
                Console.WriteLine($"{ChessGameState.CoordToString(move.srcCoord)} to {ChessGameState.CoordToString(move.dstCoord)} and promote to {move.promoteTo}");
            }
            for (int i = 0; i < moves.Count; i++) {
                Assert.AreEqual(options[i], moves[i].promoteTo);
            }
        }

        [Test]
        public void TestKingMoves () {
            var gs = SetupGameState(@"- - - - - - - r
                                      - - k - - - - - 
                                      - - - - - - - - 
                                      - - - - - - - - 
                                      - - - - - - - - 
                                      - - - - K - - - 
                                      - - - - - - - - 
                                      R - - - - - - -", 0);
            var kingMoves = -1;
            // white's turn
            PrintBoardAndKingMoves(gs, out kingMoves);
            Assert.AreEqual(8, kingMoves);
            Assert.AreEqual(14, gs.GetPossibleMovesForCurrentPlayer().Where((move) => gs.board[move.srcCoord] == ChessPiece.WhiteRook).Count());
            gs = gs.GetResultOfMove(GetMoveFromString(gs.GetPossibleMovesForCurrentPlayer(), "e3 f3"));
            // black's turn
            PrintBoardAndKingMoves(gs, out kingMoves);
            Assert.AreEqual(8, kingMoves);
            Assert.AreEqual(14, gs.GetPossibleMovesForCurrentPlayer().Where((move) => gs.board[move.srcCoord] == ChessPiece.BlackRook).Count());
            gs = gs.GetResultOfMove(GetMoveFromString(gs.GetPossibleMovesForCurrentPlayer(), "c7 b7"));
            // white's turn
            PrintBoardAndKingMoves(gs, out kingMoves);
            Assert.AreEqual(8, kingMoves);
            Assert.AreEqual(14, gs.GetPossibleMovesForCurrentPlayer().Where((move) => gs.board[move.srcCoord] == ChessPiece.WhiteRook).Count());
            gs = gs.GetResultOfMove(GetMoveFromString(gs.GetPossibleMovesForCurrentPlayer(), "a1 a6"));
            // black's turn
            PrintBoardAndKingMoves(gs, out kingMoves);
            Assert.AreEqual(4, kingMoves);
            Assert.AreEqual(false, gs.playerStates[gs.CurrentPlayerIndex].IsInCheck);
            Assert.AreEqual(14, gs.GetPossibleMovesForCurrentPlayer().Where((move) => gs.board[move.srcCoord] == ChessPiece.BlackRook).Count());
            gs = gs.GetResultOfMove(GetMoveFromString(gs.GetPossibleMovesForCurrentPlayer(), "h8 h3"));
            // white's turn
            PrintBoardAndKingMoves(gs, out kingMoves);
            Assert.AreEqual(6, kingMoves);
            Assert.AreEqual(true, gs.PlayerStates[gs.CurrentPlayerIndex].IsInCheck);
            Assert.AreEqual(0, gs.GetPossibleMovesForCurrentPlayer().Where((move) => gs.board[move.srcCoord] == ChessPiece.WhiteRook).Count());
            gs = gs.GetResultOfMove(GetMoveFromString(gs.GetPossibleMovesForCurrentPlayer(), "f3 f2"));
            // black's turn
            PrintBoardAndKingMoves(gs, out kingMoves);
            Assert.AreEqual(4, kingMoves);
            gs = gs.GetResultOfMove(GetMoveFromString(gs.GetPossibleMovesForCurrentPlayer(), "b7 b8"));
            // white's turn
            PrintBoardAndKingMoves(gs, out kingMoves);
            Assert.AreEqual(5, kingMoves);
            gs = gs.GetResultOfMove(GetMoveFromString(gs.GetPossibleMovesForCurrentPlayer(), "a6 b6"));
            // black's turn
            PrintBoardAndKingMoves(gs, out kingMoves);
            Assert.AreEqual(4, kingMoves);
            gs = gs.GetResultOfMove(GetMoveFromString(gs.GetPossibleMovesForCurrentPlayer(), "b8 a8"));
            // white's turn
            PrintBoardAndKingMoves(gs, out kingMoves);
            Assert.AreEqual(5, kingMoves);
            gs = gs.GetResultOfMove(GetMoveFromString(gs.GetPossibleMovesForCurrentPlayer(), "b6 b7"));
            // black's turn
            PrintBoardAndKingMoves(gs, out kingMoves);
            Assert.AreEqual(1, kingMoves);
            Assert.AreEqual(false, gs.playerStates[gs.CurrentPlayerIndex].IsInCheck);
            //foreach (var move in gs.GetPossibleMovesForCurrentPlayer()) {
            //    Console.WriteLine($" > {gs.board[move.srcCoord]} {ChessGameState.CoordToString(move.srcCoord)} to {ChessGameState.CoordToString(move.dstCoord)}");
            //}
            //// black can still move because black is not in check. if that rook were a queen, the king could still move by capturing the rook. i think this is just a bad setup here for testing checkmate
            //Assert.AreEqual(1, gs.GetPossibleMovesForCurrentPlayer().Count);

        }

        [Test]
        public void TestCheckState () {
            var gs = SetupGameState(@"- - - - - - - - 
                                      - - - - - - - - 
                                      - - - k - b - -
                                      - - - - - - - n
                                      - - - - - - - -
                                      - - - - - K - -
                                      - - - - - B - -
                                      - - - - - - - -", 0);
            Assert.AreEqual(false, gs.PlayerStates[ChessGameState.INDEX_WHITE].IsInCheck);
            Assert.AreEqual(false, gs.PlayerStates[ChessGameState.INDEX_BLACK].IsInCheck);
            var whiteMove = GetMoveFromString(gs.GetPossibleMovesForCurrentPlayer(), "f2 g3");
            gs = gs.GetResultOfMove(whiteMove);
            var board = gs.ToPrintableString();
            var bishopCoord = ChessGameState.CoordFromString("g3");
            var bishopMap = ChessGameState.MakePrintableAttackMap(bishopCoord, ChessMoveUtils.GetAttackMap(gs, bishopCoord), false);
            var blackKingCoord = ChessGameState.CoordFromString("d6");
            var blackKingMap = ChessGameState.MakePrintableAttackMap(blackKingCoord, ChessMoveUtils.GetAttackMap(gs, blackKingCoord), false);
            Console.WriteLine(board.HorizontalConcat(bishopMap, "  |  ").HorizontalConcat(blackKingMap, "  |  "));
            Console.WriteLine($"black king is at {ChessGameState.CoordToString(gs.playerStates[ChessGameState.INDEX_BLACK].KingCoord)}");
            Assert.AreEqual(false, gs.PlayerStates[ChessGameState.INDEX_WHITE].IsInCheck);
            Assert.AreEqual(true, gs.PlayerStates[ChessGameState.INDEX_BLACK].IsInCheck);
            var blackMoves = gs.GetPossibleMovesForCurrentPlayer();
            Console.WriteLine($"Black has {blackMoves} moves:");
            foreach (var move in blackMoves) {
                Console.WriteLine($" > {move.CoordinatesToString()} ({gs.board[move.srcCoord]})");
            }
        }

    }

}
