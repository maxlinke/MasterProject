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
                for (int j = 0; j < lines[i].Length; j += 2) {
                    var coord = ChessGameState.XYToCoord(j / 2, ChessGameState.BOARD_SIZE - 1 - i);
                    board[coord] = ChessPieceUtils.FromShortString(lines[i][j].ToString());
                }
            }
            return board;
        }

        public static ChessGameState SetupGameState (string s, int playerIndex) {
            var gs = new ChessGameState();
            gs.Initialize();
            gs.board = GetBoardFromString(s);
            for (int i = 0; i < ChessGameState.PLAYER_COUNT; i++) {
                gs.playerStates[i].KingCoord = Array.IndexOf(gs.board, (i == ChessGameState.INDEX_WHITE ? ChessPiece.WhiteKing : ChessPiece.BlackKing));
            }
            var initBoard = ChessPieceUtils.GetInitialBoard();
            for (int i = 0; i < gs.board.Length; i++) {
                gs.SetPositionHasBeenMoved(i, gs.board[i] == ChessPiece.None || gs.board[i] != initBoard[i]);
            }
            gs.currentPlayerIndex = playerIndex;
            gs.UpdatePlayerAttackMapsAndCheckStates();
            gs.UpdateGameIsOver();
            return gs;
        }

        public static ChessMove GetMoveFromString (IEnumerable<ChessMove> moves, string s) {
            var moveSplit = s.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var srcCoord = ChessGameState.CoordFromString(moveSplit[0]);
            var dstCoord = ChessGameState.CoordFromString(moveSplit[1]);
            foreach (var move in moves) {
                if (move.srcCoord == srcCoord && move.dstCoord == dstCoord) {
                    return move;
                }
            }
            throw new ArgumentException($"Couldn't find a move that matches \"{s}\"!");
        }

        public static void PrintBoardAndKingMoves (ChessGameState gs, out int kingMoveCount) {
            kingMoveCount = 0;
            var kingMap = 0L;
            var kingCoord = gs.PlayerStates[gs.currentPlayerIndex].KingCoord;
            var kingPiece = ((gs.currentPlayerIndex == ChessGameState.INDEX_WHITE) ? ChessPiece.WhiteKing : ChessPiece.BlackKing);
            var sb = new System.Text.StringBuilder();
            foreach (var kingMove in gs.GetPossibleMovesForCurrentPlayer().Where(move => gs.board[move.srcCoord] == kingPiece)) {
                kingMoveCount++;
                if (kingMove.srcCoord != kingCoord) {
                    Console.WriteLine($"Got different king coords! \"{ChessGameState.CoordToString(kingCoord)}\" and \"{ChessGameState.CoordToString(kingMove.srcCoord)}\"!");
                }
                kingMap = (kingMap | (1L << kingMove.dstCoord));
                sb.AppendLine($"{kingPiece} can move {kingMove.CoordinatesToString()}");
            }
            var plainMap = gs.ToPrintableString();
            var otherAttacks = ChessGameState.MakePrintableAttackMap(-1, gs.PlayerStates[(gs.currentPlayerIndex + 1) % 2].AttackMap);
            var ownMovement = ChessGameState.MakePrintableAttackMap(kingCoord, kingMap);
            Console.WriteLine(plainMap.HorizontalConcat(otherAttacks, "  |  ").HorizontalConcat(ownMovement, "  |  "));
            //Console.WriteLine(sb.ToString());
        }

    }

}
