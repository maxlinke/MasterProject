﻿using System;
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
                kingDistances[i] = ChessGameStateUtils.ChebyshevDistanceBetweenCoords(result.playerStates[0].KingCoord, result.playerStates[1].KingCoord);
            }
            return GetIndexOfMinimum(kingDistances, true);
        }

    }

}