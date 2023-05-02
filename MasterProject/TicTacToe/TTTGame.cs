using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterProject.TicTacToe {

    public class TTTGame : Game<TTTGame, TTTGameState, TTTMove, TTTAgent> {

        protected override int MinimumNumberOfAgentsRequired => TTTGameState.PLAYER_COUNT;

        protected override int MaximumNumberOfAgentsAllowed => TTTGameState.PLAYER_COUNT;

        protected override TTTGameState GetInitialGameState () {
            var output = new TTTGameState();
            output.Initialize();
            return output;
        }

    }

}
