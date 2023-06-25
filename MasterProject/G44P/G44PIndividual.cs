using MasterProject.MachineLearning;
using MasterProject.G44P.Agents;
using MasterProject.G44P.RatingFunctions;

namespace MasterProject.G44P 
    {
    public class G44PIndividual : NumericallyParametrizedIndividual<ParametrizedRatingFunction.Parameters> {

        public override float GetMaximumMutationStrength () => 0.25f;

        public override Agent CreateAgent () {
            return new IgnoreOpponentMoves(new ParametrizedRatingFunction(agentParams, this.guid), 8);
        }

        protected override Individual GetClone () {
            return new G44PIndividual() { agentParams = this.agentParams.Clone() };
        }

    }
}
