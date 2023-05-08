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

    public static void Main (string[] args) {
        //MasterProject.TicTacToe.TTTGame.RunHumanTwoPlayerGame();
        //PlayAgainstBot(new MasterProject.TicTacToe.Agents.AlphaBetaMinMaxer(), 5000);
        DoSyncAsyncTest(100);
        //DoTheThing(1000, true).GetAwaiter().GetResult();
        //DoTheThing(1000, false).GetAwaiter().GetResult();
        //DoTheThing(1000, true).GetAwaiter().GetResult();
        //DoTheThing(1000, false).GetAwaiter().GetResult();
    }

    static async Task DoTheThing (int duration, bool trulyAsync) {
        Console.WriteLine($"Doing the thing {((trulyAsync) ? "for real" : "but not really")}");
        var tasks = new List<Task>();
        tasks.Add(WaitAndLog(duration, trulyAsync, "lol"));
        tasks.Add(WaitAndLog(duration, trulyAsync, "lmao"));
        tasks.Add(WaitAndLog(duration, trulyAsync, "rofl"));
        tasks.Add(WaitAndLog(duration, trulyAsync, "roflmao"));
        await Task.WhenAll(tasks);
    }

    static async Task WaitAndLog (int waitDuration, bool useAsync, string logMsg) {
        if (useAsync) {
            await Task.Delay(waitDuration);
        } else {
            await Task.Run(() => {      // if this task.run isn't here this entire method will run synchronously
                var sw = new System.Diagnostics.Stopwatch();
                sw.Start();
                var foo = 0;
                var rng = new System.Random();
                while (sw.ElapsedMilliseconds < waitDuration) {
                    foo ^= rng.Next();
                }
                sw.Stop();
            });
        }
        Console.WriteLine($"{System.DateTime.Now.ToLongTimeString()} {logMsg}");

    }

    // 100 games result:
    // Synced took 22659ms
    // Parallel took 8573ms
    // 37.83% of the duration

    // 1000 games result:
    // Synced took 225944ms
    // Parallel took 80349ms
    // 35.35% of the duration

    // conclusion: just running all of them and awaiting all the results is way faster than the alternative. c# takes care of stuff for you so you don't have to manage thread counts or anything.

    // interesting: with 0 ms move timeout the results are like this
    // Synced took 670ms
    // Parallel took 7138ms
    // this shows that of the 8s for the 100 game run, 7s is just task overhead and it's still faster
    // so these benefits will scale for more complicated games as the overhead will be comparatively lower

    static void DoSyncAsyncTest (int gameCount, int timeoutMillis = Game.NO_TIMEOUT) {
        var sw = new System.Diagnostics.Stopwatch();

        sw.Restart();
        DoBotTournament(gameCount, gameCount, timeoutMillis);
        sw.Stop();
        var asyncMillis = sw.ElapsedMilliseconds;

        sw.Restart();
        DoBotTournament(gameCount, 0, timeoutMillis);
        sw.Stop();
        var syncMillis = sw.ElapsedMilliseconds;

        Console.WriteLine($"Synced took {syncMillis}ms");
        Console.WriteLine($"Parallel took {asyncMillis}ms");
    }

    static void PlayAgainstBot (MasterProject.TicTacToe.TTTAgent bot, int timeoutMillis = Game.NO_TIMEOUT) {
        MasterProject.TicTacToe.TTTGame.PlayAgainstBot(
            bot,
            new System.Random().NextDouble() < 0.5d,
            timeoutMillis,
            Game.ConsoleOutputs.Everything
        );
    }

    // TODO make a minmaxer that uses a generic minmaxing thing
    static void DoBotTournament (int gameCountPerAgentConfig, int threadCount = 1, int timeoutMillis = Game.NO_TIMEOUT) {
        threadCount = Math.Max(1, threadCount);
        var agentConfigs = new List<List<MasterProject.TicTacToe.TTTAgent>>() {
            new List<MasterProject.TicTacToe.TTTAgent>(){
                new MasterProject.TicTacToe.Agents.MinMaxer(),
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
                        }
                    }
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

    static async Task RunGamesInParallel<TGame> (IEnumerable<TGame> games) where TGame : Game {
        var tasks = new List<Task>();
        foreach (var game in games) {
            Console.WriteLine($"{System.DateTime.Now.ToLongTimeString()} run async");
            tasks.Add(game.RunAsync());
        }
        await Task.WhenAll(tasks);
    }

}

