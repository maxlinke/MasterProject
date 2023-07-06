namespace MasterProject.Chess.Agents {

    public class Swarm : TotalDistanceMinimizer {

        public override Agent Clone () {
            return new Swarm ();
        }

        protected override int GetTargetCoordForResultGamestate (ChessGameState initState, ChessGameState resultState) {
            var otherPlayerIndex = (initState.currentPlayerIndex + 1) % ChessGameState.PLAYER_COUNT;
            return resultState.playerStates[otherPlayerIndex].KingCoord;
        }

    }
}
