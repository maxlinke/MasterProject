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

        public void RunSynced () {
            RunAsync().GetAwaiter().GetResult();
        }

        public abstract Task RunAsync ();

        public abstract GameRecord GetRecord ();

    }

    public abstract class Game<TGame, TGameState, TMove, TAgent> : Game
        where TGame : Game<TGame, TGameState, TMove, TAgent>
        where TGameState : GameState<TGameState, TMove>
        where TMove : class
        where TAgent : Agent<TGame, TMove>
    {

        protected TGameState? CurrentGameState { get; private set; }

        public TGameState? GetFinalGameState () {
            if (CurrentGameState == null || !CurrentGameState.GameOver) {
                return null;
            }
            return CurrentGameState;
        }

        private bool hasRun = false;
        private readonly List<TAgent> agents = new();
        private readonly List<TGameState> gameStates = new();
        private readonly List<MoveRecord> moveRecords = new();

        protected abstract TGameState GetInitialGameState ();

        protected abstract int MinimumNumberOfAgentsRequired { get; }

        protected abstract int MaximumNumberOfAgentsAllowed { get; }

        private int _moveLimit = int.MaxValue;
        public int MoveLimit { get => _moveLimit; set => _moveLimit = Math.Max(value, 0); }

        private int _agentMoveTimeoutMilliseconds = int.MaxValue;
        public int AgentMoveTimeoutMilliseconds { get => _agentMoveTimeoutMilliseconds; set => _agentMoveTimeoutMilliseconds = Math.Max(value, 0); }

        public void SetAgents (IEnumerable<TAgent> agentsToSet) {
            if (agents.Count > 0) {
                throw new NotSupportedException("Agents have already been set!");
            }
            agents.AddRange(agentsToSet);
        }

        public TGameState GetCurrentGameStateVisibleForAgent (TAgent agent) {
            return CurrentGameState.GetVisibleGameStateForPlayer(agents.IndexOf(agent));
        }

        private void VerifyOnlyOneRun () {
            if (hasRun) {
                throw new NotSupportedException($"Running a game multiple times is not allowed!");
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

        // https://devblogs.microsoft.com/pfxteam/crafting-a-task-timeoutafter-method/

        private async Task<int> GetAgentMoveIndex (TAgent agent, IReadOnlyList<TMove> moves) {
            return await Task.Run(() => agent.GetMoveIndex((TGame)this, moves));
        }

        private async Task<int> PerformAgentTimeoutDelay () {
            await Task.Delay(AgentMoveTimeoutMilliseconds);
            return default;
        }

        public override async Task RunAsync () {
            VerifyOnlyOneRun();
            VerifyNumberOfAgents();
            CurrentGameState = GetInitialGameState();
            foreach (var agent in agents) {
                agent.OnGameStarted((TGame)this);
            }
            var rng = new Random();
            var sw = new Stopwatch();
            var moveCounter = 0;
            hasRun = true;
            while (!CurrentGameState.GameOver) {
                if (moveCounter >= MoveLimit) {
                    throw new Exception($"Move limit reached after {moveCounter} moves!");
                }
                TryLog(ConsoleOutputs.Move, $"Move {moveCounter}");
                var currentAgent = agents[CurrentGameState.CurrentPlayerIndex];
                TryLog(ConsoleOutputs.Move, $"Turn of player {CurrentGameState.CurrentPlayerIndex} ({currentAgent.GetType()})");
                var moves = CurrentGameState.GetPossibleMovesForCurrentPlayer();
                int moveIndex;
                bool moveTimeout; 
                if (moves.Count > 1) {
                    sw.Restart();
                    if (AgentMoveTimeoutMilliseconds < int.MaxValue) {
                        var agentTask = GetAgentMoveIndex(currentAgent, moves);
                        var timeoutTask = PerformAgentTimeoutDelay();
                        var resultTask = await Task.WhenAny(agentTask, timeoutTask);
                        if (resultTask == agentTask) {
                            moveIndex = agentTask.Result;
                            moveTimeout = false;
                        } else {
                            moveIndex = Agent.GetRandomMoveIndex(moves);
                            moveTimeout = true;
                        }
                    } else {
                        moveIndex = currentAgent.GetMoveIndex((TGame)this, moves);
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
                CurrentGameState = possibleOutcomes[newGameStateIndex].Outcome;
                moveCounter++;
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
