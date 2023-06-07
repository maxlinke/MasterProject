using MasterProject.Records;

namespace MasterProject.MachineLearning {

    public class BootCamp<TGame, TIndividual> 
        where TGame : Game, new()
        where TIndividual: Individual, new()
    {

        public class GenerationConfiguration {

            public int newIndividualCount { get; set; }
            public int bestCloneCount { get; set; }
            public int invertedWorstCloneCount { get; set; }
            public int mutationCount { get; set; }
            public int combinationCount { get; set; }

            public int generationSize => newIndividualCount
                                       + bestCloneCount
                                       + invertedWorstCloneCount
                                       + mutationCount
                                       + combinationCount;

        }

        // fuck it. this master project is a framework that has A way to do machine learning
        // if parametrized genetic algorithms is that way, so be it. 

        public GenerationConfiguration config { get; set; }

        public List<TIndividual[]> generations { get; set; }

        public string latestRecordId { get; set; }

        // not sure i want to keep this method specifically, but the idea is there
        void TrainGeneration () {
            if(Tournament.TryLoadWinLossDrawRecord(latestRecordId, out var loadedRecord)){
                // see if that record is for the latest generation
            }

            // whatever the result, do a tournament 
        }

        void CreateNextGeneration (WinLossDrawRecord record) {
            var individualMap = new Dictionary<string, TIndividual>();
            foreach (var individual in generations[generations.Count - 1]) {
                var tempAgent = individual.CreateAgent();
                individualMap.Add(tempAgent.Id, individual);
            }
            var performanceList = new List<(string id, float winRate, float drawRate)>();
            foreach (var individualId in individualMap.Keys) {
                var index = Array.IndexOf(record.playerIds, individualId);
                var gameCount = record.totalWins[index] + record.totalLosses[index] + record.totalDraws[index];
                performanceList.Add((
                    id: individualId,
                    winRate: (float)(record.totalWins[index]) / gameCount,
                    drawRate: (float)(record.totalDraws[index]) / gameCount
                ));
            }
            performanceList.Sort((a, b) => {    // sort with highest coming first
                if (a.winRate != b.winRate)   return Math.Sign(b.winRate - a.winRate);
                if (a.drawRate != b.drawRate) return Math.Sign(b.drawRate - a.drawRate);
                return 0;
            });

            // just for testing
            Console.WriteLine("Rated by performance");
            for (int i = 0; i < performanceList.Count; i++) {
                Console.WriteLine($" > {i}: {performanceList[i].id} (wr {performanceList[i].winRate}) (dr {performanceList[i].drawRate})");
            }

            var nextGen = new List<TIndividual>(config.generationSize);
            AddIndividuals(config.newIndividualCount, _ => {
                var newAgent = new TIndividual();
                newAgent.InitializeWithRandomCoefficients();
                return newAgent;
            });
            AddIndividuals(config.bestCloneCount, i => {
                return GetIndividualByPerformance(i).Clone();
            });
            AddIndividuals(config.invertedWorstCloneCount, i => {
                return GetIndividualByPerformance(performanceList.Count - i - 1).InvertedClone();
            });
            AddIndividuals(config.mutationCount, i => {
                return GetIndividualByPerformance(i).MutatedClone();
            });
            AddIndividuals(config.combinationCount, i => {
                var src = GetIndividualByPerformance(i);
                var other = GetIndividualByPerformance((i + 1) % performanceList.Count);
                return src.CombinedClone(other);
            });
            generations.Add(nextGen.ToArray());

            TIndividual GetIndividualByPerformance (int index) {
                var id = performanceList[index].id;
                return individualMap[id];
            }

            void AddIndividuals (int count, System.Func<int, Individual> createIndividual) {
                for (int i = 0; i < count; i++) {
                    nextGen.Add((TIndividual)(createIndividual(i)));
                }
            }
        }

        public static BootCamp<TGame, TIndividual> Create (GenerationConfiguration config) {
            var output = new BootCamp<TGame, TIndividual>();
            output.config = config;
            output.generations = new List<TIndividual[]>();
            var firstGen = new TIndividual[config.generationSize];
            for (int i = 0; i < firstGen.Length; i++) {
                firstGen[i] = new TIndividual();
                firstGen[i].InitializeWithRandomCoefficients();
            }
            output.latestRecordId = null;
            return output;
        }

    }

}
