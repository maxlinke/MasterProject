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

    }

}
