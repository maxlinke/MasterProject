using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterProject {

    public class BootCamp {

        // this will be used for training
        // this will use tournaments
        // what is the structure?
        // one (kind of) agent of which we train variants?
        // or do we allow multiple kinds?
        // i think one kind of very VERY generic agent
        // and then a generic parameter
        // and a way to mutate the parameter
        // how much do i want to hardcode here
        // in theory it'd be great if this was so generic that it doesn't care about what kind of machine learning is happening
        // this just has a population
        // runs a tournament
        // creates a new population
        // runs a tournament
        // ...
        // until an end is reached

        // so, bootcamp has its own records
        // BUT that only works if bootcamp knows a little bit about its population
        // for example, if bootcamp knows the rules to get from one population to the next generation
        // then bootcamp can write a nice json file that i can visualize
        // just like in the g44p thing back in the second semester
        // but is that too restrictive?
        // on the other hand, bootcamp could be super generic like game
        // and i can make a GeneticAlgorithmBootCamp or G44PBootCamp and that of course knows that it is doing

        // in other words, the static constructors down here need to go because this isn't hyper-generic like tournament
        // more like game, where you DO need to actually instantiate the game

        public static BootCamp New () {
            throw new NotImplementedException();
        }

        public static BootCamp Continue () {
            throw new NotImplementedException();
        }

    }

}
