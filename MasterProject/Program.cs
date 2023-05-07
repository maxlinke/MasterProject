// See https://aka.ms/new-console-template for more information

using MasterProject;
using System.Text.Json;
using System.Text.Json.Serialization;

public class Program {

    public static string GetProjectPath () {
        var binPath = Environment.CurrentDirectory;
        if (binPath.Contains("\\bin\\Debug\\")) {
            return binPath.Substring(0, binPath.LastIndexOf("\\bin\\Debug\\") + 1);
        }
        return binPath;
    }

    // TODO try running each config asynchronously and compare that to the runtime of the serial synced version

    // TODO
    public IEnumerable<IEnumerable<TObj>> GetPermutations<TObj> (int groupSize, params System.Type[] types) where TObj : new() {
        return null;
    }

    const int gameCountPerAgentConfig = 100000;

    public static void Main (string[] args) {
        //MasterProject.TicTacToe.TTTGame.RunHumanTwoPlayerGame();
        //MasterProject.TicTacToe.TTTGame.PlayAgainstBot(
        //    new MasterProject.TicTacToe.Agents.RandomAgent(), 
        //    new System.Random().NextDouble() < 0.5d, 
        //    Game.ConsoleOutputs.Everything    
        //);

        var agentConfigs = new List<List<MasterProject.TicTacToe.TTTAgent>>() {
            new List<MasterProject.TicTacToe.TTTAgent>(){
                new MasterProject.TicTacToe.Agents.RandomAgent(),
                new MasterProject.TicTacToe.Agents.RandomAgent(),
            },
            new List<MasterProject.TicTacToe.TTTAgent>(){
                new MasterProject.TicTacToe.Agents.RandomAgent(),
                new MasterProject.TicTacToe.Agents.RandomAgentWithLookAhead(),
            },
            new List<MasterProject.TicTacToe.TTTAgent>(){
                new MasterProject.TicTacToe.Agents.RandomAgentWithLookAhead(),
                new MasterProject.TicTacToe.Agents.RandomAgent(),
            },
            new List<MasterProject.TicTacToe.TTTAgent>(){
                new MasterProject.TicTacToe.Agents.RandomAgentWithLookAhead(),
                new MasterProject.TicTacToe.Agents.RandomAgentWithLookAhead(),
            },
            new List<MasterProject.TicTacToe.TTTAgent>(){
                new MasterProject.TicTacToe.Agents.RandomAgentWithLookAhead(),
                new MasterProject.TicTacToe.Agents.LineBuilder(),
            },
            new List<MasterProject.TicTacToe.TTTAgent>(){
                new MasterProject.TicTacToe.Agents.LineBuilder(),
                new MasterProject.TicTacToe.Agents.RandomAgentWithLookAhead(),
            },
            new List<MasterProject.TicTacToe.TTTAgent>(){
                new MasterProject.TicTacToe.Agents.LineBuilder(),
                new MasterProject.TicTacToe.Agents.LineBuilder(),
            },
        };
        var sb = new System.Text.StringBuilder();
        for(int c=0; c<agentConfigs.Count; c++){
            Console.WriteLine($"Run {c + 1} of {agentConfigs.Count}");
            var agents = agentConfigs[c];
            var wins = new int[agents.Count];
            var draws = 0;
            for (int i = 0; i < gameCountPerAgentConfig; i++) {
                var game = new MasterProject.TicTacToe.TTTGame();
                game.SetAgents(agents);
                game.AllowedConsoleOutputs = Game.ConsoleOutputs.Nothing;
                game.RunSynced();
                var finalState = game.GetFinalGameState();
                if (finalState.winnerIndex < 0) {
                    draws++;
                } else {
                    wins[finalState.winnerIndex]++;
                }
            }
            sb.AppendLine($"Result after {gameCountPerAgentConfig} games:");
            for (int i = 0; i < agents.Count; i++) {
                sb.AppendLine($" - Agent {i} ({agents[i].Id}) won {wins[i]} times");
            }
            sb.AppendLine($" - There were {draws} draws");
            sb.AppendLine();
            var firstAgent = agents[0];
            agents.RemoveAt(0);
            agents.Add(firstAgent);
        }
        Console.WriteLine();
        Console.WriteLine(sb.ToString());

    }

}

