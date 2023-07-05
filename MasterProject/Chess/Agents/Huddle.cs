using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterProject.Chess.Agents {

    public class Huddle : ChessAgent {

        public override bool IsStateless => true;

        public override bool IsTournamentEligible => true;

        public override Agent Clone () {
            return new Huddle();
        }

        public override int GetMoveIndex (ChessGameState gameState, IReadOnlyList<ChessMove> moves) {
            var totalDists = new float[moves.Count];
            for (int i = 0; i < moves.Count; i++) {
                var result = gameState.GetResultOfMove(moves[i]);
                totalDists[i] = 0;
                var kingCoord = result.playerStates[gameState.currentPlayerIndex].KingCoord;
                foreach (var coord in result.CoordsWithPiecesOfPlayer(gameState.currentPlayerIndex)) {
                    totalDists[i] += ChessGameStateUtils.ChebyshevDistanceBetweenCoords(kingCoord, coord);
                }
            }
            return GetIndexOfMinimum(totalDists, true);
        }
    }

}
