using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterProject.TicTacToe.Agents {

    public class RandomAgent : TTTAgent {

        public override Agent Clone () => new RandomAgent();

        public override int GetMoveIndex (TTTGameState gameState, IReadOnlyList<TTTMove> moves) {
            return GetRandomMoveIndex(moves);
        }

    }

}
