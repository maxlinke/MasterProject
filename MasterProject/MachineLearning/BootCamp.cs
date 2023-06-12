using MasterProject.Records;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MasterProject.MachineLearning {

    public class BootCamp {

        public const int DEFAULT_NEW_INDIVIDUAL_COUNT = 2;
        public const int DEFAULT_BEST_CLONE_COUNT = 3;
        public const int DEFAULT_INVERTED_WORST_CLONE_COUNT = 1;
        public const int DEFAULT_MUTATION_COUNT = 5;
        public const int DEFAULT_COMBINATION_COUNT = 5;

        public class GenerationConfiguration {

            public int newIndividualCount      { get; set; } = DEFAULT_NEW_INDIVIDUAL_COUNT;
            public int bestCloneCount          { get; set; } = DEFAULT_BEST_CLONE_COUNT;
            public int invertedWorstCloneCount { get; set; } = DEFAULT_INVERTED_WORST_CLONE_COUNT;
            public int mutationCount           { get; set; } = DEFAULT_MUTATION_COUNT;
            public int combinationCount        { get; set; } = DEFAULT_COMBINATION_COUNT;

            public int generationSize => newIndividualCount
                                       + bestCloneCount
                                       + invertedWorstCloneCount
                                       + mutationCount
                                       + combinationCount;

        }

        public const int DEFAULT_PEER_TOURNAMENTS_MATCHUP_REPETITIONS = 10;
        public const int DEFAULT_RANDOM_TOURNAMENT_MATCHUP_REPETITIONS = 10;

        public class TournamentConfiguration {

            public int playersPerGame    { get; set; }
            public int peerTournamentMatchupRepetitionCount   { get; set; } = DEFAULT_PEER_TOURNAMENTS_MATCHUP_REPETITIONS;
            public int randomTournamentMatchupRepetitionCount { get; set; } = DEFAULT_RANDOM_TOURNAMENT_MATCHUP_REPETITIONS;
            public int maxMoveCount      { get; set; } = Game.NO_MOVE_LIMIT;
            public int maxMoveMillis     { get; set; } = Game.NO_TIMEOUT;
            public int autosaveInterval  { get; set; } = 5;
            public int parallelGameCount { get; set; } = 16;

        }

        public class FitnessWeighting {

            public float peerTournamentWeight { get; set; }   = 1f;
            public float randomTournamentWeight { get; set; } = 1f;
            public float winrateWeight { get; set; }  = 1f;
            public float drawRateWeight { get; set; } = 0.5f;
            public float lossRateWeight { get; set; } = 0f;

            public float GetRatingForIndividual (Individual individual) {
                return  (peerTournamentWeight * GetTournamentRating(individual.peerTournamentResult))
                      + (randomTournamentWeight * GetTournamentRating(individual.randomTournamentResult));

                float GetTournamentRating (Individual.TournamentResult tournamentResult) {
                    float tournamentTotal = Math.Max(1, tournamentResult.totalWins + tournamentResult.totalDraws + tournamentResult.totalLosses);
                    return (winrateWeight * (tournamentResult.totalWins / tournamentTotal))
                         + (drawRateWeight * (tournamentResult.totalDraws / tournamentTotal))
                         + (lossRateWeight * (tournamentResult.totalLosses / tournamentTotal));
                }
            }

        }

        public const string SavedataDirectoryName = "BootCampData";

        public const string SavedataFileExtension = "bootCampData";

        protected static string GetProjectPathForSavedata (string id) => $"{SavedataDirectoryName}/{id}.{SavedataFileExtension}";

    }

    public class BootCamp<TGame, TIndividual> : BootCamp
        where TGame : Game, new()
        where TIndividual: Individual, new()
    {

        public GenerationConfiguration generationConfig { get; set; }

        public TournamentConfiguration tournamentConfig { get; set; }

        public FitnessWeighting fitnessWeighting { get; set; }

        public List<TIndividual[]> generations { get; set; }

        public string latestRecordId { get; set; }

        public bool peerTournamentFinished { get; set; }

        public bool randomTournamentFinished { get; set; }

        public string id { get; set; }

        public static BootCamp<TGame, TIndividual> Create (
            GenerationConfiguration generationConfig, 
            TournamentConfiguration tournamentConfig,
            FitnessWeighting fitnessWeighting
        ) {
            var output = new BootCamp<TGame, TIndividual>();
            output.id = $"BC_{System.DateTime.Now.Ticks}";
            output.generationConfig = generationConfig;
            output.tournamentConfig = tournamentConfig;
            output.fitnessWeighting = fitnessWeighting;
            var firstGen = new TIndividual[generationConfig.generationSize];
            for (int i = 0; i < firstGen.Length; i++) {
                firstGen[i] = new TIndividual();
                firstGen[i].index = i;
                firstGen[i].InitializeWithRandomCoefficients();
            }
            output.generations = new List<TIndividual[]>() { firstGen };
            output.latestRecordId = string.Empty;
            output.peerTournamentFinished = tournamentConfig.peerTournamentMatchupRepetitionCount <= 0;
            output.randomTournamentFinished = tournamentConfig.randomTournamentMatchupRepetitionCount <= 0;
            return output;
        }

        public static bool TryLoad (string id, out BootCamp<TGame, TIndividual> output) {
            if (!string.IsNullOrWhiteSpace(id) && DataLoader.TryLoadInProject(GetProjectPathForSavedata(id), out var loadedBytes)) {
                output = JsonSerializer.Deserialize<BootCamp<TGame, TIndividual>>(loadedBytes);
                return true;
            }
            output = default;
            return false;
        }

        void Save () {
            var jsonBytes = JsonSerializer.SerializeToUtf8Bytes(this);
            DataSaver.SaveInProject(GetProjectPathForSavedata(this.id), jsonBytes);
            DataSaver.Flush();
        }

        public void RunUntil (IBootCampTerminationCondition<TGame, TIndividual> terminationCondition) {
            while (true) {
                var currentGen = generations[generations.Count - 1];
                var currentAgents = new List<Agent>(currentGen.Select(individual => individual.CreateAgent()));
                if (!peerTournamentFinished) {
                    var peerRecord = RunPeerTournament(currentAgents);
                    for (int i = 0; i < currentGen.Length; i++) {
                        ApplyTournamentResult(
                            peerRecord, 
                            currentGen[i], 
                            currentAgents[i].Id, 
                            (individual, result) => individual.peerTournamentResult = result
                        );
                    }
                    peerTournamentFinished = true;
                    Save();
                }
                if (!randomTournamentFinished) {
                    var randomRecord = RunRandomTournament(currentAgents);
                    for (int i = 0; i < currentGen.Length; i++) {
                        ApplyTournamentResult(
                            randomRecord,
                            currentGen[i],
                            currentAgents[i].Id,
                            (individual, result) => individual.randomTournamentResult = result
                        );
                    }
                    randomTournamentFinished = true;
                    Save();
                }
                if (terminationCondition.EndTraining(this)) {
                    Save();
                    break;
                }
                CreateNextGeneration();
                latestRecordId = string.Empty;
                peerTournamentFinished = tournamentConfig.peerTournamentMatchupRepetitionCount <= 0;
                randomTournamentFinished = tournamentConfig.randomTournamentMatchupRepetitionCount <= 0;
                Save();
            }
        }

        bool TryLoadLatestRecord (out WinLossDrawRecord output, out bool isRandomRecord) {
            output = null;
            isRandomRecord = false;
            if (Tournament.TryLoadWinLossDrawRecord(latestRecordId, out var loadedRecord)) {
                if (loadedRecord.matchupSize == this.tournamentConfig.playersPerGame) {
                    var randomId = new TGame().GetRandomAgent().Id;
                    output = loadedRecord;
                    isRandomRecord = loadedRecord.playerIds.Contains(randomId);
                    return true;
                }
            }
            return false;
        }

        WinLossDrawRecord RunPeerTournament (IReadOnlyList<Agent> currentAgents) {
            var gotRecord = TryLoadLatestRecord(out var latestRecord, out var latestRecordIsRandomRecord);
            Tournament<TGame> peerTournament;
            if (gotRecord && !latestRecordIsRandomRecord) {
                peerTournament = Tournament<TGame>.Continue(latestRecord);
            } else {
                peerTournament = Tournament<TGame>.New(tournamentConfig.playersPerGame);
            }
            return DoTournament(
                peerTournament, 
                currentAgents, 
                tournamentConfig.peerTournamentMatchupRepetitionCount, 
                MatchupFilter.PreventAnyDuplicateAgents
            );
        }

        WinLossDrawRecord RunRandomTournament (IReadOnlyList<Agent> currentAgents) {
            var randomAgent = new TGame().GetRandomAgent();
            var tournamentAgents = new List<Agent>() { randomAgent };
            tournamentAgents.AddRange(currentAgents);
            var gotRecord = TryLoadLatestRecord(out var latestRecord, out var latestRecordIsRandomRecord);
            Tournament<TGame> randomTournament;
            if (gotRecord && latestRecordIsRandomRecord) {
                randomTournament = Tournament<TGame>.Continue(latestRecord);
            } else {
                randomTournament = Tournament<TGame>.New(tournamentConfig.playersPerGame);
            }
            return DoTournament(
                randomTournament,
                tournamentAgents,
                tournamentConfig.randomTournamentMatchupRepetitionCount,
                MatchupFilter.OnlyThisAgentExceptOneOther(randomAgent)
            );
        }

        void ApplyTournamentResult (WinLossDrawRecord record, Individual individual, string agentId, System.Action<Individual, Individual.TournamentResult> apply) {
            var recordIndex = Array.IndexOf(record.playerIds, agentId);
            var result = new Individual.TournamentResult();
            result.totalWins = record.totalWins[recordIndex];
            result.totalDraws = record.totalDraws[recordIndex];
            result.totalLosses = record.totalLosses[recordIndex];
            apply(individual, result);
        }

        WinLossDrawRecord DoTournament (Tournament<TGame> tournament, IReadOnlyList<Agent> agents, int matchupRepetitons, IMatchupFilter filter) {
            tournament.AgentMoveTimeoutMilliseconds = tournamentConfig.maxMoveMillis;
            tournament.AllowedGameConsoleOutputs = Game.ConsoleOutputs.Nothing;
            tournament.AutosaveIntervalMinutes = tournamentConfig.autosaveInterval;
            tournament.MaxNumberOfGamesToRunInParallel = tournamentConfig.parallelGameCount;
            tournament.MaxNumberOfMovesPerGame = tournamentConfig.maxMoveCount;
            tournament.PlayEachMatchupToCompletionBeforeMovingOntoNext = false;
            tournament.SaveIdPrefix = this.id;
            tournament.onSaved += (id) => latestRecordId = id;
            tournament.Run(agents, matchupRepetitons, filter);
            tournament.SaveWinLossDrawRecord();
            return tournament.GetWinLossDrawRecord();
        }

        void CreateNextGeneration () {
            var currGen = generations[generations.Count - 1];
            SortCurrentGenerationByFitness();
            var nextGen = new List<TIndividual>(generationConfig.generationSize);
            var nextIndividualIndex = currGen[currGen.Length - 1].index + 1;
            AddIndividualsToNextGen(generationConfig.newIndividualCount, _ => {
                var newAgent = new TIndividual();
                newAgent.InitializeWithRandomCoefficients();
                return newAgent;
            });
            AddIndividualsToNextGen(generationConfig.bestCloneCount, i => {
                return GetIndividualByPerformance(i).Clone();
            });
            AddIndividualsToNextGen(generationConfig.invertedWorstCloneCount, i => {
                return GetIndividualByPerformance(currGen.Length - i - 1).InvertedClone();
            });
            AddIndividualsToNextGen(generationConfig.mutationCount, i => {
                return GetIndividualByPerformance(i).MutatedClone();
            });
            AddIndividualsToNextGen(generationConfig.combinationCount, i => {
                var src = GetIndividualByPerformance(i);
                var other = GetIndividualByPerformance((i + 1) % currGen.Length);
                return src.CombinedClone(other);
            });
            generations.Add(nextGen.ToArray());

            void SortCurrentGenerationByFitness () {
                foreach (var individual in currGen) {
                    individual.finalFitness = fitnessWeighting.GetRatingForIndividual(individual);
                }
                var sortedList = new List<TIndividual>(currGen);
                sortedList.Sort((a, b) => Math.Sign(b.finalFitness - a.finalFitness));
                for (int i = 0; i < currGen.Length; i++) {
                    currGen[i] = sortedList[i];
                }
            }

            TIndividual GetIndividualByPerformance (int index) {
                return currGen[index];
            }

            void AddIndividualsToNextGen (int count, System.Func<int, Individual> createIndividual) {
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
