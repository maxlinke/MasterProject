namespace MasterProject.Chess.Agents {

    public class Huddle : TotalDistanceMinimizer {

        public override Agent Clone () {
            return new Huddle();
        }

        protected override int GetTargetCoordForResultGamestate (ChessGameState initState, ChessGameState resultState) {
            return resultState.playerStates[initState.currentPlayerIndex].KingCoord;
        }

    }

}
