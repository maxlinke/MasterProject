using static MasterProject.Chess.ChessPiece;

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

        private static bool CheckId (ChessPiece piece, int id) => (((int)piece) & id) == id;

        public static bool IsWhite (this ChessPiece piece) => CheckId(piece, ID_WHITE);
        public static bool IsBlack (this ChessPiece piece) => CheckId(piece, ID_BLACK);

        public static bool IsPawn (this ChessPiece piece) => CheckId(piece, ID_PAWN);
        public static bool IsKnight (this ChessPiece piece) => CheckId(piece, ID_KNIGHT);
        public static bool IsBishop (this ChessPiece piece) => CheckId(piece, ID_BISHOP);
        public static bool IsRook (this ChessPiece piece) => CheckId(piece, ID_ROOK);
        public static bool IsQueen (this ChessPiece piece) => CheckId(piece, ID_QUEEN);
        public static bool IsKing (this ChessPiece piece) => CheckId(piece, ID_KING);

        public static ChessPiece GetOppositeColor (this ChessPiece piece) {
            if (piece == None) {
                return None;
            }
            var pieceId = ((int)piece) & ~MASK_COLOR;
            return (ChessPiece)(piece.IsWhite() ? (ID_BLACK | pieceId) : (ID_WHITE | pieceId));
        }

        public static string ToShortString (this ChessPiece piece) => piece switch {
            None        => "-",
            WhitePawn   => "P",
            WhiteKnight => "N",
            WhiteBishop => "B",
            WhiteRook   => "R",
            WhiteQueen  => "Q",
            WhiteKing   => "K",
            BlackPawn   => "p",
            BlackKnight => "n",
            BlackBishop => "b",
            BlackRook   => "r",
            BlackQueen  => "q",
            BlackKing   => "k",
            _                      => "?",
        };

        public static ChessPiece FromShortString (string s) => s switch {
            "-" => None,
            "P" => WhitePawn,
            "N" => WhiteKnight,
            "B" => WhiteBishop,
            "R" => WhiteRook,
            "Q" => WhiteQueen,
            "K" => WhiteKing,
            "p" => BlackPawn,
            "n" => BlackKnight,
            "b" => BlackBishop,
            "r" => BlackRook,
            "q" => BlackQueen,
            "k" => BlackKing,
            _ => throw new System.ArgumentException($"Invalid argument \"{s}\"!"),
        };

        // the way this looks is vertically mirrored from white's perspective
        // because the coordinates are a1 = 0, b1 = 1, ... h8 = 63
        public static ChessPiece[] GetInitialBoard () {
            return new ChessPiece[]{
                WhiteRook, WhiteKnight, WhiteBishop, WhiteQueen, WhiteKing, WhiteBishop, WhiteKnight, WhiteRook,
                WhitePawn, WhitePawn,   WhitePawn,   WhitePawn,  WhitePawn, WhitePawn,   WhitePawn,   WhitePawn,
                None,      None,        None,        None,       None,      None,        None,        None,
                None,      None,        None,        None,       None,      None,        None,        None,
                None,      None,        None,        None,       None,      None,        None,        None,
                None,      None,        None,        None,       None,      None,        None,        None,
                BlackPawn, BlackPawn,   BlackPawn,   BlackPawn,  BlackPawn, BlackPawn,   BlackPawn,   BlackPawn,
                BlackRook, BlackKnight, BlackBishop, BlackQueen, BlackKing, BlackBishop, BlackKnight, BlackRook,
            };
        }

    }

}
