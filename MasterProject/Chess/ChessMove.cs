namespace MasterProject.Chess {
    
    public class ChessMove : IComparable<ChessMove> {

        public int sourceX;
        public int sourceY;
        public int destinationX;
        public int destinationY;
        public ChessPiece promoteTo;

        public bool checksKing;

        public string ToSortableString () {
            return $"{(char)('a' + sourceX)}{sourceY+1}{(char)('a' + destinationX)}{destinationY+1}{promoteTo.ToShortString()}";                
        }

        int IComparable<ChessMove>.CompareTo (ChessMove? other) {
            var a = this.ToSortableString();
            var b = other.ToSortableString();
            return string.CompareOrdinal(a, b);
        }

    }

}
