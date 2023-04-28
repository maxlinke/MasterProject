using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterProject {
    public abstract class RatingFunction {



    }

    public abstract class RatingFunction<T> where T : GameState {

        // leave it like this?
        // in this case the ratingfunction needs to know a lot, like who to rate the state for
        // i think this is fine?
        // as in, a ratingfunction object can be created with an agent as input
        // and then that's that

        public abstract float CalculateScore (T inputState);

    }
}
