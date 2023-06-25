// See https://aka.ms/new-console-template for more information

using MasterProject;
using MasterProject.TicTacToe;
using MasterProject.G44P;
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

        //DoTTTTournament();
        DoTTTBootCamp();

        //TestG44P();
        //DoG44PTournament();
        //DoG44PBootCamp();

        DataSaver.Flush();
        Logger.Flush();
    }

    static void TestG44P () {
        //Tournament<G44PGame>.RunSingleAgentTournamentVsRandom(new MasterProject.G44P.Agents.ABAgent(new MasterProject.G44P.RatingFunctions.MaximizeLead(), 8), 100);             // 2m 0s
        //Tournament<G44PGame>.RunSingleAgentTournamentVsRandom(new MasterProject.G44P.Agents.IgnoreOpponentMoves(new MasterProject.G44P.RatingFunctions.MaximizeLead(), 8), 100); // 0m 0s (too fast to measure)

        void TestThatTheFilterActuallyWorks () {
            var agents = new Agent[]{
                new MasterProject.G44P.Agents.IgnoreOpponentMoves(new MasterProject.G44P.RatingFunctions.MaximizeLead(), 4),
                new MasterProject.G44P.Agents.RandomAgent(),
            };
            var agentIds = agents.Select(a => a.Id).ToArray();
            var wlr = WinLossDrawRecord.New(agentIds, 4);
            var filter = MatchupFilter.EnsureAgentIsContainedOncePerMatchup(agents[0]);
            for (int i = 0; i < wlr.matchupRecords.Length; i++) {
                var m = wlr.GetMatchupFromIndex(i);
                var sb = new System.Text.StringBuilder();
                foreach (var id in m) {
                    sb.AppendLine($" - {id}");
                }
                if (filter.PreventMatchup(m)) {
                    Console.WriteLine($"{sb} --> PREVENTED\n");
                } else {
                    Console.WriteLine($"{sb} --> ALLOWED\n");
                }
            }
        }

        void TestTheParametrizedRatingFunction () {
            var gs = new G44PGameState();
            gs.Initialize(null);
            gs.PlayerStates[0].Points = 2;
            gs.PlayerStates[1].Points = 6;
            gs.PlayerStates[2].Points = 5;
            gs.PlayerStates[3].Points = 5;
            var p = new MasterProject.G44P.RatingFunctions.ParametrizedRatingFunction.Parameters() {
                ownScoreMultiplier = 1,
                otherScoreMultipliers = new float[3] { -1, -0.5f, -0.25f }
            };
            var rf = new MasterProject.G44P.RatingFunctions.ParametrizedRatingFunction(p);
            Console.WriteLine(rf.Evaluate(0, gs, 1));
            Console.WriteLine(rf.Evaluate(1, gs, 1));
            Console.WriteLine(rf.Evaluate(2, gs, 1));
            Console.WriteLine(rf.Evaluate(3, gs, 1));
        }

        void DoHardcodedAndParametrizedComparison () {
            var t1 = Tournament<G44PGame>.RunSingleAgentTournamentVsRandom(
            new MasterProject.G44P.Agents.IgnoreOpponentMoves(
                new MasterProject.G44P.RatingFunctions.MaximizeLead(),
                4
            ),
            totalGameCountPerMatchup: 100,
            parallelGameCount: 16
        );
            var r1 = t1.GetWinLossDrawRecord();
            DataSaver.SaveInProject("ComparisonPart1.tournamentResult", r1.ToJsonBytes());

            var t2 = Tournament<G44PGame>.RunSingleAgentTournamentVsRandom(
                new MasterProject.G44P.Agents.IgnoreOpponentMoves(
                    new MasterProject.G44P.RatingFunctions.ParametrizedRatingFunction(
                        new MasterProject.G44P.RatingFunctions.ParametrizedRatingFunction.Parameters() {
                            ownScoreMultiplier = 1,
                            otherScoreMultipliers = new float[3] { -1, -0.5f, -0.25f }
                        }
                    ),
                    4
                ),
                totalGameCountPerMatchup: 100,
                parallelGameCount: 16
            );
            var r2 = t2.GetWinLossDrawRecord();
            DataSaver.SaveInProject("ComparisonPart2.tournamentResult", r2.ToJsonBytes());

            var r = MasterProject.Records.WinLossDrawRecord.Merge(r1, r2);
            DataSaver.SaveInProject("Comparison.tournamentResult", r.ToJsonBytes());
        }
    }

    static void DoG44PTournament () {
        DoTournament<G44PGame>(
            continueId: "MasterProject.G44P.G44PGame_638215217298322126",
            //continueId: "",
            numberOfPlayersPerMatchup: G44PGameState.PLAYER_COUNT,
            numberOfGamesToPlay: 2,
            filter: MatchupFilter.AllowAllMatchups,
            agents: new Agent<G44PGame, G44PGameState, G44PMove>[] {
                new MasterProject.G44P.Agents.RandomAgent(),
                new MasterProject.G44P.Agents.Random2x(),
                //new MasterProject.G44P.Agents.ZigZag(),
                //new MasterProject.G44P.Agents.ZagZig(),
                //new MasterProject.G44P.Agents.OnlyFirst(),
                //new MasterProject.G44P.Agents.OnlyLast(),
                new MasterProject.G44P.Agents.ABAgent(new MasterProject.G44P.RatingFunctions.MaximizeOwnScore(), 4),
                new MasterProject.G44P.Agents.ABAgent(new MasterProject.G44P.RatingFunctions.MaximizeOwnScore(), 8),
                new MasterProject.G44P.Agents.ABAgent(new MasterProject.G44P.RatingFunctions.MaximizeLead(), 4),
                new MasterProject.G44P.Agents.ABAgent(new MasterProject.G44P.RatingFunctions.MaximizeLead(), 8),
                new MasterProject.G44P.Agents.IgnoreOpponentMoves(new MasterProject.G44P.RatingFunctions.MaximizeLead(), 4),
                new MasterProject.G44P.Agents.IgnoreOpponentMoves(new MasterProject.G44P.RatingFunctions.MaximizeLead(), 8),
                new MasterProject.G44P.Agents.IgnoreOpponentMoves(new MasterProject.G44P.RatingFunctions.MaximizeLead(), 12),
                new MasterProject.G44P.Agents.IgnoreOpponentMoves(new MasterProject.G44P.RatingFunctions.MaximizeLead(), 16),
            },
            saveResult: true,
            onBeforeRun: (tournament) => {
                tournament.AutosaveIntervalMinutes = 1;
                //tournament.AllowedGameConsoleOutputs = Game.ConsoleOutputs.Everything;
            }
        );
    }

    static void DoG44PBootCamp () {
        const string continueId = "";
        const int genCount = 3;
        DoBootCamp<G44PGame, G44PIndividual>(
            continueId: continueId,
            genCount: genCount,
            BootCamp.DefaultGenerationConfig,
            //BootCamp.DefaultTournamentConfig(4),
            BootCamp.FastTournamentConfig(playersPerGame: 4, gameReductionFactor: 10),
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

