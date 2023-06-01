namespace MasterProject.G44P.Agents {

    public class RandomAgent : G44PAgent {

        public override bool IsStateless => true;

        public override bool IsTournamentEligible => true;

        public override Agent Clone () {
            return new RandomAgent();
        }

        public override int GetMoveIndex (G44PGameState gameState, IReadOnlyList<G44PMove> moves) {
            return GetRandomMoveIndex(moves);
        }

    }

}
