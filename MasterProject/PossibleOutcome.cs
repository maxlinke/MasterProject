using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterProject {

    public class PossibleOutcome<TGameState> where TGameState : GameState {

        public float Probability { get; set; }

        public TGameState GameState { get; set; }

        public static PossibleOutcome<TGameState>[] CertainOutcome (TGameState gs) {
            return new PossibleOutcome<TGameState>[]{
                new PossibleOutcome<TGameState>(){
                    Probability = 1,
                    GameState = gs
                }
            };
        }

    }

    public static class PossibleOutcomeExtensions {

        public static void NormalizeProbabilities<TGameState> (this IEnumerable<PossibleOutcome<TGameState>> possibleOutcomes) where TGameState : GameState {
            var sum = 0f;
            foreach (var possibleOutcome in possibleOutcomes) {
                sum += Math.Max(0, possibleOutcome.Probability);
            }
            if (sum > 0) {
                foreach (var possibleOutcome in possibleOutcomes) {
                    possibleOutcome.Probability = Math.Max(0, possibleOutcome.Probability / sum);
                }
            }
        }

    }

}
