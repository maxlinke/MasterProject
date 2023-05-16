﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public static string ResultsDirectoryPath => System.IO.Path.Combine(Program.GetProjectPath(), ResultsDirectoryName);

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
        private List<Agent> agents = new ();
        private List<string> agentIds = new ();
        private WinLossDrawRecord? record;
        private int playersPerGame;

        public WinLossDrawRecord? GetWinLossDrawRecord () {
            if (!IsFinished || record == null) {
                return null;
            }
            return WinLossDrawRecord.Merge(record, WinLossDrawRecord.Empty(record.matchupSize));
        }

        public void SaveWinLossDrawRecord () {
            if (!IsFinished || record == null) {
                throw new NotImplementedException("Can't save win loss record now!");
            }
            var id = $"{typeof(TGame).FullName}_{System.DateTime.Now.Ticks}";
            DataSaver.SaveInProject($"{ResultsDirectoryName}/{id}.json", record.ToJsonBytes());
        }

        private void VerifyOnlyOneRun () {
            if (hasRun) {
                throw new NotSupportedException($"Running a game multiple times is not allowed!");
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

        public void Run (IEnumerable<Agent> agentsToUse, int numberOfGamesPerMatchup) {
            VerifyOnlyOneRun();
            this.agents.AddRange(agentsToUse);
            this.agentIds.AddRange(agentsToUse.Select((agent) => (agent.Id)));
            SetupRecord();
            hasRun = true;
            var startTime = System.DateTime.Now;
            RunRemainingMatches(numberOfGamesPerMatchup, out var moveLimitReachedCounter, out var otherExceptions);
            var endTime = System.DateTime.Now;
            IsFinished = true;
            DoEndLogs(endTime - startTime, moveLimitReachedCounter, otherExceptions);
        }

        int CountNumberOfMatchesToRun (int targetRunsPerMatchup) {
            var output = 0;
            for (int i = 0; i < record.matchupCount; i++) {
                output += CountNumberOfMatchesRemainingForMatchup(i, targetRunsPerMatchup);
            }
            return output;
        }

        int CountNumberOfMatchesRemainingForMatchup (int matchupIndex, int targetRunsPerMatchup) {
            var participantIds = record.GetMatchupFromIndex(matchupIndex);
            foreach (var id in participantIds) {
                if (this.agentIds.Contains(id)) {
                    return Math.Max(0, targetRunsPerMatchup - record.GetNumberOfMatchesPlayedInMatchup(matchupIndex));
                }
            }
            return 0;
        }

        void RunRemainingMatches (int numberOfGamesPerMatchup, out int moveLimitReachedCounter, out IReadOnlyList<Exception> otherExceptions) {
            var totalGameCount = CountNumberOfMatchesToRun(numberOfGamesPerMatchup);
            var runGameCounter = 0;
            moveLimitReachedCounter = 0;
            otherExceptions = new List<Exception>();
            List<GameRun> gameRuns = new();
            for (int i = 0; i < record.matchupCount; i++) {
                var remaining = CountNumberOfMatchesRemainingForMatchup(i, numberOfGamesPerMatchup);
                if (remaining > 0) {
                    var matchupAgentIds = record.GetMatchupFromIndex(i);
                    var agents = new Agent[playersPerGame];
                    for (int k = 0; k < agents.Length; k++) {
                        agents[i] = this.agents.FirstOrDefault((agent) => (agent.Id == matchupAgentIds[k])).Clone();
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
                        if (gameRuns.Count > MaxNumberOfGamesToRunInParallel) {
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
            Console.WriteLine($"Tournament at {((float)counter / total):F1}% running games {counter} to {counter + numGamesThisRound - 1} of {total} ({System.DateTime.Now.ToLongTimeString()})");
        }

        void RunCurrentRuns (IEnumerable<GameRun> gameRuns, ref int moveLimitReachedCounter, List<Exception> otherExceptions) {
            foreach (var gameRun in gameRuns) {
                try {
                    gameRun.runTask.GetAwaiter().GetResult();
                    var gs = gameRun.game.GetFinalGameState();
                    if (gs.IsDraw) {
                        record.RecordDraw(gameRun.agentIds);
                    } else {
                        record.RecordWin(gameRun.agentIds, gs.WinnerIndex);
                    }
                } catch (System.Exception e) {
                    if (e is Game.MoveLimitReachedException) {
                        record.RecordDraw(gameRun.agentIds);
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
