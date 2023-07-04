using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterProject.Chess.Agents {

    public class SuicideKing : ChessAgent {

        public override bool IsStateless => true;

        public override bool IsTournamentEligible => true;

        public override Agent Clone () {
            return new SuicideKing();
        }

        public override int GetMoveIndex (ChessGameState gameState, IReadOnlyList<ChessMove> moves) {
            float[] kingDistances = new float[moves.Count];
            for (int i = 0; i < moves.Count; i++) {
                var result = gameState.GetResultOfMove(moves[i]);
                ChessGameStateUtils.CoordToXY(result.PlayerStates[0].KingCoord, out var x0, out var y0);
                ChessGameStateUtils.CoordToXY(result.PlayerStates[1].KingCoord, out var x1, out var y1);
                var dx = (x1 - x0);
                var dy = (y1 - y0);
                kingDistances[i] = (float)Math.Sqrt((dx * dx) + (dy * dy));
            }
            return GetIndexOfMinimum(kingDistances, true);
        }

    }

}
