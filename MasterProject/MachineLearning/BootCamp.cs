using MasterProject.Records;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MasterProject.MachineLearning {

    public abstract class BootCamp {

        public const int DEFAULT_NEW_INDIVIDUAL_COUNT = 2;
        public const int DEFAULT_BEST_CLONE_COUNT = 3;
        public const int DEFAULT_INVERTED_WORST_CLONE_COUNT = 1;
        public const int DEFAULT_MUTATION_COUNT = 5;
        public const int DEFAULT_COMBINATION_COUNT = 5;

        public class GenerationConfiguration {

            public int newIndividualCount { get; set; }
            public int bestCloneCount { get; set; }
            public int invertedWorstCloneCount { get; set; }
            public int mutationCount { get; set; }
            public int combinationCount { get; set; }

            public int generationSize => newIndividualCount
                                       + bestCloneCount
                                       + invertedWorstCloneCount
                                       + mutationCount
                                       + combinationCount;

        }

        public static GenerationConfiguration DefaultGenerationConfig => new GenerationConfiguration() {
            newIndividualCount      = DEFAULT_NEW_INDIVIDUAL_COUNT,
            bestCloneCount          = DEFAULT_BEST_CLONE_COUNT,
            invertedWorstCloneCount = DEFAULT_INVERTED_WORST_CLONE_COUNT,
            mutationCount           = DEFAULT_MUTATION_COUNT,
            combinationCount        = DEFAULT_COMBINATION_COUNT,
        };

        public const int DEFAULT_PEER_TOURNAMENTS_MATCHUP_REPETITIONS = 10;
        public const int DEFAULT_RANDOM_TOURNAMENT_MATCHUP_REPETITIONS = 100;

        public class TournamentConfiguration {

            public int playersPerGame    { get; set; }
            public int peerTournamentMatchupRepetitionCount   { get; set; } = DEFAULT_PEER_TOURNAMENTS_MATCHUP_REPETITIONS;
            public int randomTournamentMatchupRepetitionCount { get; set; } = DEFAULT_RANDOM_TOURNAMENT_MATCHUP_REPETITIONS;
            public int maxMoveCount      { get; set; } = Game.NO_MOVE_LIMIT;
            public int maxMoveMillis     { get; set; } = Game.NO_TIMEOUT;
            public int autosaveInterval  { get; set; } = 5;
            public int parallelGameCount { get; set; } = 16;

        }

        public static TournamentConfiguration DefaultTournamentConfig (int playersPerGame) {
            return new TournamentConfiguration() {
                playersPerGame = playersPerGame, 
                peerTournamentMatchupRepetitionCount = DEFAULT_PEER_TOURNAMENTS_MATCHUP_REPETITIONS,
                randomTournamentMatchupRepetitionCount = DEFAULT_RANDOM_TOURNAMENT_MATCHUP_REPETITIONS,
                maxMoveCount = Game.NO_MOVE_LIMIT,
                maxMoveMillis = Game.NO_TIMEOUT,
                autosaveInterval = 5,
                parallelGameCount = 16,
            };
        }

        public static TournamentConfiguration FastTournamentConfig (int playersPerGame, float gameReductionFactor) {
            return new TournamentConfiguration() {
                playersPerGame = playersPerGame,
                peerTournamentMatchupRepetitionCount = (int)Math.Max(1, Math.Round(DEFAULT_PEER_TOURNAMENTS_MATCHUP_REPETITIONS / gameReductionFactor)),
                randomTournamentMatchupRepetitionCount = (int)Math.Max(1, Math.Round(DEFAULT_RANDOM_TOURNAMENT_MATCHUP_REPETITIONS / gameReductionFactor)),
                maxMoveCount = Game.NO_MOVE_LIMIT,
                maxMoveMillis = Game.NO_TIMEOUT,
                autosaveInterval = 5,
                parallelGameCount = 16,
            };
        }

        public const float DEFAULT_PEER_TOURNAMENT_FITNESS_WEIGHT = 1f;
        public const float DEFAULT_RANDOM_TOURNAMENT_FITNESS_WEIGHT = 1f;
        public const float DEFAULT_FITNESS_WINRATE_WEIGHT  = 1.0f;
        public const float DEFAULT_FITNESS_DRAWRATE_WEIGHT = 0.5f;
        public const float DEFAULT_FITNESS_LOSSRATE_WEIGHT = 0.0f;

        public class FitnessWeighting {

            public float peerTournamentWeight { get; set; }
            public float randomTournamentWeight { get; set; }
            public float winrateWeight { get; set; }
            public float drawRateWeight { get; set; }
            public float lossRateWeight { get; set; }

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

        public static FitnessWeighting DefaultFitnessWeighting => new FitnessWeighting() {
            peerTournamentWeight = DEFAULT_PEER_TOURNAMENT_FITNESS_WEIGHT,
            randomTournamentWeight = DEFAULT_RANDOM_TOURNAMENT_FITNESS_WEIGHT,
            winrateWeight = DEFAULT_FITNESS_WINRATE_WEIGHT,
            drawRateWeight = DEFAULT_FITNESS_DRAWRATE_WEIGHT,
            lossRateWeight = DEFAULT_FITNESS_LOSSRATE_WEIGHT,
        };

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

        [JsonIgnore]
        public IReadOnlyList<TIndividual> previousGeneration {
            get {
                if (generations.Count < 2) {
                    return null;
                }
                return generations[generations.Count - 2];
            }
        }

        [JsonIgnore]
        public IReadOnlyList<TIndividual> currentGeneration {
            get {
                return generations[generations.Count - 1];
            }
        }

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
            output.generations = new List<TIndividual[]>();
            output.CreateNextGeneration();
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

        public static BootCamp<TGame, TIndividual> Load (string id) {
            if (TryLoad(id, out var output)) {
                return output;
            }
            return default;
        }

        public TIndividual GetFittestIndividual () {
            List<TIndividual> sourceGeneration;
            if (peerTournamentFinished && randomTournamentFinished) {
                sourceGeneration = new List<TIndividual>(currentGeneration);
            } else {
                sourceGeneration = new List<TIndividual>(previousGeneration);
            }
            sourceGeneration.Sort((a, b) => Math.Sign(b.finalFitness - a.finalFitness));
            return sourceGeneration[0];
        }

        void Save () {
            Log("Saving Progress!");
            var jsonBytes = JsonSerializer.SerializeToUtf8Bytes(this);
            DataSaver.SaveInProject(GetProjectPathForSavedata($"{this.id}_Generation{generations.Count}"), jsonBytes);
            DataSaver.Flush();
        }

        void Log (string msg) {
            Logger.Log($"{nameof(BootCamp)}<{typeof(TGame).Name}, {typeof(TIndividual).Name}>: {msg}");
        }

        void VerifyTournamentConfigPlayersPerGame () {
            var ppg = tournamentConfig.playersPerGame;
            var tempGame = new TGame();
            var minPpg = tempGame.MinimumNumberOfAgentsRequired;
            var maxPpg = tempGame.MaximumNumberOfAgentsAllowed;
            if (ppg < minPpg || ppg > maxPpg) {
                throw new System.NotSupportedException($"Tournament config specifies {ppg} players per game, but the number must be between {minPpg} and {maxPpg}!");
            }
        }

        public void RunUntil (IBootCampTerminationCondition<TGame, TIndividual> terminationCondition) {
            VerifyTournamentConfigPlayersPerGame();
            while (true) {
                Log($"Training Generation {generations.Count - 1}");
                var currentGen = generations[generations.Count - 1];
                var currentAgents = new List<Agent>(currentGen.Select(individual => individual.CreateAgent()));
                if (!peerTournamentFinished) {
                    Log("Starting Peer Tournament");
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
                Log($"Peer Tournament Finished");
                if (!randomTournamentFinished) {
                    Log("Starting Random Tournament");
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
                Log($"Random Tournament Finished");
                if (terminationCondition.EndTraining(this)) {
                    SetCurrentGenerationFitnessAndSort();
                    Save();
                    Log($"Training Finished");
                    break;
                }
                Log($"Creating Next Generation");
                CreateNextGeneration();
                Save();
            }
        }

        public void RecreateCurrentGeneration () {
            generations.RemoveAt(generations.Count - 1);
            CreateNextGeneration();
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

        void SetCurrentGenerationFitnessAndSort () {
            var currGen = generations[generations.Count - 1];
            foreach (var individual in currGen) {
                individual.finalFitness = fitnessWeighting.GetRatingForIndividual(individual);
            }
            var sortedList = new List<TIndividual>(currGen);
            sortedList.Sort((a, b) => Math.Sign(b.finalFitness - a.finalFitness));
            for (int i = 0; i < currGen.Length; i++) {
                currGen[i] = sortedList[i];
            }
        }

        WinLossDrawRecord DoTournament (Tournament<TGame> tournament, IReadOnlyList<Agent> agents, int matchupRepetitons, IMatchupFilter filter) {
            tournament.AgentMoveTimeoutMilliseconds = tournamentConfig.maxMoveMillis;
            tournament.AllowedGameConsoleOutputs = Game.ConsoleOutputs.Nothing;
            tournament.AutosaveIntervalMinutes = tournamentConfig.autosaveInterval;
            tournament.MaxNumberOfGamesToRunInParallel = tournamentConfig.parallelGameCount;
            tournament.MaxNumberOfMovesPerGame = tournamentConfig.maxMoveCount;
            tournament.PlayEachMatchupToCompletionBeforeMovingOntoNext = false;
            tournament.SaveIdPrefix = this.id;
            tournament.LogIdPrefix = $"BC_Gen{generations.Count-1}";
            tournament.onSaved += (id) => latestRecordId = id;
            tournament.Run(agents, matchupRepetitons, filter);
            tournament.SaveWinLossDrawRecord();
            return tournament.GetWinLossDrawRecord();
        }

        void CreateNextGeneration () {
            if (generations.Count < 1) {
                generations.Add(CreateFirstGeneration());
            } else {
                generations.Add(CreateOffspringGeneration());
            }
            latestRecordId = string.Empty;
            peerTournamentFinished = tournamentConfig.peerTournamentMatchupRepetitionCount <= 0;
            randomTournamentFinished = tournamentConfig.randomTournamentMatchupRepetitionCount <= 0;

            TIndividual[] CreateFirstGeneration () {
                var firstGen = new TIndividual[generationConfig.generationSize];
                for (int i = 0; i < firstGen.Length; i++) {
                    firstGen[i] = new TIndividual();
                    firstGen[i].InitializeWithRandomCoefficients();
                    firstGen[i].guid = System.Guid.NewGuid().ToString();
                    firstGen[i].agentId = firstGen[i].CreateAgent().Id;
                }
                return firstGen;
            }

            TIndividual[] CreateOffspringGeneration () {
                var currGen = generations[generations.Count - 1];
                SetCurrentGenerationFitnessAndSort();
                var nextGen = new List<TIndividual>(generationConfig.generationSize);
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
                return nextGen.ToArray();

                TIndividual GetIndividualByPerformance (int index) {
                    return currGen[index];
                }

                void AddIndividualsToNextGen (int count, System.Func<int, Individual> createIndividual) {
                    for (int i = 0; i < count; i++) {
                        var newIndividual = (TIndividual)(createIndividual(i));
                        newIndividual.guid = System.Guid.NewGuid().ToString();
                        newIndividual.agentId = newIndividual.CreateAgent().Id;
                        nextGen.Add(newIndividual);
                    }
                }
            }
        }

    }

}
