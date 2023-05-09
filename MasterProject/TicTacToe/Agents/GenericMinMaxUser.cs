using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterProject.TicTacToe.Agents {

    public class GenericMinMaxUser : TTTAgent {

        public override int GetMoveIndex (TTTGameState gameState, IReadOnlyList<TTTMove> moves) {
            var ownIndex = gameState.CurrentPlayerIndex;
            var scores = AlphaBetaMinMax.RateMoves(
                initialGameState: gameState, 
                moves: moves, 
                maxDepth: 9, 
                maximize: (gs, depth) => gs.CurrentPlayerIndex == ownIndex,
                evaluate: (gs, depth) => {
                    if (!gs.GameOver)
                        throw new NotImplementedException("This should not happen, min max is set up to explore the entire game tree but no game over was reached!");
                    if (gs.winnerIndex == ownIndex)
                        return (depth <= 1 ? float.PositiveInfinity : 1);
                    return (gs.IsDraw ? 0 : -1);
                }
            );
            return GetIndexOfMaximum(scores, true);
        }

    }

}
