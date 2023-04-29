using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterProject {
    public abstract class Move {

    }

    public abstract class Move<TGameState, TMove> where TGameState : GameState<TGameState, TMove> where TMove : Move<TGameState, TMove> {

    }
}
