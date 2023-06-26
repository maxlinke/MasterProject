using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace MasterProject.Chess {
    
    public class ChessGameState {

        public const int BOARD_SIZE = 8;

        // https://en.wikipedia.org/wiki/Rules_of_chess

        // 7 - - - - - - - -
        // 6 - - - - - - - -
        // 5 - - - - - - - -
        // 4 - - - - - - - -
        // 3 - - - - - - - -
        // 2 - - - - - - - -
        // 1 - - - - - - - -
        // 0 - - - - - - - -
        //   0 1 2 3 4 5 6 7

        public ChessPiece[] board { get; set; } // TODO set this up in the way one would look at it (left to right, bottom to top)
        public long pieceHasMoved { get; set; } // bit mask

        [JsonIgnore]
        public ChessGameState previousState { get; set; }
        // yes, game has its own internal list of previous gamestates
        // and it would be nice to have access to that
        // however putting the gamestate history in the "get possible moves" thing would mean that alphabeta would also have to track this
        // and that would probably produce a LOT more garbage than creating a lot of gamestates already does
        // besides, the other games don't care about history
        // so it's fine to implement that here

        // TODO player states
        // -> checked
        // is there a rule in chess about repeating the same (series) of moves x times in a row?
        // if so, i need to make it so this can get that information
        // also i probably need that just to prevent these games going on forever
        // yes, there are rules for both of these things.

        public void MovePiece (int startCoord, int targetCoord) {
            // TODO
            // a) throw an exception of there is nothing
            // b) do the whole "piecehasmoved" thing
        }

        public static int XYToCoord (int x, int y) {
            return (y * BOARD_SIZE) + x;
        }

        public static void CoordToXY (int coord, out int x, out int y) {
            y = coord / BOARD_SIZE;
            x = coord % BOARD_SIZE;
        }

        // TODO test this
        public int CountTotalPiecesOfColor (int colorId) {
            var output = 0;
            for (int i = 0; i < board.Length; i++) {
                if ((((int)(board[i])) & colorId) == colorId) {
                    output++;
                }
            }
            return output;
        }

        // TODO test this
        public int CountNumberOfPieces (ChessPiece piece) {
            var output = 0;
            for (int i = 0; i < board.Length; i++) {
                if (board[i] == piece) {
                    output++;
                }
            }
            return output;
        }

        // TODO test this with the printable string thing
        public void Initialize () {
            this.board = new ChessPiece[BOARD_SIZE * BOARD_SIZE];
            this.pieceHasMoved = 0;
            for (int i = 0; i < BOARD_SIZE; i++) {
                SetPiecesVerticallySymmetric(i, 1, ChessPieceUtils.ID_PAWN);
            }
            for (int i = 0; i < 3; i++) {
                var pieceId = i switch {
                    0 => ChessPieceUtils.ID_ROOK,
                    1 => ChessPieceUtils.ID_KNIGHT,
                    2 => ChessPieceUtils.ID_BISHOP
                };
                SetPiecesVerticallySymmetric(i, 0, pieceId);
                SetPiecesVerticallySymmetric(BOARD_SIZE - 1 - i, 0, pieceId);
            }
            SetPiecesVerticallySymmetric(3, 0, ChessPieceUtils.ID_QUEEN);
            SetPiecesVerticallySymmetric(4, 0, ChessPieceUtils.ID_KING);

            void SetPiecesVerticallySymmetric (int x, int localY, int rawPieceId) {
                var wCoord = XYToCoord(x, localY);
                board[wCoord] = (ChessPiece)(ChessPieceUtils.ID_WHITE | rawPieceId);
                var bCoord = XYToCoord(x, BOARD_SIZE - 1 - localY);
                board[bCoord] = (ChessPiece)(ChessPieceUtils.ID_BLACK | rawPieceId);
            }
        }

        public string ToPrintableString () {
            // 7   r n b q k b n r
            // 6   p p p p p p p p
            // 5   . . . . . . . .
            // 4   . . . . . . . .
            // 3   . . . . . . . .
            // 2   . . . . . . . . 
            // 1   P P P P P P P P
            // 0   R N B Q K B N R
            //
            //     0 1 2 3 4 5 6 7
            var sb = new System.Text.StringBuilder();
            for (int y = BOARD_SIZE - 1; y >= 0; y--) {
                sb.Append($"{y}   ");
                for (int x = 0; x < BOARD_SIZE; x++) {
                    sb.Append($"{board[XYToCoord(x, y)].ToShortString()} ");
                }
                sb.Append("\n");
            }
            sb.Append("\n");
            sb.Append($"    ");
            for (int x = 0; x < BOARD_SIZE; x++) {
                sb.Append($"{x} ");
            }
            return sb.ToString();
        }

    }

}
