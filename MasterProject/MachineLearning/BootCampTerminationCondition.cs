using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterProject.MachineLearning {

    public static class BootCampTerminationCondition<TGame, TIndividual>
        where TGame : Game, new()
        where TIndividual: Individual, new()
    {

        private class DynamicCondition : IBootCampTerminationCondition<TGame, TIndividual> {

            private readonly System.Func<BootCamp<TGame, TIndividual>, bool> endTraining;

            public DynamicCondition (System.Func<BootCamp<TGame, TIndividual>, bool> endTraining) {
                this.endTraining = endTraining;
            }

            bool IBootCampTerminationCondition<TGame, TIndividual>.EndTraining (BootCamp<TGame, TIndividual> bootCamp) {
                return endTraining(bootCamp);
            }

        }

        public static IBootCampTerminationCondition<TGame, TIndividual> AfterFixedNumberOfGenerations (int generationCount) {
            return new DynamicCondition(bootCamp => {
                return bootCamp.generations.Count >= generationCount;
            });
        }

    }

}
