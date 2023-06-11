using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterProject.MachineLearning {

    public interface IBootCampTerminationCondition<TGame, TIndividual>
        where TGame: Game, new()
        where TIndividual : Individual, new()
    {

        bool EndTraining (BootCamp<TGame, TIndividual> bootCamp);

    }

}
