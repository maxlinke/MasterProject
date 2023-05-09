using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterProject.TicTacToe.Agents {

    public class GenericMinMaxUser : TTTAgent {

        public override int GetMoveIndex (TTTGameState gameState, IReadOnlyList<TTTMove> moves) {
            var ownIndex = gameState.CurrentPlayerIndex;
            return AlphaBetaMinMax.GetBestMoveIndex(
                gameState, 
                moves, 
                9, 
                (gs, depth) => {
                    if (gs.GameOver) {
                        if (gs.winnerIndex == ownIndex) {
                            return (depth <= 1 ? float.PositiveInfinity : 1);
                        }
                        return (gs.IsDraw ? 0 : -1);
                    }
                    throw new NotImplementedException("This will never happen");
                }
            );
        }

    }

}
