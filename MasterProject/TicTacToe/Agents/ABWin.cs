using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterProject.TicTacToe.Agents {

    public class ABWin : GenericAlphaBetaAgent {

        public override Agent Clone () => new ABWin();

        protected override int GetMaxDepth (TTTGameState gameState, IReadOnlyList<TTTMove> moves) {
            return 9;
        }

        protected override float EvaluateState (int ownIndex, TTTGameState gameState, int depth) {
            if (!gameState.GameOver)
                throw new NotImplementedException();
            if (gameState.IsDraw)
                return 0;
            return (gameState.winnerIndex == ownIndex) ? 1 : -1;
        }

        protected override bool MaximizeAtState (int ownIndex, TTTGameState gameState, int depth) {
            return gameState.CurrentPlayerIndex == ownIndex;
        }

        protected override int SelectOutputIndexFromScores (IReadOnlyList<float> scores) {
            return GetIndexOfMaximum(scores, true);
        }
    }

}
