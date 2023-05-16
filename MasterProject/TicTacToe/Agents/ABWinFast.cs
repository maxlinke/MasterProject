using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterProject.TicTacToe.Agents {
    
    public class ABWinFast : ABWin {

        public override Agent Clone () => new ABWinFast();

        protected override float EvaluateState (int ownIndex, TTTGameState gameState, int depth) {
            return 1000f * base.EvaluateState(ownIndex, gameState, depth) / depth;
        }

    }

}
