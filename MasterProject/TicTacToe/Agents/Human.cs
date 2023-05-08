using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterProject.TicTacToe.Agents {

    public class Human : TTTAgent {

        public override int GetMoveIndex (TTTGameState gs, IReadOnlyList<TTTMove> moves) {
            Console.WriteLine();
            Console.WriteLine($"You are: {TTTGameState.GetSymbolForPlayer(gs.CurrentPlayerIndex)}");
            Console.WriteLine();
            var symbolBoard = gs.GetPrintableBoardWithXsAndOs();
            var intBoard = gs.GetPrintableBoard((i) => moves.Any((move) => (move.fieldIndex == i)) ? (char)('0' + i) : ' ');
            Console.WriteLine(HorizontalConcat(symbolBoard, intBoard, "   |   "));
            while (true) {
                Console.Write("Please enter your desired move: ");
                if (int.TryParse(Console.ReadLine(), out var desiredField)) {
                    for (int i = 0; i < moves.Count; i++) {
                        if (moves[i].fieldIndex == desiredField) {
                            return i;
                        }
                    }
                    Console.WriteLine("That's not a valid move, try again!");
                } else {
                    Console.WriteLine("That's not an integer, try again!");
                }
            }
        }

        static string HorizontalConcat (string a, string b, string separator) {
            var aLines = a.Split(System.Environment.NewLine, System.StringSplitOptions.RemoveEmptyEntries);
            var bLines = b.Split(System.Environment.NewLine, System.StringSplitOptions.RemoveEmptyEntries);
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < aLines.Length; i++) {
                sb.AppendLine($"{aLines[i]}{separator}{bLines[i]}");
            }
            return sb.ToString();
        }
    }
}
