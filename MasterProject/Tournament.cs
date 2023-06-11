using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MasterProject.Records;

namespace MasterProject {

    public abstract class Tournament {

        public const int NO_AUTOSAVE = int.MaxValue;

        int _maxNumberOfGamesToRunInParallel = 1;
        public int MaxNumberOfGamesToRunInParallel {
            get => _maxNumberOfGamesToRunInParallel;
            set => _maxNumberOfGamesToRunInParallel = Math.Max(1, value);
        }

        int _maxNumberOfMovesPerGame = Game.NO_MOVE_LIMIT;
        public int MaxNumberOfMovesPerGame {
            get => _maxNumberOfMovesPerGame;
            set => _maxNumberOfMovesPerGame = Math.Max(0, value);
        }

        int _agentMoveTimeoutMilliseconds = Game.NO_TIMEOUT;
        public int AgentMoveTimeoutMilliseconds {
            get => _agentMoveTimeoutMilliseconds;
            set => _agentMoveTimeoutMilliseconds = Math.Max(0, value);
        }

        int _autosaveIntervalMinutes = NO_AUTOSAVE;
        public int AutosaveIntervalMinutes {
            get => _autosaveIntervalMinutes;
            set => _autosaveIntervalMinutes = Math.Max(1, value);
        }

        bool _playEachMatchupToCompletionBeforeMovingOntoNext = false;
        public bool PlayEachMatchupToCompletionBeforeMovingOntoNext {
            get => _playEachMatchupToCompletionBeforeMovingOntoNext;
            set => _playEachMatchupToCompletionBeforeMovingOntoNext = value;
        }

        string _saveIdPrefix = string.Empty;
        public string SaveIdPrefix {
            get => _saveIdPrefix;
            set => _saveIdPrefix = (value ?? string.Empty).Trim();
        }

        public Game.ConsoleOutputs AllowedGameConsoleOutputs { get; set; } = Game.ConsoleOutputs.Nothing;

        public const string ResultsDirectoryName = "TournamentResults";

        public const string ResultsFileExtension = "tournamentResult";

        protected static string GetProjectPathForResult (string id) => $"{ResultsDirectoryName}/{id}.{ResultsFileExtension}";

        public static bool TryLoadWinLossDrawRecord (string id, out WinLossDrawRecord output) {
            if (!string.IsNullOrWhiteSpace(id) && DataLoader.TryLoadInProject(GetProjectPathForResult(id), out var loadedBytes)) {
                output = WinLossDrawRecord.FromJsonBytes(loadedBytes);
                return true;
            }
            output = default;
            return false;
        }

    }

    public class Tournament<TGame> : Tournament where TGame : Game, new() {

        public bool IsFinished { get; private set; } = false;

        public event System.Action<string> onSaved = delegate {};

        private bool hasRun = false;
        private DateTime startTime = default;
        private Agent[] agents = new Agent[0];
        private string[] agentIds = new string[0];
        private WinLossDrawRecord? record;
        private int playersPerGame;
        private string id;
        private int autosaveCounter;

        public WinLossDrawRecord? GetWinLossDrawRecord () {
            if (!IsFinished || record == null) {
                return null;
            }
            var output = WinLossDrawRecord.Merge(record, WinLossDrawRecord.Empty(record.matchupSize));
            output.CalculateElo();
            return output;
        }

        public void SaveWinLossDrawRecord () {
            SaveWinLossDrawRecord(isAutoSave: false);
        }

        private void SaveWinLossDrawRecord (bool isAutoSave) {
            if (!isAutoSave) {
                if (!IsFinished || record == null) {
                    throw new NotImplementedException("Can't save win loss record now!");
                }
                record.CalculateElo();
            }
            var saveId = this.id;
            if (isAutoSave) {
                saveId = $"{saveId}_autosave{autosaveCounter}";
                autosaveCounter++;
            }
            if (!string.IsNullOrWhiteSpace(SaveIdPrefix)) {
                saveId = $"{SaveIdPrefix}_{saveId}";
            }
            DataSaver.SaveInProject(GetProjectPathForResult(saveId), record.ToJsonBytes());
            DataSaver.Flush();
            onSaved(saveId);
        }

        private void VerifyOnlyOneRun () {
            if (hasRun) {
                throw new NotSupportedException($"Running a game multiple times is not allowed!");
            }
        }

        private void VerifyAgents () {
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < agents.Length; i++) {
                if (!agents[i].IsTournamentEligible) {
                    sb.AppendLine($"Agent type {agents[i].GetType()} cannot be used in a tournament!");
                }
                var testClone = agents[i].Clone();
                if (testClone.GetType() != agents[i].GetType()) {
                    sb.AppendLine($"Clone of agent type {agents[i].GetType()} returned type {testClone.GetType()}!");
                }
                var agentId = agentIds[i];
                for (int j = 0; j < agentIds.Length; j++) {
                    if (i != j && agentIds[j] == agentId) {
                        sb.AppendLine($"Agent type {agents[i].GetType()} returned same id as agent type {agents[j].GetType()} ({agentId})!");
                    }
                }
            }
            if (sb.Length > 0) {
                throw new InvalidDataException(sb.ToString());
            }
        }

        private void SetupRecord () {
            var newRecord = WinLossDrawRecord.New(agentIds, playersPerGame);
            if (this.record == null) {
                this.record = newRecord;
            } else {
                this.record = WinLossDrawRecord.Merge(this.record, newRecord);
            }
        }

        struct GameRun {
            public Game game;
            public string[] agentIds;
            public Task runTask;
        }

        private Tournament () { }

        private static string GenerateId () {
            return $"{nameof(Tournament)}_{typeof(TGame).Name}_{System.DateTime.Now.Ticks}";
        }

        public static Tournament<TGame> New (int playersPerGame) {
            var output = new Tournament<TGame>();
            output.id = GenerateId();
            output.playersPerGame = playersPerGame;
            output.record = null;
            return output;
        }

        public static Tournament<TGame> Continue (WinLossDrawRecord existingRecord) {
            var output = new Tournament<TGame>();
            output.id = GenerateId();
            output.playersPerGame = existingRecord.matchupSize;
            output.record = existingRecord;
            return output;
        }

        public static Tournament<TGame> RunSingleAgentTournamentVsRandom (Agent testAgent, int totalGameCountPerMatchup, int parallelGameCount = 16) {
            var tournament = new Tournament<TGame>();
            var tempGame = new TGame();
            tournament.playersPerGame = tempGame.MinimumNumberOfAgentsRequired;
            tournament.record = null;
            tournament.AllowedGameConsoleOutputs = Game.ConsoleOutputs.Nothing;
            tournament.AutosaveIntervalMinutes = NO_AUTOSAVE;
            tournament.MaxNumberOfGamesToRunInParallel = parallelGameCount;
            tournament.Run(
                agentsToUse: new Agent[]{
                    testAgent,
                    tempGame.GetRandomAgent()
                },
                numberOfGamesPerMatchup: totalGameCountPerMatchup,
                matchupFilter: MatchupFilter.EnsureAgentIsContainedOncePerMatchup(testAgent)
            );
            return tournament;
        }

        public void Run (IEnumerable<Agent> agentsToUse, int numberOfGamesPerMatchup, IMatchupFilter matchupFilter) {
            VerifyOnlyOneRun();
            this.agents = agentsToUse.ToArray();
            this.agentIds = new List<string>(agentsToUse.Select((agent) => (agent.Id))).ToArray();
            VerifyAgents();
            SetupRecord();
            hasRun = true;
            startTime = System.DateTime.Now;
            RunRemainingMatches(numberOfGamesPerMatchup, matchupFilter, out var moveLimitReachedCounter, out var otherExceptions);
            IsFinished = true;
            DoEndLogs(moveLimitReachedCounter, otherExceptions);
        }

        int CountNumberOfMatchesRemainingForMatchup (int matchupIndex, int targetRunsPerMatchup, IMatchupFilter matchupFilter) {
            var participantIds = record.GetMatchupFromIndex(matchupIndex);
            if (matchupFilter.PreventMatchup(participantIds)){
                return 0;
            }
            foreach (var id in participantIds) {
                if (this.agentIds.Contains(id)) {
                    return Math.Max(0, targetRunsPerMatchup - record.GetNumberOfMatchesPlayedInMatchup(matchupIndex));
                }
            }
            return 0;
        }

        void RunRemainingMatches (int numberOfGamesPerMatchup, IMatchupFilter matchupFilter, out int moveLimitReachedCounter, out IReadOnlyList<Exception> otherExceptions) {
            var remainingGameCounter = new int[record.GetMatchupCount()];
            var totalGameCount = 0;
            for (int i = 0; i < record.GetMatchupCount(); i++) {
                var remaining = CountNumberOfMatchesRemainingForMatchup(i, numberOfGamesPerMatchup, matchupFilter);
                remainingGameCounter[i] = remaining;
                totalGameCount += remaining;
            }
            var runGameCounter = 0;
            var done = false;   // keeping a separate bool rather than relying on runGameCounter reaching totalGameCount
            var nextAutoSaveTime = System.DateTime.Now + System.TimeSpan.FromMinutes(AutosaveIntervalMinutes);
            moveLimitReachedCounter = 0;
            otherExceptions = new List<Exception>();
            List<GameRun> gameRuns = new();
            while (!done) {
                done = true;
                for (int i = 0; i < record.GetMatchupCount(); i++) {
                    if (remainingGameCounter[i] > 0) {
                        done = false;
                        var matchupAgentIds = record.GetMatchupFromIndex(i);
                        var agents = new Agent[playersPerGame];
                        for (int k = 0; k < agents.Length; k++) {
                            agents[k] = this.agents[Array.IndexOf(agentIds, matchupAgentIds[k])].Clone();
                        }
                        var newGameCount = (PlayEachMatchupToCompletionBeforeMovingOntoNext ? remainingGameCounter[i] : 1);
                        for (int j = 0; j < newGameCount; j++) {
                            var game = new TGame();
                            game.AllowedConsoleOutputs = AllowedGameConsoleOutputs;
                            game.AgentMoveTimeoutMilliseconds = AgentMoveTimeoutMilliseconds;
                            game.MoveLimit = MaxNumberOfMovesPerGame;
                            gameRuns.Add(new GameRun() {
                                game = game,
                                agentIds = matchupAgentIds.ToArray(),
                                runTask = game.RunAsync(agents)
                            });
                            remainingGameCounter[i]--;
                            if (gameRuns.Count >= MaxNumberOfGamesToRunInParallel) {
                                LogTournamentProgress(runGameCounter, gameRuns.Count, totalGameCount);
                                RunCurrentRuns(gameRuns, ref moveLimitReachedCounter, (List<Exception>)otherExceptions);
                                runGameCounter += gameRuns.Count;
                                gameRuns.Clear();
                                if (System.DateTime.Now > nextAutoSaveTime) {
                                    Console.WriteLine(" ---- AUTOSAVING! ----");
                                    SaveWinLossDrawRecord(isAutoSave: true);
                                    Console.WriteLine(" ---- AUTOSAVE DONE ----");
                                    nextAutoSaveTime = System.DateTime.Now + System.TimeSpan.FromMinutes(AutosaveIntervalMinutes);
                                }
                            }
                        }
                    }
                }
            }
            if (gameRuns.Count > 0) {
                LogTournamentProgress(runGameCounter, gameRuns.Count, totalGameCount);
                RunCurrentRuns(gameRuns, ref moveLimitReachedCounter, (List<Exception>)otherExceptions);
                runGameCounter += gameRuns.Count;
                gameRuns.Clear();
            }
        }

        void LogTournamentProgress (int counter, int numGamesThisRound, int total) {
            var outputMessage = $"Tournament at {(100f * counter / total):F1}% running games {counter} to {counter + numGamesThisRound - 1} of {total}";
            var currentTime = System.DateTime.Now;
            var elapsed = currentTime - startTime;
            if (elapsed.TotalSeconds > 1 && counter > 0) {
                var progress = (float)counter / total;
                var estimateDuration = elapsed / progress;
                var estimateRemaining = System.TimeSpan.FromSeconds(Math.Round((estimateDuration - elapsed).TotalSeconds));
                outputMessage = $"{outputMessage} ({FormatTimeSpan(elapsed, true)} elapsed, approx. {FormatTimeSpan(estimateRemaining, true)} remaining)";
            }
            Console.WriteLine(outputMessage);
        }

        void RunCurrentRuns (IEnumerable<GameRun> gameRuns, ref int moveLimitReachedCounter, List<Exception> otherExceptions) {
            foreach (var gameRun in gameRuns) {
                try {
                    gameRun.runTask.GetAwaiter().GetResult();
                    var gs = gameRun.game.GetFinalGameState();
                    record.RecordResult(gameRun.agentIds, gs);
                } catch (System.Exception e) {
                    if (e is Game.MoveLimitReachedException) {
                        var gs = gameRun.game.GetFinalGameState();
                        record.RecordResult(gameRun.agentIds, gs);
                        moveLimitReachedCounter++;
                    } else {
                        otherExceptions.Add(e);
                    }
                }
            }
        }

        void DoEndLogs (int moveLimitReachedCounter, IReadOnlyList<Exception> otherExceptions) {
            var duration = System.DateTime.Now - startTime;
            Console.WriteLine($"Tournament complete! (Took {FormatTimeSpan(duration, false)})");
            if (moveLimitReachedCounter > 0) {
                Console.WriteLine($"{moveLimitReachedCounter} games reached the move limit and were counted as draws!");
            }
            if (otherExceptions.Count > 0) {
                Console.WriteLine($"{otherExceptions.Count} games had other exceptions, listed now:");
                foreach (var otherException in otherExceptions) {
                    Console.WriteLine($"{otherException.GetType()} \"{otherException.Message}\"\n{otherException.StackTrace}");
                }
            }
        }

        static string FormatTimeSpan (TimeSpan timeSpan, bool trimLeadingZeroes) {
            var hours = Math.Floor(timeSpan.TotalHours);
            var minutes = timeSpan.Minutes;
            var seconds = timeSpan.Seconds;
            if (hours > 0 || !trimLeadingZeroes) {
                return $"{hours}h {minutes}m {seconds}s";
            }
            if (minutes > 0 || !trimLeadingZeroes) {
                return $"{minutes}m {seconds}s";
            }
            return $"{seconds}s";
        }

    }

}
