using System.Collections.Generic;
using static MasterProject.Chess.ChessGameState;
using static MasterProject.Chess.ChessPieceUtils;
using static MasterProject.Chess.BitMaskUtils;

namespace MasterProject.Chess {

    public static class ChessMoveUtils {

        public class PossiblePositions {

            public readonly IReadOnlyList<IReadOnlyList<int>> sequentiallyReachableCoordinates;
            public readonly IReadOnlyList<int> independentlyReachableCoordinates;

            public PossiblePositions (IReadOnlyList<IReadOnlyList<int>> coordinatesInSequence, IReadOnlyList<int> independentlyReachableCoordinates) {
                this.sequentiallyReachableCoordinates = coordinatesInSequence;
                this.independentlyReachableCoordinates = independentlyReachableCoordinates;
            }

        }

        public static readonly IReadOnlyList<PossiblePositions> possibleKingPositions;
        public static readonly IReadOnlyList<PossiblePositions> possibleQueenPositions;
        public static readonly IReadOnlyList<PossiblePositions> possibleBishopPositions;
        public static readonly IReadOnlyList<PossiblePositions> possibleKnightPositions;
        public static readonly IReadOnlyList<PossiblePositions> possibleRookPositions;
        public static readonly IReadOnlyList<PossiblePositions> possibleWhitePawnMovePositions;
        public static readonly IReadOnlyList<PossiblePositions> possibleBlackPawnMovePositions;
        public static readonly IReadOnlyList<PossiblePositions> possibleWhitePawnAttackPositions;
        public static readonly IReadOnlyList<PossiblePositions> possibleBlackPawnAttackPositions;
        public static readonly int whitePawnPromotionY;
        public static readonly int blackPawnPromotionY;

        private class MoveOffset {
            public readonly int x;
            public readonly int y;
            public MoveOffset (int x, int y) {
                this.x = x;
                this.y = y;
            }
        }

        static ChessMoveUtils () {
            var squareOffsets = new MoveOffset[8];
            for (int i = 0; i < 8; i++) {
                var s = i + ((i > 3) ? 1 : 0);  // s goes from 0 to 8 without ever going to 4
                squareOffsets[i] = new MoveOffset((s / 3) - 1, (s % 3) - 1);
            }
            var knightOffsets = new MoveOffset[8];
            for (int i = 0; i < 4; i++) {
                var y = -2 + i + ((i > 1) ? 1 : 0);     // -2, -1, 1, 2
                var x = ((i < 2) ? (1 + i) : (4 - i));  // 1, 2, 2, 1
                knightOffsets[(2 * i) + 0] = new MoveOffset(x, y);
                knightOffsets[(2 * i) + 1] = new MoveOffset(-x, y);
            }
            var diagonalAxes = new MoveOffset[4][];
            var alignedAxes = new MoveOffset[4][];
            for (int i = 0; i < 4; i++) {
                diagonalAxes[i] = new MoveOffset[BOARD_SIZE - 1];
                alignedAxes[i] = new MoveOffset[BOARD_SIZE - 1];
            }
            for (int i = 1; i < BOARD_SIZE; i++) {
                diagonalAxes[0][i - 1] = new MoveOffset(i, i);
                diagonalAxes[1][i - 1] = new MoveOffset(i, -i);
                diagonalAxes[2][i - 1] = new MoveOffset(-i, -i);
                diagonalAxes[3][i - 1] = new MoveOffset(-i, i);
                alignedAxes[0][i - 1] = new MoveOffset(0, i);
                alignedAxes[1][i - 1] = new MoveOffset(0, -i);
                alignedAxes[2][i - 1] = new MoveOffset(i, 0);
                alignedAxes[3][i - 1] = new MoveOffset(-i, 0);
            }
            var queenOffsets = new MoveOffset[8][];
            for (int i = 0; i < 4; i++) {
                queenOffsets[i] = diagonalAxes[i];
                queenOffsets[i + 4] = alignedAxes[i];
            }
            possibleKingPositions = CreatePositions(squareOffsets, null);
            possibleKnightPositions = CreatePositions(knightOffsets, null);
            possibleRookPositions = CreatePositions(null, alignedAxes);
            possibleBishopPositions = CreatePositions(null, diagonalAxes);
            possibleQueenPositions = CreatePositions(null, queenOffsets);
            var initBoard = ChessPieceUtils.GetInitialBoard();
            possibleWhitePawnMovePositions = CreatePawnMovePositions(ChessPiece.WhitePawn);
            possibleBlackPawnMovePositions = CreatePawnMovePositions(ChessPiece.BlackPawn);
            possibleWhitePawnAttackPositions = CreatePawnAttackPositions(ChessPiece.WhitePawn);
            possibleBlackPawnAttackPositions = CreatePawnAttackPositions(ChessPiece.BlackPawn);
            CoordToXY(Array.IndexOf(initBoard, ChessPiece.BlackKing), out _, out whitePawnPromotionY);
            CoordToXY(Array.IndexOf(initBoard, ChessPiece.WhiteKing), out _, out blackPawnPromotionY);

            PossiblePositions[] CreatePositions (MoveOffset[] independentOffsets, MoveOffset[][] sequentialOffsets) {
                var output = new PossiblePositions[64];
                for (int i = 0; i < 64; i++) {
                    CoordToXY(i, out var x, out var y);
                    List<int> independentPositions = null;
                    List<int[]> sequentialPositions = null;
                    if (independentOffsets != null) {
                        independentPositions = new List<int>();
                        foreach (var offset in independentOffsets) {
                            var newX = x + offset.x;
                            var newY = y + offset.y;
                            if (CheckIsInbounds(newX, newY)) {
                                independentPositions.Add(XYToCoord(newX, newY));
                            }
                        }
                    }
                    if (sequentialOffsets != null) {
                        sequentialPositions = new List<int[]>();
                        foreach (var offsets in sequentialOffsets) {
                            var positions = new List<int>();
                            foreach (var offset in offsets) {
                                var newX = x + offset.x;
                                var newY = y + offset.y;
                                if (CheckIsInbounds(newX, newY)) {
                                    positions.Add(XYToCoord(newX, newY));
                                } else {
                                    break;
                                }
                            }
                            if (positions.Count > 0) {
                                sequentialPositions.Add(positions.ToArray());
                            }
                        }
                    }
                    output[i] = new PossiblePositions(
                        (sequentialPositions != null ? sequentialPositions.ToArray() : null), 
                        (independentPositions != null ? independentPositions.ToArray() : null)
                    );
                }
                return output;
            }

            PossiblePositions[] CreatePawnMovePositions (ChessPiece piece) {
                var output = new PossiblePositions[64];
                CoordToXY(Array.IndexOf(initBoard, piece), out _, out var startY);
                int dy = Math.Sign((BOARD_SIZE / 2) - startY);
                for (int x = 0; x < BOARD_SIZE; x++) {
                    output[XYToCoord(x, 0)]              = new PossiblePositions(null, null);  // can't ever be here
                    output[XYToCoord(x, BOARD_SIZE - 1)] = new PossiblePositions(null, null);  // can't ever be here
                    output[XYToCoord(x, startY)] = new PossiblePositions(new int[][]{
                        new int[]{
                            XYToCoord(x, startY + dy),
                            XYToCoord(x, startY + dy + dy),
                        }
                    }, null);
                }
                for (int i = 0; i < (BOARD_SIZE - 3); i++) {
                    var y = startY + ((i + 1) * dy);
                    for (int x = 0; x < BOARD_SIZE; x++) {
                        output[XYToCoord(x, y)] = new PossiblePositions(null, new int[] { XYToCoord(x, y + dy) });
                    }
                }
                return output;
            }

            PossiblePositions[] CreatePawnAttackPositions (ChessPiece piece) {
                var output = new PossiblePositions[64];
                CoordToXY(Array.IndexOf(initBoard, piece), out _, out var startY);
                int dy = Math.Sign((BOARD_SIZE / 2) - startY);
                for (int x = 0; x < BOARD_SIZE; x++) {
                    output[XYToCoord(x, startY - dy)]       = new PossiblePositions(null, null);
                    output[XYToCoord(x, startY + (6 * dy))] = new PossiblePositions(null, null);
                    for (int i = 0; i < 6; i++) {
                        var y = startY + (i * dy);
                        var outputCoord = XYToCoord(x, y);
                        var leftAttackCoord = XYToCoord(x - 1, y + dy);
                        var rightAttackCoord = XYToCoord(x + 1, y + dy);
                        var includeLeft = (x < (BOARD_SIZE - 1));
                        var includeRight = (x > 0);
                        if (includeLeft && includeRight) {
                            output[outputCoord] = new PossiblePositions(null, new int[] { leftAttackCoord, rightAttackCoord }); 
                        } else if (includeLeft) {
                            output[outputCoord] = new PossiblePositions(null, new int[] { leftAttackCoord });
                        } else if (includeRight) {
                            output[outputCoord] = new PossiblePositions(null, new int[] { rightAttackCoord });
                        } else {
                            throw new System.Exception("???");
                        }
                    }
                }
                return output;
            }
        }


        public static long GetAttackMap (ChessGameState gs, int coord) {
            CoordToXY(coord, out var x, out var y);
            var board = gs.board;
            var pieceColorId = (int)(board[coord]) & MASK_COLOR;
            var pieceTypeId = (int)(board[coord]) & ~MASK_COLOR;
            var positions = default(PossiblePositions);
            switch (pieceTypeId) {
                case ID_PAWN:
                    positions = board[coord].IsWhite() ? possibleWhitePawnAttackPositions[coord] : possibleBlackPawnAttackPositions[coord];
                    break;
                case ID_KNIGHT:
                    positions = possibleKnightPositions[coord];
                    break;
                case ID_BISHOP:
                    positions = possibleBishopPositions[coord];
                    break;
                case ID_ROOK:
                    positions = possibleRookPositions[coord];
                    break;
                case ID_QUEEN:
                    positions = possibleQueenPositions[coord];
                    break;
                case ID_KING:
                    positions = possibleKingPositions[coord];
                    break;
                default:
                    throw new System.NotImplementedException($"Unknown value \"{pieceTypeId}\"!");
            }
            var output = 0L;
            if (positions.independentlyReachableCoordinates != null) {
                foreach (var reachableCoord in positions.independentlyReachableCoordinates) {
                    if (((int)(board[reachableCoord]) & MASK_COLOR) != pieceColorId) {   // don't attack same color
                        output |= (1L << reachableCoord);
                    }
                }
            }
            if (positions.sequentiallyReachableCoordinates != null) {
                foreach (var coords in positions.sequentiallyReachableCoordinates) {
                    foreach (var reachableCoord in coords) {
                        if (((int)(board[reachableCoord]) & MASK_COLOR) != pieceColorId) {   // don't attack same color
                            break;
                        }
                        output |= (1L << reachableCoord);
                        if (board[reachableCoord] != ChessPiece.None) {     // don't attack through enemies
                            break;
                        }
                    }
                }
            }
            return output;
        }

        // TODO rename this (like, what's a "regular" move anyways...)
        public static IEnumerable<ChessMove> GetMovesForPiece (ChessGameState gs, int coord) {
            CoordToXY(coord, out var x, out var y);
            var board = gs.board;
            var pieceColorId = (int)(board[coord]) & MASK_COLOR;
            var piecePlayerIndex = (pieceColorId == ID_WHITE ? ChessGameState.INDEX_WHITE : ChessGameState.INDEX_BLACK);
            var pieceTypeId = (int)(board[coord]) & ~MASK_COLOR;
            IEnumerable<ChessMove> moves;
            switch (pieceTypeId) {
                case ID_PAWN:
                    moves = GetPawnMoves(x, y);
                    break;
                case ID_KNIGHT:
                    moves = GetKnightMoves(x, y);
                    break;
                case ID_BISHOP:
                    moves = GetBishopMoves(x, y);
                    break;
                case ID_ROOK:
                    moves = GetRookMoves(x, y);
                    break;
                case ID_QUEEN:
                    moves = GetQueenMoves(x, y);
                    break;
                case ID_KING:
                    moves = GetRegularKingMoves(x, y);
                    break;
                default:
                    throw new System.NotImplementedException($"Unknown value \"{pieceTypeId}\"!");
            }
            foreach (var move in moves) {
                yield return move;
            }

            IEnumerable<ChessMove> GetVacantOrCaptureMoves (IEnumerable<MoveOffset> offsets, int x, int y, bool breakIfImpossible) {
                foreach (var offset in offsets) {
                    var newX = x + offset.x;
                    var newY = y + offset.y;
                    if (CheckIsInbounds(newX, newY)) {
                        var newCoord = XYToCoord(newX, newY);
                        var isValidCoord = false;
                        if (board[newCoord] == ChessPiece.None) {
                            isValidCoord = true;
                        } else {
                            var colorAtCoord = (int)(board[newCoord]) & MASK_COLOR;
                            var pieceAtCoord = (int)(board[newCoord]) & ~MASK_COLOR;
                            if (colorAtCoord != pieceColorId) {
                                isValidCoord = (pieceAtCoord != ID_KING);
                            }
                        }
                        if (isValidCoord) {
                            yield return new ChessMove() {
                                srcCoord = coord,
                                dstCoord = newCoord
                            };
                        } else if (breakIfImpossible) {
                            yield break;
                        }
                    } else if (breakIfImpossible) {
                        yield break;
                    }
                }
            }

            IEnumerable<ChessMove> GetPawnMoves (int x, int y) {
                // TODO 
                // remember the conversions
                // remember en passant
                throw new NotImplementedException();
            }

            IEnumerable<ChessMove> GetKnightMoves (int x, int y) {

                throw new NotImplementedException();
            }

            IEnumerable<ChessMove> GetBishopMoves (int x, int y) {

                throw new NotImplementedException();
            }

            IEnumerable<ChessMove> GetRookMoves (int x, int y) {

                throw new NotImplementedException();
            }

            IEnumerable<ChessMove> GetQueenMoves (int x, int y) {
                foreach (var move in GetBishopMoves(x, y)) {
                    yield return move;
                }
                foreach (var move in GetRookMoves(x, y)) {
                    yield return move;
                }
            }

            IEnumerable<ChessMove> GetRegularKingMoves (int x, int y) {

                throw new NotImplementedException();
                if (!gs.playerStates[piecePlayerIndex].HasCastled && !gs.playerStates[piecePlayerIndex].IsInCheck) {
                    if (!gs.GetPositionHasBeenMoved(x, y)) {
                        // The king and rook involved in castling must not have previously moved;
                        // There must be no pieces between the king and the rook;
                        // The king may not currently be under attack,
                        // .. nor may the king pass through or end up in a square that is under attack by an enemy piece
                        // .. (though the rook is permitted to be under attack and to pass over an attacked square)
                        // The castling must be kingside or queenside as shown in the diagram.
                        throw new System.NotImplementedException();
                    }
                }
            }
        }

    }

}
