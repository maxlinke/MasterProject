using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterProject.G44P.Agents {

    public class Random2x : G44PAgent {

        // the idea is that this might perform a little better than random against competent players
        // because they might send out a piece to shove one of this
        // but the following piece will then shove that piece and eek out some points more than just random play

        // also i tried using an int? instead of bool and int and that worked, but SOMETIMES failed when running a massive number of games asynchronously
        // as in, i did a "HasValue" check, which returned true, but when i actually GOT the value, all i got was an InvalidOperationException telling me there was no value
        // but even the debugger showed there being a value
        // i looked this up online and it seems to be a known issue
        // so the primitive solution it is...

        public override bool IsStateless => false;

        public override bool IsTournamentEligible => true;

        bool useLastMoveIndex;
        int lastMoveIndex;

        public override Agent Clone () {
            return new Random2x();
        }

        public override int GetMoveIndex (G44PGameState gameState, IReadOnlyList<G44PMove> moves) {
            int output;
            if (useLastMoveIndex) {
                output = lastMoveIndex;
                useLastMoveIndex = false;
            } else {
                output = GetRandomMoveIndex(moves);
                useLastMoveIndex = true;
            }
            lastMoveIndex = output;
            return output;
        }
    }

}
