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

    // TODO
    public IEnumerable<IEnumerable<TObj>> GetPermutations<TObj> (int groupSize, params System.Type[] types) where TObj : new() {
        return null;
    }


    public static void Main (string[] args) {
        //MasterProject.TicTacToe.TTTGame.RunHumanTwoPlayerGame();
        //PlayAgainstBot();
        DoBotTournament(100);
    }

    static void PlayAgainstBot () {
        MasterProject.TicTacToe.TTTGame.PlayAgainstBot(
            new MasterProject.TicTacToe.Agents.MinMaxer(),
            new System.Random().NextDouble() < 0.5d,
            Game.ConsoleOutputs.Everything
        );
    }

    // TODO figure out how to do it all in parallel (and generalize)
    // TODO make a faster minmaxer
    // TODO make a minmaxer that uses a generic minmaxing thing
    static void DoBotTournament (int gameCountPerAgentConfig, int threadCount = 1) {
        threadCount = Math.Max(1, threadCount);
        var agentConfigs = new List<List<MasterProject.TicTacToe.TTTAgent>>() {
            new List<MasterProject.TicTacToe.TTTAgent>(){
                new MasterProject.TicTacToe.Agents.RandomAgent(),
                new MasterProject.TicTacToe.Agents.RandomAgent(),
            },
            new List<MasterProject.TicTacToe.TTTAgent>(){
                new MasterProject.TicTacToe.Agents.MinMaxer(),
                new MasterProject.TicTacToe.Agents.RandomAgent(),
            },
            new List<MasterProject.TicTacToe.TTTAgent>(){
                new MasterProject.TicTacToe.Agents.RandomAgent(),
                new MasterProject.TicTacToe.Agents.MinMaxer(),
            },
            //new List<MasterProject.TicTacToe.TTTAgent>(){
            //    new MasterProject.TicTacToe.Agents.RandomAgent(),
            //    new MasterProject.TicTacToe.Agents.RandomAgentWithLookAhead(),
            //},
            //new List<MasterProject.TicTacToe.TTTAgent>(){
            //    new MasterProject.TicTacToe.Agents.RandomAgentWithLookAhead(),
            //    new MasterProject.TicTacToe.Agents.RandomAgent(),
            //},
            //new List<MasterProject.TicTacToe.TTTAgent>(){
            //    new MasterProject.TicTacToe.Agents.RandomAgentWithLookAhead(),
            //    new MasterProject.TicTacToe.Agents.RandomAgentWithLookAhead(),
            //},
            //new List<MasterProject.TicTacToe.TTTAgent>(){
            //    new MasterProject.TicTacToe.Agents.RandomAgentWithLookAhead(),
            //    new MasterProject.TicTacToe.Agents.LineBuilder(),
            //},
            //new List<MasterProject.TicTacToe.TTTAgent>(){
            //    new MasterProject.TicTacToe.Agents.LineBuilder(),
            //    new MasterProject.TicTacToe.Agents.RandomAgentWithLookAhead(),
            //},
            //new List<MasterProject.TicTacToe.TTTAgent>(){
            //    new MasterProject.TicTacToe.Agents.LineBuilder(),
            //    new MasterProject.TicTacToe.Agents.LineBuilder(),
            //},
        };
        var sb = new System.Text.StringBuilder();
        for (int c = 0; c < agentConfigs.Count; c++) {
            var runId = $"Run {c + 1} of {agentConfigs.Count}";
            Console.WriteLine(runId);
            var agents = agentConfigs[c];
            var wins = new int[agents.Count];
            var draws = 0;
            //if (threadCount < 2) {
                for (int i = 0; i < gameCountPerAgentConfig; i++) {
                    var gameId = $"Game {i + 1} of {gameCountPerAgentConfig}";
                    Console.WriteLine($"{runId} {gameId}");
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
            //} else {
            //    var tasks = new Task[threadCount];
            //    for (int i = 0; i < tasks.Length; i++) {
            //        tasks[i] = 
            //    }
            //}
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

