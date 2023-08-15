using MasterProject.G44P.RatingFunctions;


namespace MasterProject.G44P.Agents {

    public class ABIgnore : G44PAgent {

        public override string Id => $"{base.Id}_{this.ratingFunction.Id}_Depths{this.maxAbDepth},{maxIgnoreDepth}";

        public override bool IsStateless => true;

        public override bool IsTournamentEligible => true;

        public override Agent Clone () {
            return new ABIgnore(ratingFunction, maxAbDepth, maxIgnoreDepth);
        }

        public override int GetMoveIndex (G44PGameState gameState, IReadOnlyList<G44PMove> moves) {
            var ownIndex = gameState.currentPlayerIndex;
            var ratings = AlphaBetaMinMax.RateMoves(
                initialGameState: gameState,
                moves: moves,
                maxDepth: this.maxAbDepth,
                maximize: (gs, _) => (gs.CurrentPlayerIndex == ownIndex),
                evaluate: (gs, depth) => {
                    var gsCopy = gs.Clone();
                    var ignoreDepth = 0;
                    while (ignoreDepth < maxIgnoreDepth && !gsCopy.GameOver) {
                        gsCopy.ApplyMove(default);
                        ignoreDepth++;
                    }
                    return ratingFunction.Evaluate(ownIndex, gsCopy, depth + ignoreDepth);
                }
            );
            return GetIndexOfMaximum(ratings, true);
        }

        RatingFunction ratingFunction;
        int maxAbDepth;
        int maxIgnoreDepth;

        public ABIgnore (RatingFunction ratingFunction, int maxAbDepth, int maxIgnoreDepth) {
            this.ratingFunction = ratingFunction;
            this.maxAbDepth = maxAbDepth;
            this.maxIgnoreDepth = maxIgnoreDepth;
        }
    }

}
