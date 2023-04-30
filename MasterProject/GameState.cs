using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace MasterProject {

    public abstract class GameState {

        public abstract bool GameOver { get; }

    }

    public abstract class GameState<TGameState, TPlayerState, TMove> : GameState
        where TGameState : GameState<TGameState, TPlayerState, TMove>
        where TPlayerState: PlayerState
        where TMove : Move<TGameState, TPlayerState, TMove> 
    {

        public int CurrentPlayerIndex { get; }

        // init and copy
        // init does not always need playercount, because for some games it's fixed
        // init gamestate can probably go into game/match...

        public abstract IReadOnlyList<TPlayerState> PlayerStates { get; }

        public abstract IReadOnlyList<TMove> GetPossibleMovesForCurrentPlayer ();

        public abstract IReadOnlyList<PossibleOutcome<TGameState>> GetPossibleOutcomesForMove (TMove move);

    }
}
