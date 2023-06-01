using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterProject.G44P.Agents {

    public class OnlyFirst : G44PAgent {

        public override bool IsStateless => true;

        public override bool IsTournamentEligible => true;

        public override Agent Clone () => new OnlyFirst ();

        public override int GetMoveIndex (G44PGameState gameState, IReadOnlyList<G44PMove> moves) => 0;

    }
}
