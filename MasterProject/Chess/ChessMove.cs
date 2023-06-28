namespace MasterProject.Chess {
    
    public class ChessMove : IEquatable<ChessMove?> {

        public int srcCoord;
        public int dstCoord;

        public ChessPiece promoteTo;
        public bool enPassant;
        public bool castle;

        public override bool Equals (object? obj) {
            return this.Equals(obj as ChessMove);
        }

        public bool Equals (ChessMove? other) {
            return other is not null
                && srcCoord == other.srcCoord
                && dstCoord == other.dstCoord
                && promoteTo == other.promoteTo
                && enPassant == other.enPassant
                && castle == other.castle;
        }

        public static bool operator == (ChessMove? left, ChessMove? right) {
            return left.Equals(right);
        }

        public static bool operator != (ChessMove? left, ChessMove? right) {
            return !(left == right);
        }
    }

}
