namespace MasterProject.Chess.Tests {

    public static class ChessTestUtils {

        public static string CleanUpBoardString (string s) {
            var sb = new System.Text.StringBuilder();
            var lines = s.Split(System.Environment.NewLine).Select(l => l.Trim()).Where(l => l.Length > 0).ToArray();
            foreach (var line in lines) {
                var lineParts = line.Split(null).Select(p => p.Trim()).Where(p => p.Length > 0).ToArray();
                foreach (var part in lineParts) {
                    sb.Append($"{part} ");
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }

        public static ChessPiece[] GetBoardFromString (string s) {
            var lines = CleanUpBoardString(s).Split(System.Environment.NewLine);
            var board = new ChessPiece[ChessGameState.BOARD_SIZE * ChessGameState.BOARD_SIZE];
            for (int i = 0; i < lines.Length; i++) {
                for (int j = 0; j < lines[i].Length; j+=2) {
                    var coord = ChessGameState.XYToCoord(j/2, ChessGameState.BOARD_SIZE - 1 - i);
                    board[coord] = ChessPieceUtils.FromShortString(lines[i][j].ToString());
                }
            }
            return board;
        }

        public static ChessGameState SetupGameState (string s, int playerIndex) {
            var gs = new ChessGameState();
            gs.Initialize();
            gs.board = GetBoardFromString(s);
            gs.currentPlayerIndex = playerIndex;
            return gs;
        }

    }

}
