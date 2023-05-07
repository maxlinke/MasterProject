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

    const int gameCountPerAgentConfig = 100000;

    public static void Main (string[] args) {
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
            }
        };
        var sb = new System.Text.StringBuilder();
        foreach(var agents in agentConfigs){
            var wins = new int[agents.Count];
            var draws = 0;
            for (int i = 0; i < gameCountPerAgentConfig; i++) {
                var game = new MasterProject.TicTacToe.TTTGame();
                game.SetAgents(agents);
                game.AllowedConsoleOutputs = Game.ConsoleOutputs.Nothing;
                game.RunSynced();
                var record = game.GetRecord();
                var finalState = record.GameStates[record.GameStates.Length - 1] as MasterProject.TicTacToe.TTTGameState;
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

