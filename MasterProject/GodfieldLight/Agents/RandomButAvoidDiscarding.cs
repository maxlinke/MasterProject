using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterProject.GodfieldLight.Agents {

    public class RandomButAvoidDiscarding : GodfieldAgent {

        public override bool IsStateless => true;

        public override bool IsTournamentEligible => true;

        public override Agent Clone () {
            return new RandomButAvoidDiscarding();
        }

        public override int GetMoveIndex (GodfieldGameState gameState, IReadOnlyList<GodfieldMove> moves) {
            if (!gameState.currentPlayerWasHit) {
                var nonDiscardMoves = new List<int>();
                for (int i = 0; i < moves.Count; i++) {
                    var move = moves[i];
                    if (move.attack != null || move.healValue > 0) {
                        nonDiscardMoves.Add(i);
                    }
                }
                if (nonDiscardMoves.Count > 0) {
                    return nonDiscardMoves[rng.Next(nonDiscardMoves.Count)];
                }
            }
            return GetRandomMoveIndex(moves);
        }
    }

}
