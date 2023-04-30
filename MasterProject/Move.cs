using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterProject {

    public abstract class Move { }

    public abstract class Move<TGameState, TPlayerState, TMove> : Move
        where TGameState : GameState<TGameState, TPlayerState, TMove>
        where TPlayerState : PlayerState
        where TMove : Move<TGameState, TPlayerState, TMove>
    {

    }
}
