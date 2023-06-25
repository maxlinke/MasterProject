using MasterProject.MachineLearning;

namespace MasterProject.TicTacToe.MachineLearning {

    public class AgentParameters : IParameterListConvertible<float>, IParameterRangeProvider<float> {

        public float winScore { get; set; }
        public float drawScore { get; set; }
        public float lossScore { get; set; }
        public float randomProbability { get; set; }

        public AgentParameters Clone () {
            return new AgentParameters() {
                winScore = this.winScore,
                drawScore = this.drawScore,
                lossScore = this.lossScore,
                randomProbability = this.randomProbability
            };
        }

        public override bool Equals (object? obj) {
            return obj is AgentParameters parameters
                && winScore == parameters.winScore
                && drawScore == parameters.drawScore
                && lossScore == parameters.lossScore
                && randomProbability == parameters.randomProbability;
        }

        public override int GetHashCode () {
            return winScore.GetHashCode()
                 ^ drawScore.GetHashCode()
                 ^ lossScore.GetHashCode()
                 ^ randomProbability.GetHashCode();
        }

        void IParameterListConvertible<float>.ApplyParameterList (IReadOnlyList<float> parameterList) {
            this.winScore = parameterList[0];
            this.drawScore = parameterList[1];
            this.lossScore = parameterList[2];
            this.randomProbability = parameterList[3];
        }

        IReadOnlyList<float> IParameterListConvertible<float>.GetParameterList () {
            return new float[]{
                winScore,
                drawScore,
                lossScore,
                randomProbability
            };
        }

        ParameterRange<float> IParameterRangeProvider<float>.GetRangeForParameterAtIndex (int index) {
            if (index == 3) {
                return new ParameterRange<float>(min: 0, max: 1);
            }
            return new ParameterRange<float>(min: -1, max: 1);
        }

        bool IParameterRangeProvider<float>.GetParameterIsInvertible (int index) {
            return (index != 3);
        }

        public static bool operator == (AgentParameters a, AgentParameters b) {
            return a.Equals(b);
        }

        public static bool operator != (AgentParameters a, AgentParameters b) {
            return !(a.Equals(b));
        }

    }

}
