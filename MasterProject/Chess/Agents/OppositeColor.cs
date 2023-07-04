namespace MasterProject.Chess.Agents {

    public class OppositeColor : ColorMatcher {

        public override Agent Clone () {
            return new OppositeColor ();
        }

        public override bool GetShouldPutPiecesOnWhiteSquares (ChessGameState gameState) {
            return gameState.currentPlayerIndex == ChessGameState.INDEX_BLACK;
        }

    }

}
