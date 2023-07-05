using static MasterProject.Chess.ChessGameState;

namespace MasterProject.Chess {

    public static class ChessGameStateUtils {

        static ChessGameStateUtils () {
            var wfc = new List<int>();
            var bfc = new List<int>();
            var totalFieldCount = ChessGameState.BOARD_SIZE * ChessGameState.BOARD_SIZE;
            for (int i = 0; i < totalFieldCount; i++) {
                if (BoardIsWhiteAtCoordinate(i)) {
                    wfc.Add(i);
                } else {
                    bfc.Add(i);
                }
            }
            whiteFieldCoords = wfc.ToArray();
            blackFieldCoords = bfc.ToArray();
        }

        public static readonly IReadOnlyList<int> whiteFieldCoords;
        public static readonly IReadOnlyList<int> blackFieldCoords;

        public static bool BoardIsWhiteAtCoordinate (int coord) {
            CoordToXY(coord, out var x, out var y);
            return BoardIsWhiteAtXY(x, y);
        }

        public static bool BoardIsWhiteAtXY (int x, int y) {
            return ((x + y) % 2) != 0;
        }

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

        // https://en.wikipedia.org/wiki/Chebyshev_distance
        public static int ChebyshevDistanceBetweenCoords (int coordA, int coordB) {
            CoordToXY(coordA, out var xA, out var yA);
            CoordToXY(coordB, out var xB, out var yB);
            return Math.Max(Math.Abs(xA - xB), Math.Abs(yA - yB));
        }

    }

}
