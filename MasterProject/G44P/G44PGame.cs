namespace MasterProject.G44P {

    public class G44PGame : Game<G44PGame, G44PGameState, G44PMove> {

        public override int MinimumNumberOfAgentsRequired => G44PGameState.PLAYER_COUNT;

        public override int MaximumNumberOfAgentsAllowed => G44PGameState.PLAYER_COUNT;

        public override Agent GetRandomAgent () => new MasterProject.G44P.Agents.RandomAgent();

        protected override G44PGameState GetInitialGameState () {
            var output = new G44PGameState();
            output.Initialize(new List<string>(this.Agents.Select((agent) => agent.Id)));
            return output;
        }

        protected override void OnGameStateUpdated () {
            base.OnGameStateUpdated();
            if (DebugLogIsAllowed() && GameStates.Count > 0) {
                var previousGameState = GameStates[GameStates.Count - 1];
                var previousPlayerIndex = previousGameState.currentPlayerIndex;
                var latestMoveRecord = MoveRecords[MoveRecords.Count - 1];
                var latestMoveField = ((G44PMove)(latestMoveRecord.AvailableMoves[latestMoveRecord.ChosenMoveIndex])).fieldIndex;
                var log = previousGameState.ToPrintableString(previousPlayerIndex, latestMoveField, false);
                log = log.HorizontalConcat("\n\n\n ->  ".Replace("\n", System.Environment.NewLine));
                log = log.HorizontalConcat(CurrentGameState.ToPrintableString(CurrentGameState.CurrentPlayerIndex, true));
                TryDebugLog($"\n{log}\n");
            }
        }

        // around 49,7 games on average, but with quite the spread
        public static void CalculateMoveStatistics (int matchCount) {
            if (matchCount < 1) {
                throw new Exception($"{matchCount} is too low!");
            }
            var t = Tournament<G44PGame>.New(4);
            t.MaxNumberOfGamesToRunInParallel = Math.Min(matchCount, 10000);
            totalMoveCounter = 0;
            minMoves = int.MaxValue;
            maxMoves = int.MinValue;
            histogram = histogram ?? new List<int>();
            histogram.Clear();
            for (int i = 0; i < 100; i++) {
                histogram.Add(0);
            }
            collectMoveStatistics = true;
            t.Run(new Agent[] { new G44PGame().GetRandomAgent() }, matchCount, MatchupFilter.AllowAllMatchups);
            collectMoveStatistics = false;
            // i don't trust double enough for this job
            var intPart = totalMoveCounter / matchCount;
            var remainder = totalMoveCounter % matchCount;
            var realAverage = intPart + ((float)remainder / matchCount);
            Console.WriteLine($"After {matchCount} matches, the average game took {realAverage} moves, the minimum was {minMoves} and the maximum was {maxMoves}");
            Console.WriteLine($"Here's the distribution in csv form:");
            for (int i = minMoves; i <= maxMoves; i++) {
                Console.WriteLine($"{i}, {histogram[i]}");
            }
        }

        private static bool collectMoveStatistics = false;
        private static long totalMoveCounter;
        private static int minMoves;
        private static int maxMoves;
        private static List<int> histogram;

        public override GameState GetFinalGameState () {
            if (collectMoveStatistics) {
                totalMoveCounter += this.MoveCounter;
                minMoves = Math.Min(minMoves, this.MoveCounter);
                maxMoves = Math.Max(maxMoves, this.MoveCounter);
                while (histogram.Count < this.MoveCounter) {
                    histogram.Add(0);
                }
                histogram[this.MoveCounter]++;
            }
            return base.GetFinalGameState();
        }

    }

}
