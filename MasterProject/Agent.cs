using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterProject {

    public abstract class Agent { }

    public abstract class Agent<TGame, TGameState, TPlayerState, TMove, TAgent> : Agent
        where TGame : Game<TGame, TGameState, TPlayerState, TMove, TAgent>
        where TGameState : GameState<TGameState, TPlayerState, TMove>
        where TPlayerState : PlayerState
        where TMove : Move<TGameState, TPlayerState, TMove>
        where TAgent: Agent<TGame, TGameState, TPlayerState, TMove, TAgent>
    {

        protected TGame? Game { get; private set; }

        public virtual void OnGameStarted (TGame game) {
            this.Game = game;
        }

        public abstract int GetMoveIndex (IReadOnlyList<TMove> moves);

    }

}
