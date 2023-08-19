using System.Collections.Generic;

namespace MasterProject.Chess.Agents {

    public class CCCP : ChessAgent {

        // implementation details via https://sourceforge.net/p/tom7misc/svn/HEAD/tree/trunk/chess/
        // file: player.cc
        // line: 118 and onwards

        public override bool IsStateless => true;

        public override bool IsTournamentEligible => true;

        public override string Id => $"{base.Id}_{(deterministic ? "Det" : "NonDet")}";

        bool deterministic;

        public override Agent Clone () {
            return new CCCP(this.deterministic);
        }

        public CCCP (bool deterministic) {
            this.deterministic = deterministic;
        }

        public override int GetMoveIndex (ChessGameState gameState, IReadOnlyList<ChessMove> moves) {
            var scores = new float[moves.Count];
            for (int i = 0; i < moves.Count; i++) {
                var result = gameState.GetResultOfMove(moves[i]);
                if (result.playerStates[gameState.currentPlayerIndex].HasWon) {
                    scores[i] = 10000;
                    continue;
                }
                if (result.playerStates[result.currentPlayerIndex].IsInCheck) {
                    scores[i] = 9000;
                    continue;
                }
                if (gameState.board[moves[i].dstCoord] != ChessPiece.None) {
                    scores[i] = 8000 + GetRelativePieceValue(gameState.board[moves[i].dstCoord]);
                    continue;
                }
                ChessGameStateUtils.CoordToXY(moves[i].dstCoord, out var dstX, out var dstY);
                scores[i] = (100 * dstY) + (10 * (4 - GetDistanceFromCenterLine(dstX)));
                if (moves[i].promoteTo != ChessPiece.None) {
                    scores[i] += GetRelativePieceValue(moves[i].promoteTo);
                }
            }

            return GetIndexOfMaximum(scores, randomTieBreaker: !deterministic);
        }

        static int GetRelativePieceValue (ChessPiece piece) {
            if (piece.IsPawn())
                return 1;
            if (piece.IsBishop() || piece.IsKnight())
                return 3;
            if (piece.IsRook())
                return 5;
            if (piece.IsQueen())
                return 9;
            throw new System.ArgumentException($"Piece \"{piece}\" should not appear here!");
        }

        static int GetDistanceFromCenterLine (int x) {
            switch (x) {
                case 0:
                case 7:
                    return 3;
                case 1:
                case 6:
                    return 2;
                case 2:
                case 5:
                    return 1;
                case 3:
                case 4:
                    return 0;
                default:
                    throw new System.ArgumentException($"\"{x}\" is not a valid value!");
            }

        }

    }

}
