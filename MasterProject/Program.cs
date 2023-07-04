// See https://aka.ms/new-console-template for more information

using MasterProject;
using MasterProject.TicTacToe;
using MasterProject.G44P;
using MasterProject.Chess;
using MasterProject.Records;
using MasterProject.MachineLearning;
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

    public static void Main (string[] args) {
        Logger.consoleOnly = true;

        TestChess();

        //DoTTTTournament();
        //DoTTTBootCamp();

        //DoG44PTournament();
        //DoG44PBootCamp();


        // testing if i could simplify updating/copying player states
        // if they are simply value type
        // i would have to make it generic with a generic payload parameter
        // but that's about it
        // for the ones without payload, i'll need a default payload

        var s = new GenericStruct<OtherStruct>[2];
        //s[0].myT.myString = "Hello, world!";                            // illegal
        //s[0].myT = new OtherStruct() { myString = "Hello, world!" };    // cumbersome
        s[0].myT = s[0].myT.WithString("Hello, world!");                  // slightly less cumbersome
        //s[0].myT.SetString("Hello, world!");                            // doesn't work
        Console.WriteLine(s[0].myT.myString);
        var s2 = (GenericStruct<OtherStruct>[])(s.Clone());
        Console.WriteLine(s2[0].myT.myString);

        DataSaver.Flush();
        Logger.Flush();
    }

    struct GenericStruct<T> where T : struct {
        public int myInt { get; set; }
        public T myT { get; set; }
    }

    struct OtherStruct {
        public string myString { get; set; }
        public OtherStruct WithString (string s) {
            return new OtherStruct() {
                myString = s
            };
        }
        public void SetString (string s) {
            myString = s;
        }
    }

    static void TestChess () {
        //var gs = new ChessGameState();
        //gs.Initialize();
        //Console.WriteLine(gs.ToPrintableString());
        //gs.GetPossibleMovesForCurrentPlayer();

        //var sw = new System.Diagnostics.Stopwatch();
        //sw.Start();
        //for (int i = 0; i < 100; i++) {
        //    var g = new ChessGame();
        //    g.AllowedConsoleOutputs = Game.ConsoleOutputs.Everything;
        //    g.RunSynced(new ChessAgent[]{
        //        new MasterProject.Chess.Agents.RandomAgent(),
        //        new MasterProject.Chess.Agents.RandomAgent()
        //    });
        //}
        //sw.Stop();
        //var withLogs = sw.ElapsedMilliseconds;
        //sw.Restart();
        //for (int i = 0; i < 100; i++) {
        //    var g = new ChessGame();
        //    g.AllowedConsoleOutputs = Game.ConsoleOutputs.Nothing;
        //    g.RunSynced(new ChessAgent[]{
        //        new MasterProject.Chess.Agents.RandomAgent(),
        //        new MasterProject.Chess.Agents.RandomAgent()
        //    });
        //}
        //sw.Stop();
        //var withoutLogs = sw.ElapsedMilliseconds;
        //Console.WriteLine();
        //Console.WriteLine();
        //Console.WriteLine($"with logs: {withLogs} ms");           // about 32 seconds
        //Console.WriteLine($"without logs: {withoutLogs} ms");     // about 4 seconds
        //Console.WriteLine();
        //Console.WriteLine();

        //var g = new ChessGame();
        //g.AllowedConsoleOutputs = ~Game.ConsoleOutputs.Debug;
        //g.RunSynced(new ChessAgent[]{
        //    new MasterProject.Chess.Agents.Human(),
        //    new MasterProject.Chess.Agents.RandomAgent()
        //});

        var g = new ChessGame();
        g.AllowedConsoleOutputs = Game.ConsoleOutputs.Everything;
        g.RunSynced(new Agent<ChessGame, ChessGameState, ChessMove>[]{
            //new MasterProject.HumanObserverWrapperAgent<ChessGame, ChessGameState, ChessMove>(new MasterProject.Chess.Agents.SuicideKing()),
            //new MasterProject.HumanObserverWrapperAgent<ChessGame, ChessGameState, ChessMove>(new MasterProject.Chess.Agents.SameColor()),
            //new MasterProject.Chess.Agents.RandomAgent()
            new MasterProject.Chess.Agents.SameColor(),
            new MasterProject.Chess.Agents.OppositeColor(),
        });
        while (true) {
            Console.WriteLine("Finished");
            Thread.Sleep(100);
            Console.ReadLine();
        }
    }

    static void DoG44PTournament () {
        var bestFromBootCamp = BootCamp<G44PGame, G44PIndividual>.Load("BC_638233760553840225_Generation10").GetFittestIndividual();
        Console.WriteLine($"Fittest individual from bootcamp is:\n{JsonUtility.ToJson(bestFromBootCamp, true)}\n");
        DoTournament<G44PGame>(
            //continueId: "MasterProject.G44P.G44PGame_638215217298322126",
            continueId: "",
            numberOfPlayersPerMatchup: G44PGameState.PLAYER_COUNT,
            numberOfGamesToPlay: 100,
            filter: MatchupFilter.OnlyThisAgentExceptOneOther(new MasterProject.G44P.Agents.RandomAgent()),
            //agents: new Agent<G44PGame, G44PGameState, G44PMove>[] {
            //    new MasterProject.G44P.Agents.RandomAgent(),
            //    new MasterProject.G44P.Agents.Random2x(),
            //    //new MasterProject.G44P.Agents.ZigZag(),
            //    //new MasterProject.G44P.Agents.ZagZig(),
            //    //new MasterProject.G44P.Agents.OnlyFirst(),
            //    //new MasterProject.G44P.Agents.OnlyLast(),
            //    new MasterProject.G44P.Agents.ABAgent(new MasterProject.G44P.RatingFunctions.MaximizeOwnScore(), 4),
            //    new MasterProject.G44P.Agents.ABAgent(new MasterProject.G44P.RatingFunctions.MaximizeOwnScore(), 8),
            //    new MasterProject.G44P.Agents.ABAgent(new MasterProject.G44P.RatingFunctions.MaximizeLead(), 4),
            //    new MasterProject.G44P.Agents.ABAgent(new MasterProject.G44P.RatingFunctions.MaximizeLead(), 8),
            //    new MasterProject.G44P.Agents.IgnoreOpponentMoves(new MasterProject.G44P.RatingFunctions.MaximizeLead(), 4),
            //    new MasterProject.G44P.Agents.IgnoreOpponentMoves(new MasterProject.G44P.RatingFunctions.MaximizeLead(), 8),
            //    new MasterProject.G44P.Agents.IgnoreOpponentMoves(new MasterProject.G44P.RatingFunctions.MaximizeLead(), 12),
            //    new MasterProject.G44P.Agents.IgnoreOpponentMoves(new MasterProject.G44P.RatingFunctions.MaximizeLead(), 16),
            //},
            agents: new Agent<G44PGame, G44PGameState, G44PMove>[] {
                new MasterProject.G44P.Agents.RandomAgent(),
                new MasterProject.G44P.Agents.ABAgent(new MasterProject.G44P.RatingFunctions.MaximizeLead(), 8),
                new MasterProject.G44P.Agents.IgnoreOpponentMoves(new MasterProject.G44P.RatingFunctions.MaximizeLead(), 8),
                (Agent<G44PGame, G44PGameState, G44PMove>)(bestFromBootCamp.CreateAgent())
            },
            saveResult: true,
            onBeforeRun: (tournament) => {
                tournament.AutosaveIntervalMinutes = 5;
                //tournament.AllowedGameConsoleOutputs = Game.ConsoleOutputs.Everything;
            }
        );
    }

    static void DoG44PBootCamp () {
        const string continueId = "";
        const int genCount = 10;
        var tc = BootCamp.DefaultTournamentConfig(playersPerGame: 4);
        tc.parallelGameCount = 1024;    // to reduce the console spam
        DoBootCamp<G44PGame, G44PIndividual>(
            continueId: continueId,
            genCount: genCount,
            BootCamp.DefaultGenerationConfig,
            //BootCamp.DefaultTournamentConfig(4),
            //BootCamp.FastTournamentConfig(playersPerGame: 4, gameReductionFactor: 10),
            tc,
            BootCamp.DefaultFitnessWeighting
        );
    }

    static void DoTTTTournament () {
        //const string continueTournamentId = "MasterProject.TicTacToe.TTTGame_638204554117514045";
        const string continueTournamentId = "";
        const int numberOfGamesToPlay = 10;
        var filter = MatchupFilter.PreventMirrorMatches;
        DoTournament<TTTGame>(
            continueId: continueTournamentId,
            numberOfPlayersPerMatchup: 2,
            numberOfGamesToPlay: numberOfGamesToPlay,
            filter: filter,
            agents: new Agent<TTTGame, TTTGameState, TTTMove>[]{
                //new MasterProject.TicTacToe.Agents.RandomAgent(),
                //new MasterProject.TicTacToe.Agents.RandomAgentWithLookAhead(),
                //new MasterProject.TicTacToe.Agents.LineBuilder(),
                //new MasterProject.TicTacToe.Agents.ABWin(),
                //new MasterProject.TicTacToe.Agents.ABLose(),
                new MasterProject.TicTacToe.Agents.ABWinFast(),
                //new MasterProject.TicTacToe.Agents.ABLoseFast(),
                //new DilutedAgent<TTTGame, TTTGameState, TTTMove>(new MasterProject.TicTacToe.Agents.ABWinFast(), 0.5f),
                //new DilutedAgent<TTTGame, TTTGameState, TTTMove>(new MasterProject.TicTacToe.Agents.ABWinFast(), 0.8f),
                //new DilutedAgent<TTTGame, TTTGameState, TTTMove>(new MasterProject.TicTacToe.Agents.ABWinFast(), 0.9f),
                new MasterProject.TicTacToe.MachineLearning.ParametrizedABAgent(
                    new MasterProject.TicTacToe.MachineLearning.AgentParameters(){
                        winScore = 1,
                        drawScore = 0,
                        lossScore = -1,
                        randomProbability = 0
                    },
                    System.Guid.NewGuid().ToString()
                )
            },
            saveResult: true,
            onBeforeRun: (tournament) => {
                tournament.PlayEachMatchupToCompletionBeforeMovingOntoNext = false;
                tournament.AutosaveIntervalMinutes = 1;
            }
        );
    }

    static void DoTTTBootCamp () {
        var bcId = "";
        //var bcId = "BC_638222022448911356_Generation1";
        var genCount = 10;
        DoBootCamp<TTTGame, MasterProject.TicTacToe.MachineLearning.GenericTTTIndividual>(
            bcId, 
            genCount,
            BootCamp.DefaultGenerationConfig,
            //BootCamp.DefaultTournamentConfig(2),
            BootCamp.FastTournamentConfig(playersPerGame: 2, gameReductionFactor: 4),
            BootCamp.DefaultFitnessWeighting
        );
    }

    static void DoTournament<TGame> (string continueId, int numberOfPlayersPerMatchup, int numberOfGamesToPlay, IMatchupFilter filter, IReadOnlyList<Agent> agents, bool saveResult, System.Action<Tournament<TGame>> onBeforeRun = null) where TGame : Game, new() {
        Tournament<TGame> tournament;
        if (Tournament.TryLoadWinLossDrawRecord(continueId, out var loadedRecord)) {
            tournament = Tournament<TGame>.Continue(loadedRecord);
            Console.WriteLine("Continuing!");
        } else {
            tournament = Tournament<TGame>.New(numberOfPlayersPerMatchup);
            Console.WriteLine("New!");
        }

        tournament.MaxNumberOfGamesToRunInParallel = 16;
        onBeforeRun?.Invoke(tournament);
        tournament.Run(agents, numberOfGamesToPlay, filter);
        if (saveResult) {
            tournament.SaveWinLossDrawRecord();
        }
    }

    static void DoBootCamp<TGame, TIndividual> (string continueId, int genCount, BootCamp.GenerationConfiguration gc, BootCamp.TournamentConfiguration tc, BootCamp.FitnessWeighting fw) where TGame: Game, new() where TIndividual: Individual, new() {
        BootCamp<TGame, TIndividual> bc;
        if (!BootCamp<TGame, TIndividual>.TryLoad(continueId, out bc)) {
            Console.WriteLine("NEW BOOTCAMP!!!");
            bc = BootCamp<TGame, TIndividual>.Create(gc, tc, fw);
        } else {
            Console.WriteLine("CONTINUING!!!");
        }
        bc.RunUntil(BootCampTerminationCondition<TGame, TIndividual>.AfterFixedNumberOfGenerations(genCount));
    }

}

