using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterProject {

    public abstract class Agent {

        public abstract bool IsStateless { get; }

        public abstract bool IsTournamentEligible { get; }

        public virtual string Id => $"{this.GetType().FullName}";

        private static readonly Random globalRng = new();

        public static int GetRandomMoveIndex<TMove> (IReadOnlyList<TMove> moves) {
            return globalRng.Next(moves.Count);
        }

        public static int GetIndexOfMaximum (IReadOnlyList<float> values, bool randomTieBreaker) {
            var output = 0;
            var maxVal = float.NegativeInfinity;
            var maxValOccurrences = 0;
            for (int i = 0; i < values.Count; i++) {
                if (values[i] > maxVal) {
                    maxVal = values[i];
                    maxValOccurrences = 0;
                    output = i;
                }else if (values[i] == maxVal) {
                    maxValOccurrences++;
                }
            }
            if (maxValOccurrences > 1 && randomTieBreaker) {
                output = FindIndexOfValueWithRandomTieBreaker(values, maxVal, maxValOccurrences);
            }
            return output;
        }

        public static int GetIndexOfMinimum (IReadOnlyList<float> values, bool randomTieBreaker) {
            var output = 0;
            var minVal = float.PositiveInfinity;
            var minValOccurrences = 0;
            for (int i = 0; i < values.Count; i++) {
                if (values[i] < minVal) {
                    minVal = values[i];
                    minValOccurrences = 0;
                    output = i;
                } else if (values[i] == minVal) {
                    minValOccurrences++;
                }
            }
            if (minValOccurrences > 1 && randomTieBreaker) {
                output = FindIndexOfValueWithRandomTieBreaker(values, minVal, minValOccurrences);
            }
            return output;
        }

        private static int FindIndexOfValueWithRandomTieBreaker (IReadOnlyList<float> values, float targetValue, int numOccurrences) {
            var countdown = globalRng.Next(numOccurrences);
            for (int i = 0; i < values.Count; i++) {
                if (values[i] == targetValue) {
                    countdown--;
                    if (countdown < 0) {
                        return i;
                    }
                }
            }
            return -1;
        }

        public abstract Agent Clone ();

    }

    public abstract class Agent<TGame, TGameState, TMove> : Agent 
        where TGame : Game
        where TGameState : GameState<TGameState, TMove>
    {

        protected readonly Random rng = new();

        protected TGame currentGame { get; private set; }

        public virtual void OnGameStarted (TGame game) {
            currentGame = game;
        }

        public abstract int GetMoveIndex (TGameState gameState, IReadOnlyList<TMove> moves);

    }

}
