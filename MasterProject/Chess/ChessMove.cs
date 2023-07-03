namespace MasterProject.Chess {
    
    public class ChessMove : IEquatable<ChessMove?> {

        public int srcCoord;
        public int dstCoord;

        public ChessPiece promoteTo;
        public bool enPassantCapture;
        public bool castle;

        public override bool Equals (object? obj) {
            return this.Equals(obj as ChessMove);
        }

        public bool Equals (ChessMove? other) {
            return other is not null
                && srcCoord == other.srcCoord
                && dstCoord == other.dstCoord
                && promoteTo == other.promoteTo
                && enPassantCapture == other.enPassantCapture
                && castle == other.castle;
        }

        public override int GetHashCode () {
            return (srcCoord << 0)                      // takes 6 bits
                 | (dstCoord << 6)                      // takes 6 bits
                 | ((int)promoteTo << 12)               // takes 8 bits
                 | (enPassantCapture ? 1 << 30 : 0)     // just putting it at the very end
                 | (castle ? 1 << 31 : 0)               // just putting it at the very end
            ;
        }

        public static bool operator == (ChessMove? left, ChessMove? right) {
            return left.Equals(right);
        }

        public static bool operator != (ChessMove? left, ChessMove? right) {
            return !(left == right);
        }
    }

}
