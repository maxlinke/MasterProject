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
            Nothing  = 0,
            GameOver = 1,
            Move     = 2,
            Debug    = 4,
            Everything = -1
        }

        public Game () {
            this.Guid = System.Guid.NewGuid().ToString();
        }

        protected string HumanReadableId => $"{this.GetType().FullName} {Guid}";

        protected string Guid { get; private set; }

        public ConsoleOutputs AllowedConsoleOutputs { get; set; } = ConsoleOutputs.Nothing;

        public abstract void Run ();

        public abstract GameRecord GetRecord ();

        protected void TryLog (ConsoleOutputs logLevel, object message) {
            if ((this.AllowedConsoleOutputs & logLevel) == logLevel) {
                Console.WriteLine($"{this.HumanReadableId}: {message}");
            }
        }

        public void TryDebugLog (object message) {
            TryLog(ConsoleOutputs.Debug, message);
        }

    }

    public abstract class Game<TGame, TGameState, TPlayerState, TMove, TAgent> : Game
        where TGame : Game<TGame, TGameState, TPlayerState, TMove, TAgent>
        where TGameState : GameState<TGameState, TPlayerState, TMove>
        where TPlayerState : PlayerState
        where TMove : Move<TGameState, TPlayerState, TMove>
        where TAgent : Agent<TGame, TGameState, TPlayerState, TMove, TAgent>
    {

        public TGameState? CurrentGameState { get; private set; }

        private readonly List<TAgent> agents = new();
        private readonly List<TGameState> gameStates = new();
        private readonly List<int> moveDurationsMillis = new();

        protected abstract TGameState GetInitialGameState ();

        protected abstract int MinimumNumberOfAgentsRequired { get; }

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

        public override void Run () {
            if(agents.Count < MinimumNumberOfAgentsRequired) {
                throw new NotSupportedException($"{MinimumNumberOfAgentsRequired} Agents required, but got only {agents.Count}!");
            }
            var rng = new Random();
            var sw = new Stopwatch();
            CurrentGameState = GetInitialGameState();
            foreach (var agent in agents) {
                agent.OnGameStarted((TGame)this);
            }
            while (!CurrentGameState.GameOver && MoveLimit > 0) {
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
                MoveLimit--;
            }
            if (MoveLimit <= 0) {
                throw new Exception($"Move limit reached after {gameStates.Count} moves!");
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
