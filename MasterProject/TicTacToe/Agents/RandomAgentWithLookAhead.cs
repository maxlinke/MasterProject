using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterProject.TicTacToe.Agents {

    public class RandomAgentWithLookAhead : RandomAgent {

        public override Agent Clone () => new RandomAgentWithLookAhead();

        public override int GetMoveIndex (TTTGameState gs, IReadOnlyList<TTTMove> moves) {
            for(int i=0; i<moves.Count; i++){
                var moveResult = gs.GetResultOfMove(moves[i]);
                if (moveResult.GameOver && moveResult.winnerIndex == gs.CurrentPlayerIndex) {
                    return i;
                }
            }
            return base.GetMoveIndex(gs, moves);
        }

    }

}
