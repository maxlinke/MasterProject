using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterProject {

    public abstract class Game {

        // what exactly is this?
        // does it PRODUCE a match?
        // what is a match?
        // is THIS a match?
        // can game states just be the entire thing and someone just needs to create an initial gamestate?
        // but then who decides about nondeterministic things?
        // i guess that'd be the match
        // and it'd keep track of the chain of gamestates

        // whatever i do
        // someone initializes the gamestate, asks whatever agent's turn it is for their move and updates the gamestate, rinse and repeat until finished

    }

}
