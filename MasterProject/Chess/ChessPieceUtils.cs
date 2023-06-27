namespace MasterProject.Chess {

    public static class ChessPieceUtils {

        // pieces
        public const int ID_PAWN = 1 << 0;
        public const int ID_KNIGHT = 1 << 1;
        public const int ID_BISHOP = 1 << 2;
        public const int ID_ROOK = 1 << 3;
        public const int ID_QUEEN = 1 << 4;
        public const int ID_KING = 1 << 5;

        // colors
        public const int MASK_COLOR = ID_WHITE | ID_BLACK;
        public const int ID_WHITE = 1 << 6;
        public const int ID_BLACK = 1 << 7;

        public static string ToShortString (this ChessPiece piece) => piece switch {
            ChessPiece.None        => "-",
            ChessPiece.WhitePawn   => "P",
            ChessPiece.WhiteKnight => "N",
            ChessPiece.WhiteBishop => "B",
            ChessPiece.WhiteRook   => "R",
            ChessPiece.WhiteQueen  => "Q",
            ChessPiece.WhiteKing   => "K",
            ChessPiece.BlackPawn   => "p",
            ChessPiece.BlackKnight => "n",
            ChessPiece.BlackBishop => "b",
            ChessPiece.BlackRook   => "r",
            ChessPiece.BlackQueen  => "q",
            ChessPiece.BlackKing   => "k",
            _                      => "?",
        };

    }

}
