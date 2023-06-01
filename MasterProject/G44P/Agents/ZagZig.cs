using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterProject.G44P.Agents {

    public class ZagZig : ZigZag {

        public override Agent Clone () {
            return new ZagZig();
        }

        bool firstMoveFlipPerformed = false;

        public override int GetMoveIndex (G44PGameState gameState, IReadOnlyList<G44PMove> moves) {
            if (!firstMoveFlipPerformed) {
                firstMoveFlipPerformed = true;
                prevMoveIndex = moves.Count;
                delta = -1;
            }
            return base.GetMoveIndex(gameState, moves);
        }

    }

}
