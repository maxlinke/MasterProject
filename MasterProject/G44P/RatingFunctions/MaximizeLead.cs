using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterProject.G44P.RatingFunctions {

    public class MaximizeLead : RatingFunction {

        public override float Evaluate (int playerIndex, G44PGameState gameState, int depth) {
            var ownPoints = 0;
            var nextPoints = 0;
            for (int i = 0; i < gameState.PlayerStates.Count; i++) {
                if (i == playerIndex) {
                    ownPoints = gameState.PlayerStates[i].Points;
                } else {
                    nextPoints = Math.Max(nextPoints, gameState.PlayerStates[i].Points);
                }
            }
            return ownPoints - nextPoints;
        }

    }

}
