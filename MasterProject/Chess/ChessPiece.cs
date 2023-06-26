using static MasterProject.Chess.ChessPieceUtils;

namespace MasterProject.Chess {

    public enum ChessPiece {
        None        = 0,
        // white
        WhitePawn   = ID_WHITE | ID_PAWN,
        WhiteKnight = ID_WHITE | ID_KNIGHT,
        WhiteBishop = ID_WHITE | ID_BISHOP,
        WhiteRook   = ID_WHITE | ID_ROOK,
        WhiteQueen  = ID_WHITE | ID_QUEEN,
        WhiteKing   = ID_WHITE | ID_KING,
        // black
        BlackPawn   = ID_BLACK | ID_PAWN,
        BlackKnight = ID_BLACK | ID_KNIGHT,
        BlackBishop = ID_BLACK | ID_BISHOP,
        BlackRook   = ID_BLACK | ID_ROOK,
        BlackQueen  = ID_BLACK | ID_QUEEN,
        BlackKing   = ID_BLACK | ID_KING,
    }

}
