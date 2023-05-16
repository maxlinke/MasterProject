using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterProject.TicTacToe.Agents {
    
    public class ABLoseFast : ABWinFast {

        public override Agent Clone () => new ABLoseFast();

        protected override int SelectOutputIndexFromScores (IReadOnlyList<float> scores) {
            return GetIndexOfMinimum(scores, true);
        }

    }

}
