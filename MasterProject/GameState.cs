using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace MasterProject {

    public abstract class GameState {

#pragma warning disable CS8618
        private GameState () { /* explicitly disallowing parameterless constructors */ }
#pragma warning restore CS8618

        // what if rather than this i go down to the generic thing and make a public init method that takes in some generic init object
        // then the parameterless constructor would be all i need...
        public GameState (int playerCount) {
            this.playerCount = playerCount;
            this.currentPlayerIndex = 0;
            m_playerHasWon = new bool[playerCount];
            m_playerHasDrawn = new bool[playerCount];
            m_playerHasLost = new bool[playerCount];
            // add an explicit abstract init call here?
        }

        // what do i need this cloning for, exactly?
        // is it for creating the variants?
        // because that doesn't need cloning
        // well, the player data has to be cloned
        // hmm...
        public GameState (GameState source) {
            this.playerCount = source.playerCount;
            this.currentPlayerIndex = source.currentPlayerIndex;
            this.m_playerHasWon = new bool[playerCount];
            this.m_playerHasDrawn = new bool[playerCount];
            this.m_playerHasLost = new bool[playerCount];
            for(int i=0; i< playerCount; i++) {
                this.m_playerHasWon[i] = source.m_playerHasWon[i];
                this.m_playerHasDrawn[i] = source.m_playerHasDrawn[i];
                this.m_playerHasLost[i] = source.m_playerHasLost[i];
            }
            // add an explicit abstract copyvalues call here?
        }

        [JsonInclude] public int playerCount { get; private set; }
        [JsonInclude] public int currentPlayerIndex { get; protected set; }

        // i must admit, i'm not super happy with possibly having arrays for everything in here, rather than object-orienting this into player states
        // but since player states are driven by the gamestate, that kinda means that playerstate variables/methods/whatevers must be public and i don't like that
        [JsonInclude] protected bool[] m_playerHasWon;
        [JsonInclude] protected bool[] m_playerHasDrawn;
        [JsonInclude] protected bool[] m_playerHasLost;

        public IReadOnlyList<bool> playerHasWon => m_playerHasWon;
        public IReadOnlyList<bool> playerHasDrawn => m_playerHasDrawn;
        public IReadOnlyList<bool> playerHasLost => m_playerHasLost;

    }

    public abstract class GameState<TGameState, TMove> : GameState
        where TGameState : GameState<TGameState, TMove>
        where TMove : Move<TGameState, TMove> 
    {

        public GameState (int playerCount) : base(playerCount) { }

        public GameState (GameState<TGameState, TMove> source) : base(source) { }

        //public abstract IReadOnlyList<TMove> GetPossibleMoves ();   // no parameters? this assumes a player...

        

    }
}
