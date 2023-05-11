using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterProject.TicTacToe.Agents {

    public class ABLose : ABWin {

        protected override int SelectOutputIndexFromScores (IReadOnlyList<float> scores) {
            return GetIndexOfMinimum(scores, true);
        }

    }

}
