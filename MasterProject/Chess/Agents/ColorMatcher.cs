using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterProject.Chess.Agents {

    public abstract class ColorMatcher : ChessAgent {

        public override bool IsStateless => true;

        public override bool IsTournamentEligible => true;

        public abstract bool GetShouldPutPiecesOnWhiteSquares (ChessGameState gameState);

        public override int GetMoveIndex (ChessGameState gameState, IReadOnlyList<ChessMove> moves) {
            var matchingColorCounter = new float[moves.Count];
            var matchColorId = (gameState.currentPlayerIndex == ChessGameState.INDEX_WHITE ? ChessPieceUtils.ID_WHITE : ChessPieceUtils.ID_BLACK);
            var checkCoords = GetShouldPutPiecesOnWhiteSquares(gameState) ? ChessGameStateUtils.whiteFieldCoords : ChessGameStateUtils.blackFieldCoords;
            for (int i = 0; i < moves.Count; i++) {
                var result = gameState.GetResultOfMove(moves[i]);
                matchingColorCounter[i] = 0;
                foreach (var coord in checkCoords) {
                    var piece = result.board[coord];
                    var pieceColorId = ((int)piece) & ChessPieceUtils.MASK_COLOR;
                    if (pieceColorId == matchColorId) {
                        matchingColorCounter[i] += 1;
                    }
                }
            }
            return GetIndexOfMaximum(matchingColorCounter, true);
        }

    }

}
