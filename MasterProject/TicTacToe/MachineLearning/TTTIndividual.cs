using MasterProject.MachineLearning;

namespace MasterProject.TicTacToe.MachineLearning {

    public class TTTIndividual : Individual {

        // just for testing
        // the expected result is that win > draw > loss and random -> 0

        public AgentParameters agentParams { get; set; }

        public override Agent CreateAgent () {
            return new ParametrizedABAgent(this.agentParams, this.guid);
        }

        protected override void CombineCoefficients (Individual otherIndividual) {
            var other = (TTTIndividual)otherIndividual;
            var remainingFromThis = 2;
            var remainingFromOther = 2;
            SetCoeffs(
                ShouldCopyFromThis() ? this.agentParams.winScore : other.agentParams.winScore,
                ShouldCopyFromThis() ? this.agentParams.drawScore : other.agentParams.drawScore,
                ShouldCopyFromThis() ? this.agentParams.lossScore : other.agentParams.lossScore,
                ShouldCopyFromThis() ? this.agentParams.randomProbability : other.agentParams.randomProbability
            );

            bool ShouldCopyFromThis () {
                if (remainingFromThis < 1) {
                    return false;
                }
                if (remainingFromOther < 1) {
                    return true;
                }
                if (rng.NextDouble() > 0.5) {
                    remainingFromThis--;
                    return true;
                } else {
                    remainingFromOther--;
                    return false;
                }
            }
        }

        protected override Individual GetClone () {
            return new TTTIndividual() { agentParams = this.agentParams };
        }

        protected override void InvertCoefficients () {
            SetCoeffs(
                -agentParams.winScore,
                -agentParams.drawScore,
                -agentParams.lossScore,
                agentParams.randomProbability   // this isn't really a coefficient directly
            );
        }

        protected override void MutateCoefficients () {
            var mag = 0.25 * rng.NextDouble();  // to make the progress a bit slower (just for the graphs)
            AlterCoeffs(
                (rng.NextDouble() - 0.5) * 4 * mag,
                (rng.NextDouble() - 0.5) * 4 * mag,
                (rng.NextDouble() - 0.5) * 4 * mag,
                (rng.NextDouble() - 0.5) * 2 * mag
            );
        }

        protected override void RandomizeCoefficients () {
            SetCoeffs(
                (rng.NextDouble() - 0.5) * 2,
                (rng.NextDouble() - 0.5) * 2,
                (rng.NextDouble() - 0.5) * 2,
                rng.NextDouble()
            );
        }

        void SetCoeffs (double w, double d, double l, double r) {
            agentParams = new AgentParameters() {
                winScore = (float)w,
                drawScore = (float)d,
                lossScore = (float)l,
                randomProbability = (float)r
            };
            ClampCoeffs();
        }

        void AlterCoeffs (double dw, double dd, double dl, double dr) {
            agentParams = new AgentParameters() {
                winScore = agentParams.winScore + (float)dw,
                drawScore = agentParams.drawScore + (float)dd,
                lossScore = agentParams.lossScore + (float)dl,
                randomProbability = agentParams.randomProbability + (float)dr
            };
            ClampCoeffs();
        }

        void ClampCoeffs () {
            agentParams = new AgentParameters() {
                winScore = Math.Clamp(agentParams.winScore, -1, 1),
                drawScore = Math.Clamp(agentParams.drawScore, -1, 1),
                lossScore = Math.Clamp(agentParams.lossScore, -1, 1),
                randomProbability = Math.Clamp(agentParams.randomProbability, 0, 1),
            };
        }

    }
}
