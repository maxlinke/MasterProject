using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterProject.TicTacToe.Agents {

    public class LineBuilder : TTTAgent {

        public override bool IsStateless => true;

        public override bool IsTournamentEligible => true;

        public override Agent Clone () => new LineBuilder();

        public override int GetMoveIndex (TTTGameState gs, IReadOnlyList<TTTMove> moves) {
            var ownIndex = gs.CurrentPlayerIndex;
            var possibleLineMoves = new List<int>();
            var leastMovesRemaining = int.MaxValue;
            // probably more?
            // need to sort by "how far am i from my goal of a completed line"
            // hm, i could score the moves
            // if this move completes a line, perfect. take that. 
            // if with this move i'm one more move from a complete line, that's great too
            // and so on
            // so i'm looking for the moves with the fewest moves to complete a line
            for(int i=0; i<moves.Count; i++){
                var move = moves[i];
                var result = gs.GetResultOfMove(move);
                foreach (var line in TTTGameState.lines) {
                    var possible = true;
                    var remaining = 0;
                    foreach (var field in line) {
                        var isEmpty = result.board[field] == TTTGameState.EMPTY_FIELD;
                        var isOwnField = result.board[field] == ownIndex;
                        possible &= (isEmpty || isOwnField);
                        remaining += (isEmpty ? 1 : 0);
                    }
                    if (possible) {
                        if (remaining < leastMovesRemaining) {
                            leastMovesRemaining = remaining;
                            possibleLineMoves.Clear();
                        }
                        if(remaining <= leastMovesRemaining){
                            possibleLineMoves.Add(i);
                        }
                    }
                }
            }
            if (possibleLineMoves.Count > 0) {
                return possibleLineMoves[rng.Next(possibleLineMoves.Count)];
            }
            return Agent.GetRandomMoveIndex(moves);
        }

    }

}
