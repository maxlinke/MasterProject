using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterProject.G44P.RatingFunctions {

    public class MaximizeOwnScore : RatingFunction {

        public override float Evaluate (int playerIndex, G44PGameState gameState, int depth) {
            return gameState.playerStates[playerIndex].Points;
        }

    }

}
