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

        // TODO i don't like the float, float, bool func
        // technically i could probably do this by just messing with the evaluation function?
        // start off with a best score of -infinity always
        // 

        private static readonly Random tiebreakerRng = new();

        public static int SelectMove<TGameState, TMove> (
            TGameState initialGameState,
            IReadOnlyList<TMove> moves,
            int maxDepth,
            Func<TGameState, int, bool> maximize,
            Func<TGameState, int, float> evaluate,
            bool randomTieBreaker
        )
            where TGameState : GameState<TGameState, TMove>
        {
            maxDepth = Math.Max(1, maxDepth);
            var scores = new float[moves.Count];
            var maxScore = float.NegativeInfinity;
            var maxScoreCounter = 0;
            var outputIndex = 0;
            for (int i = 0; i < moves.Count; i++) {
                var outcomes = initialGameState.GetPossibleOutcomesForMove(moves[i]);
                scores[i] = 0;
                foreach (var outcome in outcomes) {
                    scores[i] += outcome.Probability * RateGameStateRecursive<TGameState, TMove>(
                        currentState: outcome.GameState,
                        maxDepth: maxDepth - 1,
                        maximize: maximize,
                        evaluate: evaluate
                    );
                }
                if (scores[i] > maxScore) {
                    maxScore = scores[i];
                    maxScoreCounter = 1;
                    outputIndex = i;
                } else if (scores[i] == maxScore) {
                    maxScoreCounter++;
                }
            }
            if (maxScoreCounter > 1 && randomTieBreaker) {
                var tiebreakerCountdown = tiebreakerRng.Next(maxScoreCounter);
                for (int i = 0; i < scores.Length; i++) {
                    if (scores[i] == maxScore) {
                        tiebreakerCountdown--;
                        if (tiebreakerCountdown <= 0) {
                            outputIndex = i;
                            break;
                        }
                    }
                }
            }
            return outputIndex;
        }

        public static int GetBestMoveIndex<TGameState, TMove> (
            TGameState gameState, 
            IReadOnlyList<TMove> moves, 
            int maxDepth, 
            Func<TGameState, int, float> evaluate,
            bool randomTieBreaker
        )
            where TGameState : GameState<TGameState, TMove> 
        {
            var ownPlayerIndex = gameState.CurrentPlayerIndex;
            return SelectMove(
                initialGameState: gameState,
                moves: moves,
                maxDepth: maxDepth,
                maximize: (gs, _) => (gs.CurrentPlayerIndex == ownPlayerIndex),
                evaluate: evaluate,
                randomTieBreaker: randomTieBreaker
            );
        }

        // for alpha-beta-pruning to work, there must be maximizing and minimizing
        // so this would oddly enough be very expensive
        //public static int GetWorstMoveIndex<TGameState, TMove> (
        //    TGameState gameState,
        //    IReadOnlyList<TMove> moves,
        //    int maxDepth,
        //    Func<TGameState, int, float> evaluate,
        //    bool randomTieBreaker
        //)
        //    where TGameState : GameState<TGameState, TMove>
        //{
        //    return SelectMove(
        //        initialGameState: gameState,
        //        moves: moves,
        //        maxDepth: maxDepth,
        //        maximize: (_, _) => true,                   // always maxmize (because of the inverted eval)
        //        evaluate: (gs, d) => (-evaluate(gs, d)),    // invert the evaluation
        //        randomTieBreaker: randomTieBreaker
        //    );
        //}

        public static float RateGameStateRecursive<TGameState, TMove> (
            TGameState currentState,
            int maxDepth,
            Func<TGameState, int, bool> maximize,
            Func<TGameState, int, float> evaluate
        )
            where TGameState : GameState<TGameState, TMove>
        {
            return RateGameStateRecursive<TGameState, TMove>(
                currentState: currentState,
                depth: 1,
                depthRemaining: maxDepth,
                alpha: float.NegativeInfinity,
                beta: float.PositiveInfinity,
                maximize: maximize,
                evaluate: evaluate
            );
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
