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

        public abstract void Run ();

        public abstract GameRecord GetRecord ();

    }

    public abstract class Game<TGame, TGameState, TMove, TAgent> : Game
        where TGame : Game<TGame, TGameState, TMove, TAgent>
        where TGameState : GameState<TGameState, TMove>
        where TAgent : Agent<TGame, TMove>
    {

        protected TGameState? CurrentGameState { get; private set; }

        private bool hasRun = false;
        private readonly List<TAgent> agents = new();
        private readonly List<TGameState> gameStates = new();
        private readonly List<int> moveDurationsMillis = new();

        protected abstract TGameState GetInitialGameState ();

        protected abstract int MinimumNumberOfAgentsRequired { get; }

        protected abstract int MaximumNumberOfAgentsAllowed { get; }

        private int _moveLimit = int.MaxValue;
        public int MoveLimit { get => _moveLimit; set => _moveLimit = Math.Max(value, 0); }

        private int _agentMoveTimeoutSeconds = int.MaxValue;
        public int AgentMoveTimeoutSeconds { get => _agentMoveTimeoutSeconds; set => _agentMoveTimeoutSeconds = Math.Max(value, 0); }

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

        public override void Run () {
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
                TryLog(ConsoleOutputs.Move, $"Move {gameStates.Count}");
                var moves = CurrentGameState.GetPossibleMovesForCurrentPlayer();
                sw.Restart();
                // TODO await this with a timeout
                var moveIndex = agents[CurrentGameState.CurrentPlayerIndex].GetMoveIndex(moves);
                sw.Stop();
                moveDurationsMillis.Add((int)sw.ElapsedMilliseconds);
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
                Timestamp = DateTime.Now.Ticks,
                AgentIds = new List<string>(this.agents.Select(agent => agent.Id)).ToArray(),
                GameStates = this.gameStates.ToArray(),
                MoveDurationsMillis = this.moveDurationsMillis.ToArray()
            };
        }

    }

}
