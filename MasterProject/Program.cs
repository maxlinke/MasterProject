// See https://aka.ms/new-console-template for more information

using MasterProject;
using MasterProject.TicTacToe;
using MasterProject.G44P;
using MasterProject.Records;
using MasterProject.MachineLearning;
using System.Text.Json;
using System.Text.Json.Serialization;
using TTTIndividual = MasterProject.TicTacToe.MachineLearning.TTTIndividual;

public class Program {

    public static string GetProjectPath () {
        var binPath = Environment.CurrentDirectory;
        if (binPath.Contains("\\bin\\Debug\\")) {
            return binPath.Substring(0, binPath.LastIndexOf("\\bin\\Debug\\") + 1);
        }
        return binPath;
    }

    public static void Main (string[] args) {
        //var foo = new MasterProject.TicTacToe.MachineLearning.AgentParameters();
        //var bar = new MasterProject.TicTacToe.MachineLearning.AgentParameters();
        //Console.WriteLine(foo.GetHashCode());
        //Console.WriteLine(bar.GetHashCode());
        //foo.winScore = 1;
        //Console.WriteLine(foo.GetHashCode());
        //var json = JsonSerializer.SerializeToUtf8Bytes(foo);
        //var fooClone = JsonSerializer.Deserialize<MasterProject.TicTacToe.MachineLearning.AgentParameters>(json);
        //Console.WriteLine(foo.GetHashCode());

        //DoTTTTournament();
        DoTTTBootCamp();

        //TestG44P();
        //DoG44PTournament();

        // TODO
        // make simple ttt-individuals
        // a weighting for winning, drawing and losing
        // and a weight for how often a random move should be played anyways
        // and for starters, just check if the serialization even works
        // and the deserialization of course
        // so do a bootcamp with the absolute minimum
        // also maybe add some console logs to whenever a generation is done training
        // then, when the serialization reliably works, do the ttt-tournament in earnest
        // and if that works, do one for g44p
        // the individuals (thanks to inheritance) need to be their own classes, containing the ratingparameters objects
        // shouldn't be too hard though. 

        DataSaver.Flush();
    }

    // TODO option for game log files
    //  -> not even an option
    //  -> just make a logger class
    //  -> and when main terminates, write the log to file
    //  -> https://stackoverflow.com/questions/4470700/how-to-save-console-writeline-output-to-text-file
    //  -> but do it with the manual logger instead
    // TODO option for tournament log files
    // TODO non-optional tournament error log files
    // TODO make the visualizer adapt to a higher player count by not having 50% hardcoded as neutral but dependent on the player count per matchup with 25% being neutral for 4 players for example

    // TODO continue the previous g44p tournament with a depth 4 and depth 8 simple ab agent
    // then add more ab agents and play more
    // then do the whole machine learning thing
    // and finally put the winner of that into the tournament as well and see the performance of that compared to explicit solutions

    // TODO if log enabled method
    // so i don't have to comment and uncomment the nice debug output

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

    // TODO continuing the bootcamp gives this exception (invaliddataexception in tournament.verifyagents or something)
    // Agent type MasterProject.TicTacToe.MachineLearning.ParametrizedABAgent returned same id as agent type MasterProject.TicTacToe.MachineLearning.ParametrizedABAgent (MasterProject.TicTacToe.MachineLearning.ParametrizedABAgent_28297030)!
	// Agent type MasterProject.TicTacToe.MachineLearning.ParametrizedABAgent returned same id as agent type MasterProject.TicTacToe.MachineLearning.ParametrizedABAgent(MasterProject.TicTacToe.MachineLearning.ParametrizedABAgent_-2114008383)!
	// Agent type MasterProject.TicTacToe.MachineLearning.ParametrizedABAgent returned same id as agent type MasterProject.TicTacToe.MachineLearning.ParametrizedABAgent(MasterProject.TicTacToe.MachineLearning.ParametrizedABAgent_7672059)!
	// Agent type MasterProject.TicTacToe.MachineLearning.ParametrizedABAgent returned same id as agent type MasterProject.TicTacToe.MachineLearning.ParametrizedABAgent(MasterProject.TicTacToe.MachineLearning.ParametrizedABAgent_28297030)!
	// Agent type MasterProject.TicTacToe.MachineLearning.ParametrizedABAgent returned same id as agent type MasterProject.TicTacToe.MachineLearning.ParametrizedABAgent(MasterProject.TicTacToe.MachineLearning.ParametrizedABAgent_-2114008383)!
	// Agent type MasterProject.TicTacToe.MachineLearning.ParametrizedABAgent returned same id as agent type MasterProject.TicTacToe.MachineLearning.ParametrizedABAgent(MasterProject.TicTacToe.MachineLearning.ParametrizedABAgent_7672059)!
    // either somehow my offspring creation results in duplicates (the only good explanation with the ids being hashcodes)
    // or something else is happening that i'll need to debug
    // it is very unlikely that this is random chance though
    // i could get the agent types and parent indices for one, see what kind of agents are causing the problems
    // and on another hand, i could just use guids for them and the parameters instead of auto-generated hashcodes

    // TODO test both running a single generation, saving and running the second generation from loaded data
    // and just doing two generations from the get-go
    static void DoTTTBootCamp () {
        var bcId = "";
        //var bcId = "BC_638222022448911356_Generation1";
        var genCount = 2;
        BootCamp<TTTGame, TTTIndividual> bc;
        if (!BootCamp<TTTGame, TTTIndividual>.TryLoad(bcId, out bc)) {
            Console.WriteLine("NEW BOOTCAMP!!!");
            bc = BootCamp<TTTGame, TTTIndividual>.Create(
                BootCamp.DefaultGenerationConfig,
                BootCamp.DefaultTournamentConfig(2),
                BootCamp.DefaultFitnessWeighting
            );
        } else {
            Console.WriteLine("CONTINUING!!!");
        }

        //var prettyPrint = new JsonSerializerOptions { WriteIndented = true };
        //for (int i = 0; i < bc.currentGeneration.Count; i++) {
        //    var individualI = bc.currentGeneration[i];
        //    var agentI = individualI.CreateAgent();
        //    for (int j = i + 1; j < bc.currentGeneration.Count; j++) {
        //        var individualJ = bc.currentGeneration[j];
        //        var agentJ = individualJ.CreateAgent();
        //        if (agentI.Id == agentJ.Id) {
        //            Console.WriteLine($">>> {i} and {j} create agents with the same id!");
        //            Console.WriteLine($"{i} is {individualI.IndividualType} with parents {GetParents(individualI)}");
        //            Console.WriteLine($"parameters for {i} are {JsonSerializer.Serialize(individualI.agentParams, prettyPrint)}");
        //            Console.WriteLine($"{j} is {individualJ.IndividualType} with parents {GetParents(individualJ)}");
        //            Console.WriteLine($"parameters for {j} are {JsonSerializer.Serialize(individualJ.agentParams, prettyPrint)}");
        //            Console.WriteLine();
        //        }
        //    }
        //}
        //
        //string GetParents (TTTIndividual individual) {
        //    var sb = new System.Text.StringBuilder();
        //    if (individual.parentIndices.Length > 0) {
        //        sb.Append(individual.parentIndices[0]);
        //        for (int i = 1; i < individual.parentIndices.Length; i++) {
        //            sb.Append($", {individual.parentIndices[i]}");
        //        }
        //    }
        //    return sb.ToString();
        //}
        //return; // TODO remove

        bc.RunUntil(BootCampTerminationCondition<TTTGame, TTTIndividual>.AfterFixedNumberOfGenerations(genCount));
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

}

