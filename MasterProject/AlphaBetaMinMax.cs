using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterProject {
    public static class AlphaBetaMinMax {

        public static Move<U, T> CalculateBestMove<T, U> (GameState<T, U> sourceState, int maxDepth, int maxDurationMillis = 1000)
            where T : Move<U, T>
            where U : GameState<T, U>
        {
            // rating function?
            // also this will probably have to "maaximize" scores for each player and act upon that
            // rather than just negating the rating function for the acting player on enemy turns
            return default;
        }

	}
}
