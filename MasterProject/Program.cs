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
    public static IEnumerable<IEnumerable<TObj>> GetPermutations<TObj> (int groupSize, params TObj[] objs) {
        // outputcount = pow(objs.Length, groupSize)
        // basically just nested for loops
        // easy with recursion
        // is there a way without recursion?
        // there always is...

        yield return null;
    }

    void TestMethod () {
        var perms1 = GetPermutations<TestObj>(3, new Foo(), new Bar());
        var perms2 = GetPermutations<TestObj>(3, new TestObj[] { new Foo(), new Bar() });
        var list = new List<TestObj>();
        list.Add(new Foo());
        list.Add(new Bar());
        var perms3 = GetPermutations(3, list);
    }

    abstract class TestObj { }

    class Foo : TestObj { }

    class Bar : TestObj { }

    // TODO make up some winlossdrawmatrices, try the merge function, try exporting to json and datasavering it
    // then make a quick html/js thing to take care of the rest
    // elo calculation can be done there
    // and also image generation and stuff
    // TODO generate an image and see what happens when i right-click it (can i save it?)

    public static void Main (string[] args) {
        //MasterProject.TicTacToe.TTTGame.RunHumanTwoPlayerGame();
        //PlayAgainstBot(new MasterProject.TicTacToe.Agents.ABLoseFast());
        //DoSyncAsyncTest(20);
        //DoBotTournament(100, 100);
        //DoTheThing(1000, true).GetAwaiter().GetResult();
        //DoTheThing(1000, false).GetAwaiter().GetResult();
        //DoTheThing(1000, true).GetAwaiter().GetResult();
        //DoTheThing(1000, false).GetAwaiter().GetResult();
        DoWinLossDrawTest();
        //DataSaver.SaveInProject("DATASAVERTEST/hello.txt", System.Text.Encoding.UTF8.GetBytes("hello, world"));
        DataSaver.Flush();
    }

    static void DoWinLossDrawTest () {
        //var a = new WinLossDrawRecord();
        //a.wins["Peter"]["Bob"] = 3;
        //a.losses["Bob"]["Peter"] = 3;
        //a.draws["Peter"]["Bob"] = 2;
        //a.draws["Bob"]["Peter"] = 1;
        //a.wins["Bob"]["Peter"] = 1;
        //a.losses["Bob"]["Peter"] = 1;
        //var b = new WinLossDrawRecord();
        //b.wins["Peter"]["Bob"] = 6;
        //b.losses["Bob"]["Peter"] = 6;
        //b.wins["Peter"]["Jim"] = 2;
        //b.losses["Jim"]["Peter"] = 2;
        //b.draws["Jim"]["Mary"] = 1;
        //b.draws["Mary"]["Jim"] = 1;
        //var c = WinLossDrawRecord.Merge(a, b);
        //DataSaver.SaveInProject($"WinLossTest/a.json", JsonSerializer.SerializeToUtf8Bytes(a));
        //DataSaver.SaveInProject($"WinLossTest/b.json", JsonSerializer.SerializeToUtf8Bytes(b));
        //DataSaver.SaveInProject($"WinLossTest/c.json", JsonSerializer.SerializeToUtf8Bytes(c));
    }

    static void DoSyncAsyncTest (int gameCount, int timeoutMillis = Game.NO_TIMEOUT) {
        var asyncMillis = DoBotTournament(gameCount, gameCount, timeoutMillis);
        var syncMillis = DoBotTournament(gameCount, 0, timeoutMillis);
        Console.WriteLine($"Parallel took {asyncMillis}ms");
        Console.WriteLine($"Synced took {syncMillis}ms");
    }

    static void PlayAgainstBot (MasterProject.TicTacToe.TTTAgent bot, int timeoutMillis = Game.NO_TIMEOUT) {
        MasterProject.TicTacToe.TTTGame.PlayAgainstBot(
            bot,
            new System.Random().NextDouble() < 0.5d,
            timeoutMillis,
            Game.ConsoleOutputs.Everything
        );
    }

    static long DoBotTournament (int gameCountPerAgentConfig, int threadCount = 1, int timeoutMillis = Game.NO_TIMEOUT) {
        var sw = new System.Diagnostics.Stopwatch();
        sw.Start();
        threadCount = Math.Max(1, threadCount);
        var agentConfigs = new List<List<MasterProject.TicTacToe.TTTAgent>>() {
            //new List<MasterProject.TicTacToe.TTTAgent>(){
            //    new MasterProject.TicTacToe.Agents.RandomAgent(),
            //    new MasterProject.TicTacToe.Agents.ABLose(),
            //},
            new List<MasterProject.TicTacToe.TTTAgent>(){
                new MasterProject.TicTacToe.Agents.ABWin(),
                new MasterProject.TicTacToe.Agents.RandomAgent(),
            },
            new List<MasterProject.TicTacToe.TTTAgent>(){
                new MasterProject.TicTacToe.Agents.ABWinFast(),
                new MasterProject.TicTacToe.Agents.RandomAgent(), 
            }
        };
        var sb = new System.Text.StringBuilder();
        for (int c = 0; c < agentConfigs.Count; c++) {
            var runId = $"Run {c + 1} of {agentConfigs.Count}";
            Console.WriteLine(runId);
            var agents = agentConfigs[c];
            var wins = new int[agents.Count];
            var draws = 0;
            var totalMovesUntilWin = new int[agents.Count];
            if (threadCount < 2) {
                for (int i = 0; i < gameCountPerAgentConfig; i++) {
                    var gameId = $"Game {i + 1} of {gameCountPerAgentConfig}";
                    Console.WriteLine($"{runId} {gameId}");
                    var game = new MasterProject.TicTacToe.TTTGame();
                    game.SetAgents(agents);
                    game.AgentMoveTimeoutMilliseconds = timeoutMillis;
                    game.RunSynced();
                    var finalState = game.GetFinalGameState();
                    if (finalState.winnerIndex < 0) {
                        draws++;
                    } else {
                        wins[finalState.winnerIndex]++;
                        totalMovesUntilWin[finalState.winnerIndex] += game.GetRecord().GameStates.Length - 1;
                    }
                }
            } else {
                for (int i = 0; i < gameCountPerAgentConfig; i += threadCount) {
                    var games = new MasterProject.TicTacToe.TTTGame[Math.Min(threadCount, (gameCountPerAgentConfig - i))];
                    var gameId = $"Games {i + 1} to {i + games.Length} of {gameCountPerAgentConfig}";
                    Console.WriteLine($"{runId} {gameId}");
                    Console.WriteLine($"{System.DateTime.Now.ToLongTimeString()} init");
                    for (int j = 0; j < games.Length; j++) {
                        games[j] = new MasterProject.TicTacToe.TTTGame();
                        games[j].SetAgents(agents);
                        games[j].AgentMoveTimeoutMilliseconds = timeoutMillis;
                    }
                    Console.WriteLine($"{System.DateTime.Now.ToLongTimeString()} start wait");
                    RunGamesInParallel(games).GetAwaiter().GetResult();
                    Console.WriteLine($"{System.DateTime.Now.ToLongTimeString()} finish wait");
                    foreach (var game in games) {
                        var finalState = game.GetFinalGameState();
                        if (finalState.winnerIndex < 0) {
                            draws++;
                        } else {
                            wins[finalState.winnerIndex]++;
                            totalMovesUntilWin[finalState.winnerIndex] += game.GetRecord().GameStates.Length - 1;
                        }
                    }
                }
            }
            sb.AppendLine($"Result after {gameCountPerAgentConfig} games:");
            for (int i = 0; i < agents.Count; i++) {
                sb.Append($" - Agent {i} ({agents[i].Id}) won {wins[i]} times ");
                if (wins[i] > 0) {
                    var avgMoves = (float)(totalMovesUntilWin[i]) / wins[i];
                    sb.Append($"in a game of on average {avgMoves} moves");
                }
                sb.AppendLine();
            }
            sb.AppendLine($" - There were {draws} draws");
            sb.AppendLine();
        }
        Console.WriteLine();
        Console.WriteLine(sb.ToString());
        sw.Stop();
        return sw.ElapsedMilliseconds;
    }

    static async Task RunGamesInParallel<TGame> (IEnumerable<TGame> games) where TGame : Game {
        var tasks = new List<Task>();
        foreach (var game in games) {
            Console.WriteLine($"{System.DateTime.Now.ToLongTimeString()} run async");
            tasks.Add(game.RunAsync());
        }
        await Task.WhenAll(tasks);
    }

}

