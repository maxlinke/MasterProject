using MasterProject.TicTacToe.Agents;

namespace MasterProject.TicTacToe.MachineLearning {

    public class ParametrizedABAgent : GenericAlphaBetaAgent {

        private readonly AgentParameters agentParams;
        private readonly string guid;

        public override string Id => $"{base.Id}_{this.guid}";

        public ParametrizedABAgent (AgentParameters agentParams, string guid) {
            this.agentParams = agentParams;
            this.guid = guid;
        }

        public override Agent Clone () {
            return new ParametrizedABAgent(this.agentParams, this.guid);
        }

        protected override float EvaluateState (int ownIndex, TTTGameState gameState, int depth) {
            if (!gameState.GameOver) {
                throw new NotImplementedException();
            }
            var rawScore = (gameState.IsDraw 
                            ? agentParams.drawScore
                            : ((gameState.WinnerIndex == ownIndex) 
                                ? agentParams.winScore 
                                : agentParams.lossScore
                            ));
            return (1000f * rawScore) / depth;
        }

        protected override int GetMaxDepth (TTTGameState gameState, IReadOnlyList<TTTMove> moves) {
            return 9;
        }

        protected override bool MaximizeAtState (int ownIndex, TTTGameState gameState, int depth) {
            return gameState.CurrentPlayerIndex == ownIndex;
        }

        protected override int SelectOutputIndexFromScores (IReadOnlyList<float> scores) {
            if (rng.NextDouble() < agentParams.randomProbability) {
                return rng.Next(0, scores.Count);
            }
            return GetIndexOfMaximum(scores, true);
        }

    }

}
