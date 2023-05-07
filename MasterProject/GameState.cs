﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace MasterProject {

    public abstract class GameState {

        public abstract bool GameOver { get; }

        public abstract int CurrentPlayerIndex { get; }

    }

    public abstract class GameState<TGameState, TMove> : GameState
        where TGameState : GameState<TGameState, TMove>
    {

        public abstract IReadOnlyList<TMove> GetPossibleMovesForCurrentPlayer ();

        public abstract IReadOnlyList<PossibleOutcome<TGameState>> GetPossibleOutcomesForMove (TMove move);

        public abstract TGameState GetVisibleGameStateForPlayer (int playerIndex);

    }

    public abstract class GameState<TGameState, TMove, TPlayerState> : GameState<TGameState, TMove>
        where TGameState : GameState<TGameState, TMove, TPlayerState>
        where TPlayerState : PlayerState
    {

        public abstract IReadOnlyList<TPlayerState> PlayerStates { get; }

    }

}
