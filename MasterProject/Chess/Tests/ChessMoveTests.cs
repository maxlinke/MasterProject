using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

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
        public void TestCheckState () {
            // TODO
            throw new System.NotImplementedException();
        }

    }

}
