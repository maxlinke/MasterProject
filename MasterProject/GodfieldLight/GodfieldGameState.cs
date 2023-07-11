using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MasterProject.GodfieldLight {

    public class GodfieldGameState : GameState<GodfieldGameState, GodfieldMove, GodfieldPlayerState> {

        public int DEFAULT_CARD_COUNT = 7;  // reduced from 9 because of the POSSIBLE number of defense permutations (2 to the power of armor cards, so at 9 there would be 512, at 7 only 128 in the worst case)
        public int INIT_HEALTH = 40;
        public int MAX_HEALTH = 99;
        public int APOCALYPSE_START_TURN = 50;

        [System.Text.Json.Serialization.JsonIgnore] GodfieldGame game;

        public GodfieldPlayerState[] playerStates { get; set; }

        public bool isRealState { get; set; }
        public int turnNumber { get; set; }

        public Stack<Attack> attacks { get; set; }
        public Attack? currentAttack {
            get {
                if (attacks.Count > 0) {
                    return attacks.Peek();
                }
                return null;
            }
        }

        public int turnPlayer => turnNumber % playerStates.Length;

        public override IReadOnlyList<GodfieldPlayerState> PlayerStates => playerStates;

        public override int CurrentPlayerIndex {
            get {
                if (currentAttack != null) {
                    return currentAttack.remainingTargetPlayerIndices[0];
                }
                return turnPlayer;
            }
        }

        private static readonly Random rng = new Random();

        void ThrowErrorIfNotRealState () {
            if (!isRealState) {
                throw new InvalidOperationException($"This can only be called on the actual game state!");
            }
        }

        public override IReadOnlyList<GodfieldMove> GetPossibleMovesForCurrentPlayer () {
            ThrowErrorIfNotRealState();
            if (currentAttack != null) {
                return MoveUtils.GetDefensiveMovesForCurrentPlayer(this).ToArray();
            } else if (CurrentPlayerIndex == turnPlayer) {
                return MoveUtils.GetOwnTurnMovesForCurrentPlayer(this).ToArray();
            } else {
                throw new System.NotImplementedException("???");
            }
        }

        public override IReadOnlyList<PossibleOutcome<GodfieldGameState>> GetPossibleOutcomesForMove (GodfieldMove move) {
            ThrowErrorIfNotRealState();
            // TODO
            // process 
            // check for "pray" (specified above)
            throw new NotImplementedException();
        }

        public override GodfieldGameState GetVisibleGameStateForPlayer (int playerIndex) {
            ThrowErrorIfNotRealState();
            var output = new GodfieldGameState();
            output.isRealState = false;
            output.turnNumber = this.turnNumber;
            output.attacks = this.attacks;  // is this safe? probably...
            output.playerStates = new GodfieldPlayerState[this.playerStates.Length];
            for (int i = 0; i < output.playerStates.Length; i++) {
                // dream and fog *would* have to be implemented here
                output.playerStates[i] = this.playerStates[i].Clone(cloneDeck : (i == playerIndex));   // we can't see other players' decks
            }
            return output;
        }

        public void Initialize (GodfieldGame game) {
            this.game = game;
            Initialize(game.PlayerCount);
        }

        // this one's here for testing
        public void Initialize (int playerCount) {
            this.isRealState = true;
            this.turnNumber = 0;
            this.attacks = new Stack<Attack>();
            this.playerStates = new GodfieldPlayerState[playerCount];
            for (int i = 0; i < playerStates.Length; i++) {
                playerStates[i] = new GodfieldPlayerState();
                playerStates[i].health = INIT_HEALTH;
                playerStates[i].cards = new List<Card>();
                playerStates[i].cardIds = new List<string>();
                for (int j = 0; j < DEFAULT_CARD_COUNT; j++) {
                    var newCard = Card.GetRandomCard(rng);
                    playerStates[i].cards.Add(newCard);
                    playerStates[i].cardIds.Add(newCard.id);
                }
            }
        }

        public void ResolveUnresolvedCards () {
            for (int i = 0; i < playerStates.Length; i++) {
                var ps = playerStates[i];
                if (ps.health <= 0) {
                    continue;
                }
                var cards = ps.cards;
                var cardIds = ps.cardIds;
                for (int j = 0; j < cards.Count; j++) {
                    if (cards[j] == Card.Unresolved) {
                        if (turnNumber >= APOCALYPSE_START_TURN) {
                            while (ps.health > 0 && Devils.CheckIfDevilAppears(rng, out var devilId, out var deltaHp)) {
                                if (game != null && game.LogIsAllowed(Game.ConsoleOutputs.Move)) {
                                    game.TryLog(Game.ConsoleOutputs.Move, $"Player {i} pulled {devilId}");
                                }
                                ps.health = Math.Clamp(ps.health + deltaHp, 0, MAX_HEALTH);
                            }
                            if (ps.health <= 0) {
                                break;
                            }
                        }
                        var newCard = Card.GetRandomCard(rng);
                        cards[j] = newCard;
                        cardIds[j] = newCard.id;
                    }
                }
            }
        }

        public void UpdateGameOver () {
            var livingPlayerCount = 0;
            var newlyDeadPlayerCount = 0;
            for (int i = 0; i < playerStates.Length; i++) {
                var ps = playerStates[i];
                if (!ps.HasLost) {
                    if (ps.health > 0) {
                        livingPlayerCount++;
                    }else{
                        newlyDeadPlayerCount++;
                    }
                }
            }
            for (int i = 0; i < playerStates.Length; i++) {
                var ps = playerStates[i];
                if (!ps.HasLost) {
                    if (ps.health > 0) {
                        ps.HasWon = (livingPlayerCount == 1);
                    } else {
                        ps.HasLost = (livingPlayerCount > 0);
                        ps.HasDrawn = (livingPlayerCount == 0);
                    }
                }
            }
        }

    }
}
