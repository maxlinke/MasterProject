using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterProject.MachineLearning {

    public abstract class Generation {

        // should this be abstract? 
        // i don't know yet

        // i need to serialize these so i can visualize stuff
        // so the constructor needs to be public / nonexistent


        public abstract IReadOnlyList<Agent> GetAgents ();

        public abstract Generation GetNextGeneration ();

        //public static Generation CreateNew () {
        //    return new Generation();
        //}

    }

}
