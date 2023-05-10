using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterProject.TicTacToe.Agents {

    public class ABLose : ABWin {

        protected override int SelectOutputIndexFromScores (IReadOnlyList<float> scores) {
            //var sb = new System.Text.StringBuilder();
            //foreach (var score in scores) {
            //    sb.Append($"{score:F1}, ");
            //}
            //var output = GetIndexOfMinimum(scores, true);
            //Console.WriteLine($"{sb}\n{output} -> {scores[output]}");
            //return output;
            return GetIndexOfMinimum(scores, true);
        }

    }

}
