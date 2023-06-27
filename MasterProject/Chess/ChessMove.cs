using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterProject.Chess {
    
    public class ChessMove : IComparable<ChessMove> {

        public int sourceRow;
        public int sourceColumn;
        public int destinationRow;
        public int destinationColumn;
        public ChessPiece promoteTo;

        public string ToSortableString () {
            return $"{sourceRow}{sourceColumn}{destinationRow}{destinationColumn}{promoteTo.ToShortString()}";                
        }

        int IComparable<ChessMove>.CompareTo (ChessMove? other) {
            var a = this.ToSortableString();
            var b = other.ToSortableString();
            return string.CompareOrdinal(a, b);
        }

    }

}
