using MasterProject.MachineLearning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterProject.G44P.RatingFunctions {

    public class ParametrizedRatingFunction : RatingFunction {

        public override string Id => $"{base.Id}_{nameSuffix}";

        public class Parameters : IParameterListConvertible<float>, IParameterRangeProvider<float> {

            public float ownScoreMultiplier { get; set; } = 0;
            public float[] otherScoreMultipliers { get; set; } = new float[3];

            public Parameters Clone () {
                return new Parameters() {
                    ownScoreMultiplier = this.ownScoreMultiplier,
                    otherScoreMultipliers = (float[])this.otherScoreMultipliers.Clone()
                };
            }

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

            IReadOnlyList<float> IParameterListConvertible<float>.GetParameterList () {
                var output = new List<float>();
                output.Add(ownScoreMultiplier);
                output.AddRange(otherScoreMultipliers);
                return output;
            }

            void IParameterListConvertible<float>.ApplyParameterList (IReadOnlyList<float> parameterList) {
                ownScoreMultiplier = parameterList[0];
                otherScoreMultipliers = new float[parameterList.Count - 1];
                for (int i = 1; i < parameterList.Count; i++) {
                    otherScoreMultipliers[i - 1] = parameterList[i];
                }
            }

            ParameterRange<float> IParameterRangeProvider<float>.GetRangeForParameterAtIndex (int index) {
                return new ParameterRange<float>() {
                    min = -1,
                    max = 1
                };
            }

        }

        public Parameters parameters;
        private string nameSuffix;

        public ParametrizedRatingFunction (Parameters parameters) {
            this.parameters = parameters;
            this.nameSuffix = $"ParamsHash{this.parameters.GetHashCode()}";
        }

        public ParametrizedRatingFunction (Parameters parameters, string nameSuffix) {
            this.parameters = parameters;
            this.nameSuffix = nameSuffix;
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
