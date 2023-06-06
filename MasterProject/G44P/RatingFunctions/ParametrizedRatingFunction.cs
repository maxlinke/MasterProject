using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterProject.G44P.RatingFunctions {

    public class ParametrizedRatingFunction : RatingFunction {

        public override string Id => $"{base.Id}_RatingParamsHash{this.GetHashCode()}";

        public class Parameters {

            public float ownScoreMultiplier { get; set; }
            public float[] otherScoreMultipliers { get; set; }

            public override bool Equals (object? obj) {
                return obj is Parameters other
                    && other.GetHashCode() == this.GetHashCode()
                ;       
            }

            public override int GetHashCode () {
                var output = ownScoreMultiplier.GetHashCode();
                if (otherScoreMultipliers != null) {
                    foreach (var multiplier in otherScoreMultipliers) {
                        output ^= multiplier.GetHashCode();
                    }
                }
                return output;
            }

        }

        public Parameters parameters;

        public ParametrizedRatingFunction (Parameters parameters) {
            this.parameters = parameters;
        }

        public override float Evaluate (int playerIndex, G44PGameState gameState, int depth) {
            var output = 0f;
            output += gameState.PlayerStates[playerIndex].Points * parameters.ownScoreMultiplier;
            var ignoreIndex = 1 << playerIndex;
            for (int i = 0; i < parameters.otherScoreMultipliers.Length; i++) {
                var maxPoints = -1;
                var maxPointsIndex = -1;
                for (int j = 0; j < gameState.PlayerStates.Count; j++) {
                    if ((ignoreIndex & (1 << j)) == 0) {
                        if (gameState.PlayerStates[j].Points > maxPoints) {
                            maxPoints = gameState.PlayerStates[j].Points;
                            maxPointsIndex = j;
                        }
                    }
                }
                output += gameState.PlayerStates[maxPointsIndex].Points * parameters.otherScoreMultipliers[i];
                ignoreIndex |= (1 << maxPointsIndex);
            }
            return output;
        }

    }

}
