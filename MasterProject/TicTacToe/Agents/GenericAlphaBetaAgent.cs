using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterProject.TicTacToe.Agents {

    public abstract class GenericAlphaBetaAgent : TTTAgent {

        public override int GetMoveIndex (TTTGameState gameState, IReadOnlyList<TTTMove> moves) {
            var ownIndex = gameState.CurrentPlayerIndex;
            var scores = AlphaBetaMinMax.RateMoves(
                initialGameState: gameState,
                moves: moves,
                maxDepth: GetMaxDepth(gameState, moves),
                maximize: (gs, depth) => MaximizeAtState(ownIndex, gs, depth),
                evaluate: (gs, depth) => EvaluateState(ownIndex, gs, depth)
            );
            return SelectOutputIndexFromScores(scores);
        }

        protected abstract int GetMaxDepth (TTTGameState gameState, IReadOnlyList<TTTMove> moves);

        protected abstract bool MaximizeAtState (int ownIndex, TTTGameState gameState, int depth);

        protected abstract float EvaluateState (int ownIndex, TTTGameState gameState, int depth);

        protected abstract int SelectOutputIndexFromScores (IReadOnlyList<float> scores);

    }

}
