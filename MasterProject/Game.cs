using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using System.Diagnostics;

namespace MasterProject {

    public abstract class Game {

        [Flags]
        public enum ConsoleOutputs {
            Nothing = 0,
            GameOver = 1,
            Move = 2,
            Debug = 4,
            Everything = -1
        }

        public Game () {
            this.Guid = System.Guid.NewGuid().ToString();
        }

        protected string HumanReadableId => $"{this.GetType().FullName} {Guid}";

        protected string Guid { get; private set; }

        public ConsoleOutputs AllowedConsoleOutputs { get; set; } = ConsoleOutputs.Nothing;

        protected void TryLog (ConsoleOutputs logLevel, object message) {
            if ((this.AllowedConsoleOutputs & logLevel) == logLevel) {
                Console.WriteLine($"{this.HumanReadableId}: {message}");
            }
        }

        public void TryDebugLog (object message) {
            TryLog(ConsoleOutputs.Debug, message);
        }

        public void RunSynced (IEnumerable<Agent> agents) {
            Run(agents).GetAwaiter().GetResult();
        }

        public async Task RunAsync (IEnumerable<Agent> agents) {
            await Task.Run(() => Run(agents));  // because if there's no timeout set, the run-task will actually not run asynchronously so this forces it to do so
        }

        public const int NO_MOVE_LIMIT = int.MaxValue;
        public const int NO_TIMEOUT = int.MaxValue;

        private int _moveLimit = NO_MOVE_LIMIT;
        public int MoveLimit { get => _moveLimit; set => _moveLimit = Math.Max(value, 0); }

        private int _agentMoveTimeoutMilliseconds = NO_TIMEOUT;
        public int AgentMoveTimeoutMilliseconds { get => _agentMoveTimeoutMilliseconds; set => _agentMoveTimeoutMilliseconds = Math.Max(value, 0); }

        protected abstract Task Run (IEnumerable<Agent> agents);

        public abstract GameRecord GetRecord ();

        public abstract GameState GetFinalGameState ();

        public class MoveLimitReachedException : System.Exception { }

    }

    public abstract class Game<TGame, TGameState, TMove, TAgent> : Game
        where TGame : Game<TGame, TGameState, TMove, TAgent>
        where TGameState : GameState<TGameState, TMove>
        where TMove : class
        where TAgent : Agent<TGame, TGameState, TMove>
    {

        protected TGameState? CurrentGameState { get; private set; }

        public override GameState GetFinalGameState () {
            if (CurrentGameState == null || !CurrentGameState.GameOver) {
                return null;
            }
            return CurrentGameState;
        }

        private bool hasRun = false;
        private readonly List<TAgent> agents = new();
        private readonly List<TGameState> gameStates = new();
        private readonly List<MoveRecord> moveRecords = new();

        public int moveCounter => moveRecords.Count;

        protected abstract TGameState GetInitialGameState ();

        protected abstract int MinimumNumberOfAgentsRequired { get; }

        protected abstract int MaximumNumberOfAgentsAllowed { get; }

        public TGameState GetCurrentGameStateVisibleForAgent (TAgent agent) {
            return CurrentGameState.GetVisibleGameStateForPlayer(agents.IndexOf(agent));
        }

        private void VerifyOnlyOneRun () {
            if (hasRun) {
                throw new NotSupportedException($"Running a game multiple times is not allowed!");
            }
        }

        private void TrySetAgents (IEnumerable<Agent> agentsToUse) {
            foreach (var agent in agentsToUse) {
                if (!(agent is TAgent tAgent)) {
                    throw new ArgumentException($"Agent {agent.Id} ({agent.GetType().FullName}) is not a {typeof(TAgent).FullName}!");
                }
                this.agents.Add(tAgent);
            }
        }

        private void VerifyNumberOfAgents () {
            if (agents.Count < MinimumNumberOfAgentsRequired) {
                throw new NotSupportedException($"{MinimumNumberOfAgentsRequired} Agents required, but got only {agents.Count}!");
            }
            if (agents.Count > MaximumNumberOfAgentsAllowed) {
                throw new NotSupportedException($"{agents.Count} Agents are set, but the maximum allowed is {MaximumNumberOfAgentsAllowed}!");
            }
        }

        protected override async Task Run (IEnumerable<Agent> agentsTouse) {
            VerifyOnlyOneRun();
            TrySetAgents(agentsTouse);
            VerifyNumberOfAgents();
            CurrentGameState = GetInitialGameState();
            var rng = new Random();
            var sw = new Stopwatch();
            hasRun = true;
            foreach (var agent in agents) {
                agent.OnGameStarted((TGame)this);
            }
            while (!CurrentGameState.GameOver) {
                if (moveCounter >= MoveLimit) {
                    throw new MoveLimitReachedException();
                }
                TryLog(ConsoleOutputs.Move, $"Move {moveCounter}");
                var currentAgent = agents[CurrentGameState.CurrentPlayerIndex];
                TryLog(ConsoleOutputs.Move, $"Turn of player {CurrentGameState.CurrentPlayerIndex} ({currentAgent.GetType()})");
                var moves = CurrentGameState.GetPossibleMovesForCurrentPlayer();
                int moveIndex;
                bool moveTimeout; 
                if (moves.Count > 1) {
                    var agentVisibleState = CurrentGameState.GetVisibleGameStateForPlayer(CurrentGameState.CurrentPlayerIndex);
                    sw.Restart();
                    if (AgentMoveTimeoutMilliseconds < int.MaxValue) {
                        var agentTask = Task.Run(() => currentAgent.GetMoveIndex(agentVisibleState, moves));
                        var timeoutTask = Task.Delay(AgentMoveTimeoutMilliseconds);
                        var resultTask = await Task.WhenAny(agentTask, timeoutTask);
                        if (resultTask == agentTask) {
                            moveIndex = agentTask.Result;
                            moveTimeout = false;
                        } else {
                            moveIndex = Agent.GetRandomMoveIndex(moves);
                            moveTimeout = true;
                        }
                    } else {
                        moveIndex = currentAgent.GetMoveIndex(agentVisibleState, moves);
                        moveTimeout = false;
                    }
                    sw.Stop();
                } else {
                    sw.Reset();
                    moveIndex = 0;
                    moveTimeout = false;
                }
                if (moveTimeout) {
                    TryLog(ConsoleOutputs.Move, $"Agent {currentAgent.Id} timed out after {sw.ElapsedMilliseconds}ms");
                }
                if (moveIndex < 0 || moveIndex >= moves.Count) {
                    if (moves.Count < 1) {
                        throw new NotSupportedException("A gamestate must provide at least one move!");
                    } else {
                        throw new NotSupportedException($"Invalid move index \"{moveIndex}\" chosen by agent \"{currentAgent.Id}\"!");
                    }
                }
                var possibleOutcomes = CurrentGameState.GetPossibleOutcomesForMove(moves[moveIndex]);
                var p = rng.NextDouble();
                var newGameStateIndex = -1;
                for (int i = 0; i < possibleOutcomes.Count; i++) {
                    p -= possibleOutcomes[i].Probability;
                    newGameStateIndex = i;
                    if (p <= 0) {
                        break;
                    }
                }
                moveRecords.Add(new MoveRecord() {
                    AvailableMoves = moves.ToArray(),
                    ChosenMoveIndex = moveIndex,
                    MoveChoiceDurationMillis = sw.ElapsedMilliseconds,
                    MoveChoiceTimedOut = moveTimeout
                });
                gameStates.Add(CurrentGameState);
                CurrentGameState = possibleOutcomes[newGameStateIndex].GameState;
            }
            gameStates.Add(CurrentGameState);
            TryLog(ConsoleOutputs.GameOver, "Game Over");
        }

        public override GameRecord GetRecord () {
            return new GameRecord() {
                GameType = this.GetType().FullName,
                GameId = this.Guid,
                Completed = this.CurrentGameState?.GameOver,
                Timestamp = DateTime.Now.Ticks,
                AgentIds = new List<string>(this.agents.Select(agent => agent.Id)).ToArray(),
                GameStates = this.gameStates.ToArray(),
                Moves = this.moveRecords.ToArray()
            };
        }

    }

}
