// See https://aka.ms/new-console-template for more information

using MasterProject;
using MasterProject.TicTacToe;
using MasterProject.TicTacToe.Agents;
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
        //ExceptionInTaskTest();
        //ExceptionsInTasksTest();

        var tournament = Tournament<TTTGame>.New(2);
        tournament.MaxNumberOfGamesToRunInParallel = 16;
        tournament.Run(new TTTAgent[]{
            new RandomAgent(),
            new RandomAgentWithLookAhead(),
            new LineBuilder(),
            new ABWinFast(),
            new ABLoseFast()
        }, 100);
        tournament.SaveWinLossDrawRecord();

        DataSaver.Flush();
    }

    static void ExceptionInTaskTest () {
        try {
            var task = Task.Run(TaskWithException);
            Console.WriteLine($"{System.DateTime.Now.ToLongTimeString()} task started, waiting");
            Thread.Sleep(3000);
            Console.WriteLine($"{System.DateTime.Now.ToLongTimeString()} wait finished");
            task.GetAwaiter().GetResult();  // this is where the exception gets caught
        } catch (System.Exception e) {
            Console.WriteLine($"Caught exception {e.GetType()} \"{e.Message}\"\n{e.StackTrace}");
        }
    }

    static void ExceptionsInTasksTest () {
        var tasks = new Task<int>[4];
        for (int i = 0; i < tasks.Length; i++) {
            tasks[i] = (i % 2 == 0
                ? Task.Run(TaskWithException)
                : Task.Run(TaskWithoutException));
        }
        Console.WriteLine($"{System.DateTime.Now.ToLongTimeString()} tasks started, waiting");
        Thread.Sleep(3000);
        Console.WriteLine($"{System.DateTime.Now.ToLongTimeString()} wait finished");
        foreach (var task in tasks) {
            try {
                var result = task.GetAwaiter().GetResult();
                Console.WriteLine($"task result: {result}");
            } catch (System.Exception e) {
                Console.WriteLine($"Caught exception {e.GetType()} \"{e.Message}\"\n{e.StackTrace}");
            }
        }
        //Task.WhenAll(tasks).GetAwaiter().GetResult();     // doesn't let me discern where exceptions happened and where it went smoothly
        
    }

    static async Task<int> TaskWithException () {
        await Task.Delay(100);
        Console.WriteLine($"{System.DateTime.Now.ToLongTimeString()} exception about to be thrown");
        throw new Exception("hello");
    }

    static async Task<int> TaskWithoutException() {
        await Task.Delay(100);
        return 42;
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

            //new List<MasterProject.TicTacToe.TTTAgent>(){
            //    new MasterProject.TicTacToe.Agents.ABWin(),
            //    new MasterProject.TicTacToe.Agents.RandomAgent(),
            //},
            //new List<MasterProject.TicTacToe.TTTAgent>(){
            //    new MasterProject.TicTacToe.Agents.ABWinFast(),
            //    new MasterProject.TicTacToe.Agents.RandomAgent(), 
            //}

            new List<MasterProject.TicTacToe.TTTAgent>(){
                new MasterProject.TicTacToe.Agents.ABWinFast(),
                new MasterProject.TicTacToe.Agents.ABLoseFast(),
            },
            new List<MasterProject.TicTacToe.TTTAgent>(){
                new MasterProject.TicTacToe.Agents.ABLoseFast(),
                new MasterProject.TicTacToe.Agents.ABWinFast(),
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
                    game.AgentMoveTimeoutMilliseconds = timeoutMillis;
                    game.RunSynced(agents);
                    var finalState = game.GetFinalGameState();
                    if (finalState.WinnerIndex < 0) {
                        draws++;
                    } else {
                        wins[finalState.WinnerIndex]++;
                        totalMovesUntilWin[finalState.WinnerIndex] += game.GetRecord().GameStates.Length - 1;
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
                        games[j].AgentMoveTimeoutMilliseconds = timeoutMillis;
                    }
                    Console.WriteLine($"{System.DateTime.Now.ToLongTimeString()} start wait");
                    RunGamesInParallel(games, agents).GetAwaiter().GetResult();
                    Console.WriteLine($"{System.DateTime.Now.ToLongTimeString()} finish wait");
                    foreach (var game in games) {
                        var finalState = game.GetFinalGameState();
                        if (finalState.WinnerIndex < 0) {
                            draws++;
                        } else {
                            wins[finalState.WinnerIndex]++;
                            totalMovesUntilWin[finalState.WinnerIndex] += game.GetRecord().GameStates.Length - 1;
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

    static async Task RunGamesInParallel<TGame> (IEnumerable<TGame> games, IEnumerable<Agent> agents) where TGame : Game {
        var tasks = new List<Task>();
        foreach (var game in games) {
            Console.WriteLine($"{System.DateTime.Now.ToLongTimeString()} run async");
            tasks.Add(game.RunAsync(agents));
        }
        await Task.WhenAll(tasks);
    }

}

