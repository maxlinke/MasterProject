using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterProject.G44P.Agents {

    public class OnlyLast : G44PAgent {

        public override bool IsStateless => true;

        public override bool IsTournamentEligible => true;

        public override Agent Clone () => new OnlyLast ();

        public override int GetMoveIndex (G44PGameState gameState, IReadOnlyList<G44PMove> moves) => moves.Count - 1;

    }

}
