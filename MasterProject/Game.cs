using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace MasterProject {

    public abstract class Game {

        [System.Flags]
        public enum ConsoleOutputs {
            Nothing  = 0,
            GameOver = 1,
            Move     = 2,
            Debug    = 4,
            Everything = -1
        }

        protected string HumanReadableId => $"{this.GetType().FullName} TODO guid";

        // TODO guid

        public ConsoleOutputs AllowedConsoleOutputs { get; set; } = ConsoleOutputs.Nothing;

        public abstract void Run ();

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

        [JsonIgnore]
        public IReadOnlyList<TAgent>? Agents { get; set; }  // TODO but DO serialize the agents' ids, so this probably needs to become a get { ... } set { ... } property

        [JsonIgnore]
        public TGameState? CurrentGameState { get; protected set; }

        [JsonInclude]
        private List<TGameState>? GameStates;

        protected abstract TGameState GetInitialGameState ();

        protected abstract int MinimumNumberOfAgentsRequired { get; }

        // agent timeout parameter? 
        public override void Run () {
            if (Agents == null) {
                throw new ArgumentException("Agents aren't set yet!");
            }
            if(Agents.Count < MinimumNumberOfAgentsRequired) {
                throw new NotSupportedException($"{MinimumNumberOfAgentsRequired} Agents required, but got only {Agents.Count}!");
            }
            var rng = new Random();
            CurrentGameState = GetInitialGameState();
            GameStates = new List<TGameState>();
            foreach (var agent in Agents) {
                agent.OnGameStarted((TGame)this);
            }
            while (!CurrentGameState.GameOver) {
                TryLog(ConsoleOutputs.Move, $"Move {GameStates.Count}");
                var moves = CurrentGameState.GetPossibleMovesForCurrentPlayer();
                // TODO await this with a timeout
                var moveIndex = Agents[CurrentGameState.CurrentPlayerIndex].GetMoveIndex(moves);
                var possibleOutcomes = CurrentGameState.GetPossibleOutcomesForMove(moves[moveIndex]);
                var p = rng.NextDouble();
                var newGameStateIndex = -1;
                for (int i = 0; i < possibleOutcomes.Count; i++) {
                    p -= possibleOutcomes[i].Probability;   // TODO this works, right?
                    if (p <= 0) {
                        newGameStateIndex = i;
                        break;
                    }
                }
                GameStates.Add(CurrentGameState);
                CurrentGameState = possibleOutcomes[newGameStateIndex].Outcome;
            }
            GameStates.Add(CurrentGameState);
            TryLog(ConsoleOutputs.GameOver, "Game Over");
        }

    }

}
