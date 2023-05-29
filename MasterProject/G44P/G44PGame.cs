using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterProject.G44P {

    public class G44PGame : Game<G44PGame, G44PGameState, G44PMove> {

        protected override int MinimumNumberOfAgentsRequired => G44PGameState.PLAYER_COUNT;

        protected override int MaximumNumberOfAgentsAllowed => G44PGameState.PLAYER_COUNT;

        protected override G44PGameState GetInitialGameState () {
            throw new NotImplementedException();
        }

    }

}
