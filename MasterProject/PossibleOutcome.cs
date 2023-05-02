using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterProject {

    public class PossibleOutcome<T> {

        public float Probability { get; set; }

        public T? Outcome { get; set; }

    }

    public static class PossibleOutcomeExtensions {

        public static void NormalizeProbabilities<T> (this IEnumerable<PossibleOutcome<T>> possibleOutcomes) {
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
