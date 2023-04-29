using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterProject {

    public abstract class Game {

        // all the types
            // gamestate -> playerstate
            // playerstate
            // move
            // the whole "possibility" thing (move -> possibilities -> gamestate + likelyhood) this is just a wrapper so probably doesn't need its own type
            // agents -> game and everything

        // ... this isn't too bad?

        // more types (minmax)
            // ratingfunctions -> gamestate and everything

        // THIS CLASS KEEPS ALL THE GAMESTATES THAT HAVE PASSED
        // THIS CLASS ASKS THE AGENTS TO DECIDE ON A MOVE
        // THIS CLASS IS SERIALIZABLE?
        //      -> gamestates
        //      -> chosen moves
        //      -> agent ids
        //      -> start/end time

    }

}
