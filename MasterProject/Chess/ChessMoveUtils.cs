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
        public static readonly int whitePawnHomeRowY;
        public static readonly int blackPawnHomeRowY;
        public static readonly int whitePawnEnPassantStartY;
        public static readonly int blackPawnEnPassantStartY;

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
            CoordToXY(Array.IndexOf(initBoard, ChessPiece.WhitePawn), out _, out whitePawnHomeRowY);
            CoordToXY(Array.IndexOf(initBoard, ChessPiece.BlackPawn), out _, out blackPawnHomeRowY);
            whitePawnEnPassantStartY = blackPawnHomeRowY + (2 * Math.Sign(whitePawnHomeRowY - blackPawnHomeRowY));
            blackPawnEnPassantStartY = whitePawnHomeRowY + (2 * Math.Sign(blackPawnHomeRowY - whitePawnHomeRowY));

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
            foreach (var reachableCoord in EnumerateVacantOrEnemyCoords(board, positions, pieceColorId)) {
                output |= (1L << reachableCoord);
            }
            return output;
        }

        public static bool CheckCoordIsUnderAttack (long attackMap, int coord) {
            return ((attackMap >> coord) & 1L) == 1L;
        }

        private static IEnumerable<int> EnumerateVacantOrEnemyCoords (ChessPiece[] board, PossiblePositions positions, int ownColorId) {
            if (positions.independentlyReachableCoordinates != null) {
                foreach (var reachableCoord in positions.independentlyReachableCoordinates) {
                    if (((int)(board[reachableCoord]) & MASK_COLOR) != ownColorId) {   // don't attack same color
                        yield return reachableCoord;
                    }
                }
            }
            if (positions.sequentiallyReachableCoordinates != null) {
                foreach (var reachableCoords in positions.sequentiallyReachableCoordinates) {
                    foreach (var reachableCoord in reachableCoords) {
                        if (((int)(board[reachableCoord]) & MASK_COLOR) == ownColorId) {   // don't attack same color
                            break;
                        }
                        yield return reachableCoord;
                        if (board[reachableCoord] != ChessPiece.None) {     // don't attack through enemies
                            break;
                        }
                    }
                }
            }
        }

        public static bool CheckForEnPassant (ChessGameState gs, ChessMove move) {
            // TODO
            // check "behind" the move coord (so dst.x but src.y) NOW for enemy pawn and previously nothing
            // check "in front" the move coord (dst.y, enemy homerow) NOW empty, previously occupied
            throw new NotImplementedException();
        }

        public static IEnumerable<ChessMove> GetLegalMovesForPiece (ChessGameState gs, int coord) {
            var board = gs.board;
            var pieceColorId = (int)(board[coord]) & MASK_COLOR;
            var piecePlayerIndex = (pieceColorId == ID_WHITE ? ChessGameState.INDEX_WHITE : ChessGameState.INDEX_BLACK);
            var pieceTypeId = (int)(board[coord]) & ~MASK_COLOR;
            IEnumerable<ChessMove> moves;
            switch (pieceTypeId) {
                case ID_PAWN:
                    moves = GetPawnMoves();
                    break;
                case ID_KNIGHT:
                    moves = GetVacantOrCaptureMoves(possibleKnightPositions[coord]);
                    break;
                case ID_BISHOP:
                    moves = GetVacantOrCaptureMoves(possibleBishopPositions[coord]);
                    break;
                case ID_ROOK:
                    moves = GetVacantOrCaptureMoves(possibleRookPositions[coord]);
                    break;
                case ID_QUEEN:
                    moves = GetVacantOrCaptureMoves(possibleQueenPositions[coord]);
                    break;
                case ID_KING:
                    moves = GetKingMoves();
                    break;
                default:
                    throw new System.NotImplementedException($"Unknown value \"{pieceTypeId}\"!");
            }
            foreach (var move in moves) {
                if (!gs.DetermineIfMoveLeavesKingInCheck(move, piecePlayerIndex)) {
                    yield return move;
                }
            }

            IEnumerable<ChessMove> GetVacantOrCaptureMoves (PossiblePositions positions) {
                foreach (var reachableCoord in EnumerateVacantOrEnemyCoords(board, positions, pieceColorId)) {
                    if (!board[reachableCoord].IsKing()) {
                        yield return new ChessMove() {
                            srcCoord = coord,
                            dstCoord = reachableCoord
                        };
                    }
                }
            }

            IEnumerable<ChessMove> GetVacantMoves (PossiblePositions positions) {
                foreach (var reachableCoord in EnumerateVacantOrEnemyCoords(board, positions, pieceColorId)) {
                    if (board[reachableCoord] == ChessPiece.None) {
                        yield return new ChessMove() {
                            srcCoord = coord,
                            dstCoord = reachableCoord
                        };
                    }
                }
            }

            IEnumerable<ChessMove> GetCaptureMoves (PossiblePositions positions) {
                foreach (var reachableCoord in EnumerateVacantOrEnemyCoords(board, positions, pieceColorId)) {
                    if (board[reachableCoord] != ChessPiece.None && !board[reachableCoord].IsKing()) {
                        yield return new ChessMove() {
                            srcCoord = coord,
                            dstCoord = reachableCoord
                        };
                    }
                }
            }

            IEnumerable<ChessMove> GetPawnMoves () {
                PossiblePositions movePositions;
                PossiblePositions attackPositions;
                int prePromotionY;
                int enPassantStartY;
                IEnumerable<ChessPiece> promotionOptions;
                if (pieceColorId == ID_WHITE) {
                    movePositions = possibleWhitePawnMovePositions[coord];
                    attackPositions = possibleWhitePawnAttackPositions[coord];
                    prePromotionY = blackPawnHomeRowY;
                    enPassantStartY = whitePawnEnPassantStartY;
                    promotionOptions = WhitePawnPromotionOptions;
                } else {
                    movePositions = possibleBlackPawnMovePositions[coord];
                    attackPositions = possibleBlackPawnAttackPositions[coord];
                    prePromotionY = whitePawnHomeRowY;
                    enPassantStartY = blackPawnEnPassantStartY;
                    promotionOptions = BlackPawnPromotionOptions;
                }
                CoordToXY(coord, out _, out var pawnY);
                if (pawnY == prePromotionY) {
                    foreach (var move in GetVacantMoves(movePositions)) {
                        foreach (var promotion in promotionOptions) {
                            move.promoteTo = promotion;
                            yield return move;
                        }
                    }
                    foreach (var move in GetCaptureMoves(attackPositions)) {
                        foreach (var promotion in promotionOptions) {
                            move.promoteTo = promotion;
                            yield return move;
                        }
                    }
                } else if (pawnY == enPassantStartY) {
                    foreach (var move in GetVacantMoves(movePositions)) {
                        yield return move;
                    }
                    foreach (var move in GetCaptureMoves(attackPositions)) {
                        if (CheckForEnPassant(gs, move)) {
                            move.enPassant = true;
                        }
                        yield return move;
                    }
                } else {
                    foreach (var move in GetVacantMoves(movePositions)) {
                        yield return move;
                    }
                    foreach (var move in GetCaptureMoves(attackPositions)) {
                        yield return move;
                    }
                }
            }

            IEnumerable<ChessMove> GetKingMoves () {
                foreach (var move in GetVacantOrCaptureMoves(possibleKingPositions[coord])) {
                    yield return move;
                }
                if (!gs.playerStates[piecePlayerIndex].HasCastled && !gs.playerStates[piecePlayerIndex].IsInCheck) {
                    if (!gs.GetPositionHasBeenMoved(coord)) {
                        CoordToXY(coord, out var kingX, out var kingY);
                        var otherPlayerIndex = (piecePlayerIndex + 1) % 2;
                        var otherPlayerAttackMap = gs.PlayerStates[otherPlayerIndex].AttackMap;
                        var queensideCastleX = kingX - 2;
                        if (CanCastleWithRookAtXToX(0, queensideCastleX)) {
                            yield return new ChessMove() {
                                srcCoord = coord,
                                dstCoord = XYToCoord(queensideCastleX, kingY),
                                castle = true
                            };
                        }
                        var kingSideCastleX = kingX + 2;
                        if (CanCastleWithRookAtXToX(BOARD_SIZE - 1, kingSideCastleX)) {
                            yield return new ChessMove() {
                                srcCoord = coord,
                                dstCoord = XYToCoord(kingSideCastleX, kingY),
                                castle = true
                            };
                        }

                        bool CanCastleWithRookAtXToX (int rookX, int castleX) {
                            if (!gs.GetPositionHasBeenMoved(rookX, kingY)) {    // "hardcoding" the rook position and making the assumping that if he hasn't moved, he's still there...
                                var kingMoveStartX = (kingX < castleX ? kingX + 1 : kingX - 1);
                                var rookMoveStartX = (rookX < castleX ? rookX + 1 : rookX - 1);
                                var relaxedMinX = Math.Min(kingMoveStartX, rookMoveStartX);
                                var relaxedMaxX = Math.Max(kingMoveStartX, rookMoveStartX);
                                for (int x = relaxedMinX; x <= relaxedMaxX; x++) {
                                    var coord = XYToCoord(x, kingY);
                                    if (board[coord] != ChessPiece.None) {
                                        return false;
                                    }
                                }
                                var strictMinX = Math.Min(kingMoveStartX, castleX);
                                var strictMaxX = Math.Max(kingMoveStartX, castleX);
                                for (int x = strictMinX; x <= strictMaxX; x++) {
                                    var coord = XYToCoord(x, kingY);
                                    if (CheckCoordIsUnderAttack(otherPlayerAttackMap, coord)) {
                                        return false;
                                    }
                                }
                            }
                            return false;
                        }
                    }
                }
            }
        }

    }

}
