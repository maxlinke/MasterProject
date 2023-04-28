using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterProject {
    public abstract class Move {

    }

    public abstract class Move<T, U> where T : GameState<U, T> where U : Move<T, U> {

    }
}
