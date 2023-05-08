using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterProject.TicTacToe.Agents {

    public class RandomAgentWithLookAhead : RandomAgent {

        // there's nothing ttt specific about this or the other agent
        // in fact, the random move could always be implemented as a fallback option in agent
        // and both of these could become default agents
        // although they'd still have to be abstract because of the this.GetType().FullName id which gets REALLY messy with generic types
        // also depending on how i save the agent data, if i do it by namespace, then only the proper implementation have those
        // and of course this would need to analyze the possible outcomes and take the winning outcome with the highest probability rather than the ttt-specific thing

        public override int GetMoveIndex (TTTGame game, IReadOnlyList<TTTMove> moves) {
            var gs = game.GetCurrentGameStateVisibleForAgent(this);
            for(int i=0; i<moves.Count; i++){
                var moveResult = gs.GetResultOfMove(moves[i]);
                if (moveResult.GameOver && moveResult.winnerIndex == gs.CurrentPlayerIndex) {
                    return i;
                }
            }
            return base.GetMoveIndex(game, moves);
        }

    }

}
