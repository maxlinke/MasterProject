using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MasterProject.Records;

namespace MasterProject {

    public abstract class Tournament {

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

        // options
        // what agents
        // what matchups
        // how many matches total/per matchup
        // recording gamerecords (optional)
        // recording win/loss/draw-matrix (NOT optional)
        public bool IsFinished { get; private set; } = false;

        private bool hasRun = false;
        private Agent[] agents = new Agent[0];
        private string[] agentIds = new string[0];
        private WinLossDrawRecord? record;
        private int playersPerGame;

        public WinLossDrawRecord? GetWinLossDrawRecord () {
            if (!IsFinished || record == null) {
                return null;
            }
            var output = WinLossDrawRecord.Merge(record, WinLossDrawRecord.Empty(record.matchupSize));
            output.CalculateElo();
            return output;
        }

        public void SaveWinLossDrawRecord () {
            if (!IsFinished || record == null) {
                throw new NotImplementedException("Can't save win loss record now!");
            }
            record.CalculateElo();
            var id = $"{typeof(TGame).FullName}_{System.DateTime.Now.Ticks}";
            DataSaver.SaveInProject(GetProjectPathForResult(id), record.ToJsonBytes());
        }

        private void VerifyOnlyOneRun () {
            if (hasRun) {
                throw new NotSupportedException($"Running a game multiple times is not allowed!");
            }
        }

        private void VerifyAgents () {
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < agents.Length; i++) {
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

        public static Tournament<TGame> New (int playersPerGame) {
            var output = new Tournament<TGame>();
            output.playersPerGame = playersPerGame;
            output.record = null;
            return output;
        }

        public static Tournament<TGame> Continue (WinLossDrawRecord existingRecord) {
            var output = new Tournament<TGame>();
            output.playersPerGame = existingRecord.matchupSize;
            output.record = existingRecord;
            return output;
        }

        public void Run (IEnumerable<Agent> agentsToUse, int numberOfGamesPerMatchup, bool playMirrorMatches) {
            VerifyOnlyOneRun();
            this.agents = agentsToUse.ToArray();
            this.agentIds = new List<string>(agentsToUse.Select((agent) => (agent.Id))).ToArray();
            VerifyAgents();
            SetupRecord();
            hasRun = true;
            var startTime = System.DateTime.Now;
            RunRemainingMatches(numberOfGamesPerMatchup, playMirrorMatches, out var moveLimitReachedCounter, out var otherExceptions);
            var endTime = System.DateTime.Now;
            IsFinished = true;
            DoEndLogs(endTime - startTime, moveLimitReachedCounter, otherExceptions);
        }

        int CountNumberOfMatchesToRun (int targetRunsPerMatchup, bool playMirrorMatches) {
            var output = 0;
            for (int i = 0; i < record.GetMatchupCount(); i++) {
                output += CountNumberOfMatchesRemainingForMatchup(i, targetRunsPerMatchup, playMirrorMatches);
            }
            return output;
        }

        int CountNumberOfMatchesRemainingForMatchup (int matchupIndex, int targetRunsPerMatchup, bool playMirrorMatches) {
            var participantIds = record.GetMatchupFromIndex(matchupIndex);
            if (!playMirrorMatches) {
                var allIdentical = true;
                for (int i = 1; i < participantIds.Count; i++) {
                    allIdentical &= (participantIds[i - 1] == participantIds[i]);
                }
                if (allIdentical) {
                    return 0;
                }
            }
            foreach (var id in participantIds) {
                if (this.agentIds.Contains(id)) {
                    return Math.Max(0, targetRunsPerMatchup - record.GetNumberOfMatchesPlayedInMatchup(matchupIndex));
                }
            }
            return 0;
        }

        void RunRemainingMatches (int numberOfGamesPerMatchup, bool playMirrorMatches, out int moveLimitReachedCounter, out IReadOnlyList<Exception> otherExceptions) {
            var totalGameCount = CountNumberOfMatchesToRun(numberOfGamesPerMatchup, playMirrorMatches);
            var runGameCounter = 0;
            moveLimitReachedCounter = 0;
            otherExceptions = new List<Exception>();
            List<GameRun> gameRuns = new();
            for (int i = 0; i < record.GetMatchupCount(); i++) {
                var remaining = CountNumberOfMatchesRemainingForMatchup(i, numberOfGamesPerMatchup, playMirrorMatches);
                if (remaining > 0) {
                    var matchupAgentIds = record.GetMatchupFromIndex(i);
                    var agents = new Agent[playersPerGame];
                    for (int k = 0; k < agents.Length; k++) {
                        agents[k] = this.agents[Array.IndexOf(agentIds, matchupAgentIds[k])].Clone();
                    }
                    for (int j = 0; j < remaining; j++) {
                        var game = new TGame();
                        game.AllowedConsoleOutputs = AllowedGameConsoleOutputs;
                        game.AgentMoveTimeoutMilliseconds = AgentMoveTimeoutMilliseconds;
                        game.MoveLimit = MaxNumberOfMovesPerGame;
                        gameRuns.Add(new GameRun() {
                            game = game,
                            agentIds = matchupAgentIds.ToArray(),
                            runTask = game.RunAsync(agents)
                        });
                        if (gameRuns.Count >= MaxNumberOfGamesToRunInParallel) {
                            LogTournamentProgress(runGameCounter, gameRuns.Count, totalGameCount);
                            RunCurrentRuns(gameRuns, ref moveLimitReachedCounter, (List<Exception>)otherExceptions);
                            runGameCounter += gameRuns.Count;
                            gameRuns.Clear();
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
            Console.WriteLine($"Tournament at {(100f * counter / total):F1}% running games {counter} to {counter + numGamesThisRound - 1} of {total} ({System.DateTime.Now.ToLongTimeString()})");
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

        void DoEndLogs (System.TimeSpan duration, int moveLimitReachedCounter, IReadOnlyList<Exception> otherExceptions) {
            Console.WriteLine($"Tournament complete! (Took {Math.Floor(duration.TotalHours)}h {duration.Minutes}m {duration.Seconds}s)");
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

    }

}
