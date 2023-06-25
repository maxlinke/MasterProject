using MasterProject.MachineLearning;

namespace MasterProject.TicTacToe.MachineLearning {

    public class GenericTTTIndividual : NumericallyParametrizedIndividual<AgentParameters> {
        
        public override float GetMaximumMutationStrength () => 0.125f;

        public override Agent CreateAgent () {
            return new ParametrizedABAgent(this.agentParams, this.guid);
        }

        protected override Individual GetClone () {
            return new GenericTTTIndividual() {
                agentParams = this.agentParams.Clone()
            };
        }
    }

}
