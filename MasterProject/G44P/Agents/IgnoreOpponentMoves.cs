using MasterProject.G44P.RatingFunctions;

namespace MasterProject.G44P.Agents {

    public class IgnoreOpponentMoves : G44PAgent {

        public override bool IsStateless => true;

        public override bool IsTournamentEligible => true;

        public override string Id => $"{base.Id}_{this.ratingFunction.Id}_Depth{this.maxDepth}";

        RatingFunction ratingFunction;
        int maxDepth;

        public IgnoreOpponentMoves (RatingFunction ratingFunction, int maxDepth) {
            this.ratingFunction = ratingFunction;
            this.maxDepth = maxDepth;
        }


        public override Agent Clone () {
            return new IgnoreOpponentMoves(this.ratingFunction, this.maxDepth);
        }

        public override int GetMoveIndex (G44PGameState gameState, IReadOnlyList<G44PMove> moves) {
            var ownIndex = gameState.currentPlayerIndex;
            var scores = new float[moves.Count];
            for(int i=0; i<moves.Count; i++) {
                var gsCopy = gameState.GetResultOfMove(moves[i]);
                var depth = 1;
                while (depth < maxDepth && !gsCopy.GameOver) {
                    gsCopy.ApplyMove(default);
                    depth++;
                }
                scores[i] = ratingFunction.Evaluate(ownIndex, gsCopy, depth);
            }
            return GetIndexOfMaximum(scores, true);
        }
    }

}
