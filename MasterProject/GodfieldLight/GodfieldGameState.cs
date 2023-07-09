using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MasterProject.GodfieldLight.Cards;

namespace MasterProject.GodfieldLight {

    public class GodfieldGameState : GameState<GodfieldGameState, GodfieldMove, GodfieldPlayerState> {

        public int DEFAULT_CARD_COUNT = 9;
        public int INIT_HEALTH = 40;
        public int MAX_HEALTH = 99;

        // this should be more "inspired" by godfield than actual godfield
        // what i want is a card game, not a board game
        // what i want is imperfect information
        // what i want is that there are moves that give multiple possible results
        // i don't need guardians
        // i don't need miracles
        // that means i don't need mana
        // or money
        // basically just hp, attack and defense cards
        // i do want the instakill attacks that must be defended against
        // so there is some strategy
        // and i want random weapons
        // and the bounce sword
        // and the reflect sword
        // so there is SOME tactics

        // adding back money/mana and the exchange card would be cool
        // but it clashes with the framework a bit
        // i will have to write that into the thesis
        // it is not impossible to fit it into the framework
        // but with 40 hp, 10 mp and 20$ there are (TODO CALCULATE) possible ways to use exchange
        // here's a way to make godfield work within this framework
        // move outcomes are only for weapon randomness
        // cards and such are simply "resolved" upon deciding on a state
        // i need a callback for that
        // well, i think i have
        // in game

        public GodfieldPlayerState[] playerStates { get; set; }
        public int currentPlayerIndex { get; set; }

        public bool isRealState { get; set; }
        public int turnNumber { get; set; }

        public override IReadOnlyList<GodfieldPlayerState> PlayerStates => playerStates;
        public override int CurrentPlayerIndex => currentPlayerIndex;

        void ThrowErrorIfNotRealState () {
            if (!isRealState) {
                throw new InvalidOperationException($"This can only be called on the actual game state!");
            }
        }

        public override IReadOnlyList<GodfieldMove> GetPossibleMovesForCurrentPlayer () {
            ThrowErrorIfNotRealState();
            // TODO
            throw new NotImplementedException();
        }

        public override IReadOnlyList<PossibleOutcome<GodfieldGameState>> GetPossibleOutcomesForMove (GodfieldMove move) {
            ThrowErrorIfNotRealState();
            // TODO
            throw new NotImplementedException();
        }

        public override GodfieldGameState GetVisibleGameStateForPlayer (int playerIndex) {
            ThrowErrorIfNotRealState();
            // TODO
            // actually important here
            // do i want to implement fog?
            // or "status effects" in general?
            // could also implement dream that way
            // although dream would require some kind of "parameter matching" to find cards that do the same thing, but differently
            throw new NotImplementedException();
        }

        public void Initialize (int playerCount) {
            this.playerStates = new GodfieldPlayerState[playerCount];
            for (int i = 0; i < playerCount; i++) {
                playerStates[i].health = INIT_HEALTH;
                playerStates[i].cards = new List<Cards.Card>();
                for (int j = 0; j < DEFAULT_CARD_COUNT; j++) {
                    playerStates[i].cards.Add(CardData.GetRandomCard());
                }
            }
            this.currentPlayerIndex = 0;
            this.turnNumber = 0;
            this.isRealState = true;
        }

        public void ResolveUnresolvedCards () {
            for (int i = 0; i < playerStates.Length; i++) {
                var cards = playerStates[i].cards;
                for (int j = 0; j < cards.Count; j++) {
                    if (cards[j] == Card.Unresolved) {
                        cards[j] = CardData.GetRandomCard();
                    }
                }
            }
        }

    }
}
