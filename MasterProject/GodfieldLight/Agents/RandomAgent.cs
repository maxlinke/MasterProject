using System.Collections.Generic;

namespace MasterProject.GodfieldLight.Agents {

    public class RandomAgent : GodfieldAgent {

        public override bool IsStateless => true;

        public override bool IsTournamentEligible => true;

        public override Agent Clone () {
            return new RandomAgent();
        }

        public override int GetMoveIndex (GodfieldGameState gameState, IReadOnlyList<GodfieldMove> moves) {
            return GetRandomMoveIndex(moves);
        }

    }

}
