using System.Collections.Generic;

namespace MasterProject.Chess.Agents {

    public class RandomAgent : ChessAgent {

        public override bool IsStateless => true;

        public override bool IsTournamentEligible => true;

        public override Agent Clone () {
            return new RandomAgent();
        }

        public override int GetMoveIndex (ChessGameState gameState, IReadOnlyList<ChessMove> moves) {
            return GetRandomMoveIndex(moves);
        }

    }

}
