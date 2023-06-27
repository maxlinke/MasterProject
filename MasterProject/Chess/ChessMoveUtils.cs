using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MasterProject.Chess.ChessGameState;
using static MasterProject.Chess.ChessPieceUtils;

namespace MasterProject.Chess {

    public static class ChessMoveUtils {

        class MoveOffset {
            public readonly int x;
            public readonly int y;
            public MoveOffset (int x, int y) {
                this.x = x;
                this.y = y;
            }
        }

        static ChessMoveUtils () {
            var squareOffsets = new MoveOffset[8];
            var knightOffsets = new MoveOffset[8];
            for (int i = 0; i < 8; i++) {
                var k = i + ((i > 3) ? 1 : 0);  // k goes from 0 to 8 without ever going to 4
                squareOffsets[i] = new MoveOffset((k / 3) - 1, (k % 3) - 1);
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
            squareMoveOffsets = squareOffsets;
            knightMoveOffsets = knightOffsets;
            diagonalAxisMoveOffsets = diagonalAxes;
            alignedAxisMoveOffsets = alignedAxes;
        }

        static readonly IReadOnlyList<IReadOnlyList<MoveOffset>> diagonalAxisMoveOffsets;
        static readonly IReadOnlyList<IReadOnlyList<MoveOffset>> alignedAxisMoveOffsets;
        static readonly IReadOnlyList<MoveOffset> squareMoveOffsets;
        static readonly IReadOnlyList<MoveOffset> knightMoveOffsets;

        public static IEnumerable<ChessMove> GetRegularMovesForPiece (ChessGameState gs, int coord) {
            CoordToXY(coord, out var x, out var y);
            var board = gs.board;
            var pieceColorId = (int)(board[coord]) & MASK_COLOR;
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
                foreach (var offset in squareMoveOffsets) {
                    var newX = x + offset.x;
                    var newY = y + offset.y;
                    if (CheckIsInbounds(newX, newY)) {
                        var newCoord = XYToCoord(newX, newY);
                        var isValidCoord = false;
                        var coordIsOtherKing = false;
                        if (board[newCoord] == ChessPiece.None) {
                            isValidCoord = true;
                        } else {
                            var colorAtCoord = (int)(board[newCoord]) & MASK_COLOR;
                            var pieceAtCoord = (int)(board[newCoord]) & ~MASK_COLOR;
                            if (colorAtCoord != pieceColorId) {
                                isValidCoord = true;
                                coordIsOtherKing = (pieceAtCoord == ID_KING);
                            }
                        }
                        if (isValidCoord) {
                            yield return new ChessMove() {
                                sourceX = x,
                                sourceY = y,
                                destinationX = newX,
                                destinationY = newY,
                                checksKing = coordIsOtherKing
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
                // remember the conversions?
                // 
                throw new NotImplementedException();
            }

            IEnumerable<ChessMove> GetKnightMoves (int x, int y) {
                foreach (var move in GetVacantOrCaptureMoves(knightMoveOffsets, x, y, false)) {
                    yield return move;
                }
            }

            IEnumerable<ChessMove> GetBishopMoves (int x, int y) {
                foreach (var moves in diagonalAxisMoveOffsets) {
                    foreach (var move in GetVacantOrCaptureMoves(moves, x, y, true)) {
                        yield return move;
                    }
                }
            }

            IEnumerable<ChessMove> GetRookMoves (int x, int y) {
                foreach (var moves in alignedAxisMoveOffsets) {
                    foreach (var move in GetVacantOrCaptureMoves(moves, x, y, true)) {
                        yield return move;
                    }
                }
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
                foreach (var move in GetVacantOrCaptureMoves(squareMoveOffsets, x, y, false)) {
                    yield return move;
                }
                // TODO castle thing when that gets added back in
            }
        }

    }

}
