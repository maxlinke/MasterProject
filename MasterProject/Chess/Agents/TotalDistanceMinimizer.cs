using System.Collections.Generic;

namespace MasterProject.Chess.Agents {

    public abstract class TotalDistanceMinimizer : ChessAgent {

        public override bool IsStateless => true;

        public override bool IsTournamentEligible => true;

        protected abstract int GetTargetCoordForResultGamestate (ChessGameState initState, ChessGameState resultState);

        public override int GetMoveIndex (ChessGameState gameState, IReadOnlyList<ChessMove> moves) {
            var totalDists = new float[moves.Count];
            for (int i = 0; i < moves.Count; i++) {
                var result = gameState.GetResultOfMove(moves[i]);
                totalDists[i] = 0;
                var targetCoord = GetTargetCoordForResultGamestate(gameState, result);
                foreach (var coord in result.CoordsWithPiecesOfPlayer(gameState.currentPlayerIndex)) {
                    totalDists[i] += ChessGameStateUtils.ChebyshevDistanceBetweenCoords(targetCoord, coord);
                }
            }
            return GetIndexOfMinimum(totalDists, true);
        }

    }
}
