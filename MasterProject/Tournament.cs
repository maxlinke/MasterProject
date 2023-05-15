using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterProject {

    public class Tournament<TGame> where TGame : Game, new() {

        // options
        // what agents
        // what matchups
        // how many matches total/per matchup
        // recording gamerecords (optional)
        // recording win/loss/draw-matrix (NOT optional)

        int _maxNumberOfGamesToRunInParallel = 1;
        public int MaxNumberOfGamesToRunInParallel {
            get => _maxNumberOfGamesToRunInParallel;
            set => _maxNumberOfGamesToRunInParallel = Math.Max(1, value);
        }

        public int _maxNumberOfMovesPerGame = Game.NO_MOVE_LIMIT;
        public int MaxNumberOfMovesPerGame {
            get => _maxNumberOfMovesPerGame;
            set => _maxNumberOfMovesPerGame = Math.Max(0, value);
        }

        public int _agentMoveTimeoutMilliseconds = Game.NO_TIMEOUT;
        public int AgentMoveTimeoutMilliseconds {
            get => _agentMoveTimeoutMilliseconds;
            set => _agentMoveTimeoutMilliseconds = Math.Max(0, value);
        }

        public Game.ConsoleOutputs AllowedGameConsoleOutputs { get; set; } = Game.ConsoleOutputs.Nothing;

        public bool IsFinished { get; private set; } = false;

        private bool hasRun = false;
        private List<Agent> agents = new ();
        private WinLossDrawRecord record;

        public WinLossDrawRecord GetWinLossDrawRecord () {
            if (!IsFinished) {
                return new WinLossDrawRecord(new List<string>(agents.Select(agent => agent.Id)), 0);
            }
            return WinLossDrawRecord.Merge(this.record, new WinLossDrawRecord(new string[0], this.record.matchupSize));
        }

        private void VerifyOnlyOneRun () {
            if (hasRun) {
                throw new NotSupportedException($"Running a game multiple times is not allowed!");
            }
        }

        struct GameRun {
            public Game game;
            public string[] agentIds;
            public Task runTask;
        }

        public void Run (IEnumerable<Agent> agentsToUse, int playersPerGame, int numberOfGamesPerMatchup) {
            VerifyOnlyOneRun();
            this.agents.AddRange(agentsToUse);
            var agentIds = new List<string>(agentsToUse.Select((agent) => (agent.Id)));
            this.record = new WinLossDrawRecord(agentIds, playersPerGame);
            var totalGameCount = record.matchupCount * numberOfGamesPerMatchup;
            hasRun = true;
            var startTime = System.DateTime.Now;
            int moveLimitReachedCounter = 0;
            List<Exception> otherExceptions = new();
            List<GameRun> gameRuns = new();
            for (int i = 0; i < totalGameCount;) {
                gameRuns.Clear();
                var gamesThisRound = Math.Min(MaxNumberOfGamesToRunInParallel, totalGameCount - i);
                Console.WriteLine($"Tournament at {((float)i / totalGameCount):F1}% running games {i} to {i + gamesThisRound - 1} of {totalGameCount} ({System.DateTime.Now.ToLongTimeString()})");
                for (int j = 0; j < gamesThisRound; j++) {
                    var matchupIndex = i / numberOfGamesPerMatchup;
                    var matchupAgentIds = record.GetMatchupFromIndex(matchupIndex);
                    var agents = new Agent[playersPerGame];
                    for (int k = 0; k < agents.Length; k++) {
                        agents[i] = this.agents.FirstOrDefault((agent) => (agent.Id == matchupAgentIds[k])).Clone();
                    }
                    var game = new TGame();
                    game.AllowedConsoleOutputs = AllowedGameConsoleOutputs;
                    game.AgentMoveTimeoutMilliseconds = AgentMoveTimeoutMilliseconds;
                    game.MoveLimit = MaxNumberOfMovesPerGame;
                    gameRuns.Add(new GameRun() {
                        game = game,
                        agentIds = matchupAgentIds.ToArray(),
                        runTask = game.RunAsync(agents)
                    });
                    i++;
                }
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
            IsFinished = true;
            var endTime = System.DateTime.Now;
            var duration = endTime - startTime;
            Console.WriteLine($"Tournament complete! (Took {duration.Hours}h {duration.Minutes}m {duration.Seconds}s)");
            if (moveLimitReachedCounter > 0) {
                Console.WriteLine($"{moveLimitReachedCounter} of {totalGameCount} games reached the move limit and were counted as draws!");
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
