using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterProject.G44P.Agents {

    public class ZigZag : G44PAgent {

        public override bool IsStateless => false;

        public override bool IsTournamentEligible => true;

        protected int prevMoveIndex = -1;
        protected int delta = 1;

        public override Agent Clone () {
            return new ZigZag ();
        }

        public override int GetMoveIndex (G44PGameState gameState, IReadOnlyList<G44PMove> moves) {
            var newMoveIndex = prevMoveIndex + delta;
            if (newMoveIndex < 0) {
                newMoveIndex = Math.Min(1, moves.Count);
                delta = 1;
            }else if (newMoveIndex >= moves.Count) {
                newMoveIndex = Math.Max(0, moves.Count - 2);
                delta = -1;
            }
            prevMoveIndex = newMoveIndex;
            return newMoveIndex;
        }

    }

}
