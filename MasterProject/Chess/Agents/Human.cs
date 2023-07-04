using System.Collections.Generic;

namespace MasterProject.Chess.Agents {

    public class Human : ChessAgent {

        public override bool IsStateless => true;

        public override bool IsTournamentEligible => false;

        public override Agent Clone () {
            return new Human();
        }

        public override int GetMoveIndex (ChessGameState gameState, IReadOnlyList<ChessMove> moves) {
            Console.WriteLine();
            Console.WriteLine($"You are {(gameState.currentPlayerIndex == ChessGameState.INDEX_WHITE ? "White (bottom)" : "Black (top)")}");
            Console.WriteLine(gameState.ToPrintableString());
            while (true) {
                Console.WriteLine($"Please enter your desired move [start coord] [end coord]: ");
                var text = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(text)) {
                    continue;
                }
                var split = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (split.Length != 2) {
                    continue;
                }
                var srcCoord = ChessGameStateUtils.CoordFromString(split[0]);
                var dstCoord = ChessGameStateUtils.CoordFromString(split[1]);
                if (srcCoord < 0 || dstCoord < 0) {
                    continue;
                }
                var matchingIndices = new List<int>();
                for (int i = 0; i < moves.Count; i++) {
                    if (moves[i].srcCoord == srcCoord && moves[i].dstCoord == dstCoord) {
                        matchingIndices.Add(i);
                    }
                }
                if (matchingIndices.Count == 1) {
                    return matchingIndices[0];
                }
                if (matchingIndices.Count > 1) {
                    Console.WriteLine("The selected move has multiple options:");
                    for (int i = 0; i < matchingIndices.Count; i++) {
                        Console.WriteLine($" {i} : Promote to {moves[matchingIndices[i]].promoteTo}");
                    }
                    if (int.TryParse(Console.ReadLine(), out var parsedNumber)) {
                        if (parsedNumber >= 0 && parsedNumber < matchingIndices.Count) {
                            return matchingIndices[parsedNumber];
                        }
                    }
                }
            }
        }

    }

}
