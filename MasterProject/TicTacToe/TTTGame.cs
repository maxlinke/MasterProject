using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterProject.TicTacToe {

    public class TTTGame : Game<TTTGame, TTTGameState, TTTMove, TTTAgent> {

        protected override int MinimumNumberOfAgentsRequired => TTTGameState.PLAYER_COUNT;

        protected override int MaximumNumberOfAgentsAllowed => TTTGameState.PLAYER_COUNT;

        protected override TTTGameState GetInitialGameState () {
            var output = new TTTGameState();
            output.Initialize();
            return output;
        }

        public static void RunHumanTwoPlayerGame (Game.ConsoleOutputs consoleOutputs = Game.ConsoleOutputs.Nothing) {
            PlayGameWithAgents(new Agents.Human(), new Agents.Human(), consoleOutputs);
        }

        public static void PlayAgainstBot (TTTAgent agent, bool agentGoesFirst, Game.ConsoleOutputs consoleOutputs = Game.ConsoleOutputs.Nothing) {
            PlayGameWithAgents(
                agentGoesFirst ? agent : new Agents.Human(),
                agentGoesFirst ? new Agents.Human() : agent,
                consoleOutputs
            );
        }

        private static void PlayGameWithAgents (TTTAgent agent1, TTTAgent agent2, Game.ConsoleOutputs consoleOutputs = Game.ConsoleOutputs.Nothing) {
            var game = new TTTGame();
            game.AllowedConsoleOutputs = consoleOutputs;
            game.SetAgents(new List<TTTAgent>(){
                agent1,
                agent2,
            });
            Console.WriteLine($"Agent 1 is {agent1.GetType()}, Agent 2 is {agent2.GetType()}");
            game.RunSynced();
            Console.WriteLine();
            Console.WriteLine("GAME OVER!");
            var gs = game.GetFinalGameState();
            if (gs.winnerIndex < 0) {
                Console.WriteLine("Resut: DRAW");
            } else {
                Console.WriteLine($"Result: {TTTGameState.GetSymbolForPlayer(gs.winnerIndex)} wins!");
            }
            Console.WriteLine($"Final gamestate: \n{gs.GetPrintableBoardWithXsAndOs()}");
        }

    }

}
