namespace MasterProject.Chess.Agents {

    public class SameColor : ColorMatcher {

        public override Agent Clone () {
            return new SameColor();
        }

        public override bool GetShouldPutPiecesOnWhiteSquares (ChessGameState gameState) {
            return gameState.currentPlayerIndex == ChessGameState.INDEX_WHITE;
        }
    }

}
