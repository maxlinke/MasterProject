using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterProject {

    public static class AlphaBetaMinMax {

        // careful with infinite evaluations
        // they will always dominate, no matter how unlikely they are
        // they are best used to force a selection, i.e. if a given move directly leads to victory, then infinity would be appropriate
        // otherwise if a victory is rated equally, no matter how far into the gametree it occurs, the earlier best move will be chosen

        public static IReadOnlyList<float> RateMoves<TGameState, TMove> (
            TGameState initialGameState,
            IReadOnlyList<TMove> moves,
            int maxDepth,
            Func<TGameState, int, bool> maximize,
            Func<TGameState, int, float> evaluate
        )
            where TGameState : GameState<TGameState, TMove>
        {
            maxDepth = Math.Max(1, maxDepth);
            var scores = new float[moves.Count];
            for (int i = 0; i < moves.Count; i++) {
                var outcomes = initialGameState.GetPossibleOutcomesForMove(moves[i]);
                scores[i] = 0;
                foreach (var outcome in outcomes) {
                    scores[i] += outcome.Probability * RateGameStateRecursive<TGameState, TMove>(
                        currentState: outcome.GameState,
                        depth: 1,
                        depthRemaining: maxDepth - 1,
                        alpha: float.NegativeInfinity,
                        beta: float.PositiveInfinity,
                        maximize: maximize,
                        evaluate: evaluate
                    );
                }
            }
            return scores;
        }

        // combination of https://en.wikipedia.org/wiki/Alpha%E2%80%93beta_pruning
        // and https://en.wikipedia.org/wiki/Expectiminimax for the probabalistic aspect
        // for deterministic outcomes (probability = 1) it is identical to normal alpha beta pruning
        private static float RateGameStateRecursive<TGameState, TMove> (
            TGameState currentState, 
            int depth, 
            int depthRemaining, 
            float alpha, 
            float beta, 
            Func<TGameState, int, bool> maximize,
            Func<TGameState, int, float> evaluate
        )
            where TGameState : GameState<TGameState, TMove> 
        {
            if (currentState.GameOver || depthRemaining < 1) {
                return evaluate(currentState, depth);
            }
            if (maximize(currentState, depth)) {
                var value = float.NegativeInfinity;
                foreach (var move in currentState.GetPossibleMovesForCurrentPlayer()) {
                    var combinedOutcomes = 0f;
                    foreach (var outcome in currentState.GetPossibleOutcomesForMove(move)) {
                        combinedOutcomes += outcome.Probability * RateGameStateRecursive<TGameState, TMove>(
                            currentState: outcome.GameState,
                            depth: depth + 1,
                            depthRemaining: depthRemaining - 1,
                            alpha: alpha,
                            beta: beta,
                            maximize: maximize,
                            evaluate: evaluate
                        );
                    }
                    value = Math.Max(value, combinedOutcomes);
                    if (value > beta) {
                        break;
                    }
                    alpha = Math.Max(alpha, value);
                }
                return value;
            } else {
                var value = float.PositiveInfinity;
                foreach (var move in currentState.GetPossibleMovesForCurrentPlayer()) {
                    var combinedOutcomes = 0f;
                    foreach (var outcome in currentState.GetPossibleOutcomesForMove(move)) {
                        combinedOutcomes += outcome.Probability * RateGameStateRecursive<TGameState, TMove>(
                            currentState: outcome.GameState,
                            depth: depth + 1,
                            depthRemaining: depthRemaining - 1,
                            alpha: alpha,
                            beta: beta,
                            maximize: maximize,
                            evaluate: evaluate
                        );
                    }
                    value = Math.Min(value, combinedOutcomes);
                    if (value < alpha) {
                        break;
                    }
                    beta = Math.Min(beta, value);
                }
                return value;
            }
        }

	}

}
