using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MasterProject.G44P.RatingFunctions;

namespace MasterProject.G44P.Agents {

    public class ABAgent : G44PAgent {

        public override string Id => $"{base.Id}_{this.ratingFunction.Id}_Depth{this.maxDepth}";

        public override bool IsStateless => true;

        public override bool IsTournamentEligible => true;

        public override int GetMoveIndex (G44PGameState gameState, IReadOnlyList<G44PMove> moves) {
            var ownIndex = gameState.currentPlayerIndex;
            var ratings = AlphaBetaMinMax.RateMoves(
                initialGameState: gameState,
                moves: moves,
                maxDepth: this.maxDepth,
                maximize: (gs, _) => (gs.CurrentPlayerIndex == ownIndex),
                evaluate: (gs, depth) => ratingFunction.Evaluate(ownIndex, gs, depth)
            );
            return GetIndexOfMaximum(ratings, true);
        }

        public override Agent Clone () {
            return new ABAgent(this.ratingFunction, this.maxDepth);
        }

        RatingFunction ratingFunction;
        int maxDepth;

        public ABAgent (RatingFunction ratingFunction, int maxDepth) {
            this.ratingFunction = ratingFunction;
            this.maxDepth = maxDepth;
        }

    }

}
