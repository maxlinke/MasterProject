using static MasterProject.Chess.ChessGameState;

namespace MasterProject.Chess {

    public static class ChessGameStateUtils {

        public static int XYToCoord (int x, int y) {
            return (y * BOARD_SIZE) + x;
        }

        public static void CoordToXY (int coord, out int x, out int y) {
            y = coord / BOARD_SIZE;
            x = coord % BOARD_SIZE;
        }

        public static string CoordToString (int coord) {
            CoordToXY(coord, out var x, out var y);
            return $"{(char)('a' + x)}{y + 1}";
        }

        public static int CoordFromString (string value) {
            if (string.IsNullOrWhiteSpace(value)) {
                return -1;
            }
            if (value.Length != 2) {
                return -1;
            }

            var x = (char.ToLower(value[0]) - (int)('a'));
            var y = (value[1] - (int)('1'));
            if (x < 0 || x >= BOARD_SIZE || y < 0 || y >= BOARD_SIZE) {
                return -1;
            }
            return XYToCoord(x, y);
        }

    }

}
