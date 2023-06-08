using MasterProject.Records;

namespace MasterProject.MachineLearning {

    public class BootCamp {

        public const int DEFAULT_NEW_INDIVIDUAL_COUNT = 2;
        public const int DEFAULT_BEST_CLONE_COUNT = 3;
        public const int DEFAULT_INVERTED_WORST_CLONE_COUNT = 1;
        public const int DEFAULT_MUTATION_COUNT = 5;
        public const int DEFAULT_COMBINATION_COUNT = 5;

        public class GenerationConfiguration {

            public int newIndividualCount      { get; set; } = BootCamp.DEFAULT_NEW_INDIVIDUAL_COUNT;
            public int bestCloneCount          { get; set; } = BootCamp.DEFAULT_BEST_CLONE_COUNT;
            public int invertedWorstCloneCount { get; set; } = BootCamp.DEFAULT_INVERTED_WORST_CLONE_COUNT;
            public int mutationCount           { get; set; } = BootCamp.DEFAULT_MUTATION_COUNT;
            public int combinationCount        { get; set; } = BootCamp.DEFAULT_COMBINATION_COUNT;

            public int generationSize => newIndividualCount
                                       + bestCloneCount
                                       + invertedWorstCloneCount
                                       + mutationCount
                                       + combinationCount;

        }

        public const int DEFAULT_GAMES_PER_MATCHUP = 10;

        public class TournamentConfiguration {

            public int playersPerGame    { get; set; }
            public int gamesPerMatchup   { get; set; } = DEFAULT_GAMES_PER_MATCHUP;
            public int maxMoveCount      { get; set; } = Game.NO_MOVE_LIMIT;
            public int maxMoveMillis     { get; set; } = Game.NO_TIMEOUT;
            public int autosaveInterval  { get; set; } = 5;
            public int parallelGameCount { get; set; } = 16;

        }

    }

    public class BootCamp<TGame, TIndividual> : BootCamp
        where TGame : Game, new()
        where TIndividual: Individual, new()
    {

        public GenerationConfiguration generationConfig { get; set; }

        public TournamentConfiguration tournamentConfig { get; set; }

        public List<TIndividual[]> generations { get; set; }

        public string latestRecordId { get; set; }

        public static BootCamp<TGame, TIndividual> Create (GenerationConfiguration generationConfig, TournamentConfiguration tournamentConfig) {
            var output = new BootCamp<TGame, TIndividual>();
            output.generationConfig = generationConfig;
            output.tournamentConfig = tournamentConfig;
            var firstGen = new TIndividual[generationConfig.generationSize];
            for (int i = 0; i < firstGen.Length; i++) {
                firstGen[i] = new TIndividual();
                firstGen[i].index = i;
                firstGen[i].InitializeWithRandomCoefficients();
            }
            output.generations = new List<TIndividual[]>() { firstGen };
            output.latestRecordId = string.Empty;
            return output;
        }

        // still needs a way to save and load

        // possibly refactor this into more explicit finishing conditions
        // like matchupfilter
        // so, one for a limited number of generations
        // one for a given fitness
        // although for that i might have to have the bots play against random bots to determine fitness
        // which would be another parameter to the tournament config
        // so one run against all the other bots to find who's best
        // then one to determine fitness against random bots
        // with the "onlyone" matchupfilter
        // now, with the thing being random and all
        // there is the possibility that the random-bot-tournament yields a different "result" than the first one
        // should fitness be the combination of both?
        // with the first tournament determining who's best among the generation
        // and the second tournament determining who's good overall?
        // i think i should do some research on common strategies here...
        public void RunUntil (System.Func<BootCamp<TGame, TIndividual>, bool> endRun) {
            while (!endRun(this)) {
                var record = DoTournament();
                CreateNextGeneration(record);
            }
        }

        WinLossDrawRecord DoTournament () {
            Tournament<TGame> tournament = null;
            var currentGen = generations[generations.Count - 1];
            var currentAgents = new List<Agent>(currentGen.Select(individual => individual.CreateAgent()));
            if(Tournament.TryLoadWinLossDrawRecord(latestRecordId, out var loadedRecord)){
                if (loadedRecord.matchupSize == this.tournamentConfig.playersPerGame) {
                    if (loadedRecord.playerIds.Length == currentAgents.Count) {
                        var isRecordForCurrentGen = true;
                        foreach (var agent in currentAgents) {
                            if (!loadedRecord.playerIds.Contains(agent.Id)) {
                                isRecordForCurrentGen = false;
                                break;
                            }
                        }
                        if (isRecordForCurrentGen) {
                            tournament = Tournament<TGame>.Continue(loadedRecord);
                        }
                    }
                }
            }
            if (tournament == null) {
                tournament = Tournament<TGame>.New(this.tournamentConfig.playersPerGame);
            }
            tournament.AgentMoveTimeoutMilliseconds = tournamentConfig.maxMoveMillis;
            tournament.AllowedGameConsoleOutputs = Game.ConsoleOutputs.Nothing;
            tournament.AutosaveIntervalMinutes = tournamentConfig.autosaveInterval;
            tournament.MaxNumberOfGamesToRunInParallel = tournamentConfig.parallelGameCount;
            tournament.MaxNumberOfMovesPerGame = tournamentConfig.maxMoveCount;
            tournament.PlayEachMatchupToCompletionBeforeMovingOntoNext = false;
            tournament.onSaved += (id) => latestRecordId = id;
            tournament.Run(currentAgents, tournamentConfig.gamesPerMatchup, MatchupFilter.PreventAnyDuplicateAgents);
            tournament.SaveWinLossDrawRecord();
            return tournament.GetWinLossDrawRecord();
        }

        void CreateNextGeneration (WinLossDrawRecord record) {
            var prevGen = generations[generations.Count - 1];
            var individualMap = new Dictionary<string, TIndividual>();
            foreach (var individual in prevGen) {
                var tempAgent = individual.CreateAgent();
                individualMap.Add(tempAgent.Id, individual);
            }
            var performanceList = new List<(string id, float winRate, float drawRate)>();
            foreach (var individualId in individualMap.Keys) {
                var index = Array.IndexOf(record.playerIds, individualId);
                var gameCount = record.totalWins[index] + record.totalLosses[index] + record.totalDraws[index];
                performanceList.Add((
                    id: individualId,
                    winRate: (float)(record.totalWins[index]) / gameCount,
                    drawRate: (float)(record.totalDraws[index]) / gameCount
                ));
            }
            performanceList.Sort((a, b) => {    // sort with highest coming first
                if (a.winRate != b.winRate)   return Math.Sign(b.winRate - a.winRate);
                if (a.drawRate != b.drawRate) return Math.Sign(b.drawRate - a.drawRate);
                return 0;
            });

            // just for testing
            Console.WriteLine("Rated by performance");
            for (int i = 0; i < performanceList.Count; i++) {
                Console.WriteLine($" > {i}: {performanceList[i].id} (wr {performanceList[i].winRate}) (dr {performanceList[i].drawRate})");
            }

            var nextGen = new List<TIndividual>(generationConfig.generationSize);
            var nextIndividualIndex = prevGen[prevGen.Length - 1].index + 1;
            AddIndividuals(generationConfig.newIndividualCount, _ => {
                var newAgent = new TIndividual();
                newAgent.InitializeWithRandomCoefficients();
                return newAgent;
            });
            AddIndividuals(generationConfig.bestCloneCount, i => {
                return GetIndividualByPerformance(i).Clone();
            });
            AddIndividuals(generationConfig.invertedWorstCloneCount, i => {
                return GetIndividualByPerformance(performanceList.Count - i - 1).InvertedClone();
            });
            AddIndividuals(generationConfig.mutationCount, i => {
                return GetIndividualByPerformance(i).MutatedClone();
            });
            AddIndividuals(generationConfig.combinationCount, i => {
                var src = GetIndividualByPerformance(i);
                var other = GetIndividualByPerformance((i + 1) % performanceList.Count);
                return src.CombinedClone(other);
            });
            generations.Add(nextGen.ToArray());

            TIndividual GetIndividualByPerformance (int index) {
                var id = performanceList[index].id;
                return individualMap[id];
            }

            void AddIndividuals (int count, System.Func<int, Individual> createIndividual) {
                for (int i = 0; i < count; i++) {
                    var newIndividual = (TIndividual)(createIndividual(i));
                    newIndividual.index = nextIndividualIndex;
                    nextGen.Add(newIndividual);
                    nextIndividualIndex++;
                }
            }
        }

    }

}
