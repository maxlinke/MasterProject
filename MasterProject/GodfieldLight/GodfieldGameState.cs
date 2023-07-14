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
        public bool currentPlayerWasHit { get; set; }

        public List<Attack> attacks { get; set; }
        public Attack? currentAttack => (attacks.Count > 0 ? attacks[0] : null);

        public int turnPlayer => turnNumber % playerStates.Length;
        public int livingPlayerCount => GodfieldPlayerState.GetLivingPlayerCount(this.playerStates);

        public override IReadOnlyList<GodfieldPlayerState> PlayerStates => playerStates;

        public override int CurrentPlayerIndex => (currentAttack != null ? currentAttack.remainingTargetPlayerIndices[0] : turnPlayer);

        private static readonly Random rng = new Random();

        public GodfieldGameState Clone () {
            ThrowErrorIfNotRealState();
            var output = new GodfieldGameState() {
                game = this.game,
                playerStates = GodfieldPlayerState.CreateProperArrayClone(this.playerStates),
                isRealState = true,
                turnNumber = this.turnNumber,
                attacks = new List<Attack>(this.attacks),
            };
            return output;
        }

        public GodfieldGameState CloneWithFirstAttackTargetRemovedAndUsedCardsUnresolved (int[] usedCardsIndices) {
            var output = this.Clone();
            var attackResult = this.currentAttack.GetResultWithFirstTargetRemoved();
            if (attackResult == null) {
                output.attacks.RemoveAt(0);
            } else {
                output.attacks[0] = attackResult;
            }
            var outputPlayerState = output.playerStates[this.CurrentPlayerIndex];
            for (int i = 0; i < usedCardsIndices.Length; i++) {
                outputPlayerState.cards[usedCardsIndices[i]] = Card.Unresolved;
                outputPlayerState.cardIds[usedCardsIndices[i]] = Card.Unresolved.id;
            }
            return output;
        }

        public override GodfieldGameState GetVisibleGameStateForPlayer (int playerIndex) {
            ThrowErrorIfNotRealState();
            var output = new GodfieldGameState();
            output.isRealState = false;
            output.turnNumber = this.turnNumber;
            output.currentPlayerWasHit = this.currentPlayerWasHit;
            output.attacks = this.attacks;  // is this safe? probably...
            output.playerStates = new GodfieldPlayerState[this.playerStates.Length];
            for (int i = 0; i < output.playerStates.Length; i++) {
                // dream and fog *would* have to be implemented here
                output.playerStates[i] = this.playerStates[i].Clone(cloneDeck: (i == playerIndex));   // we can't see other players' decks
            }
            return output;
        }

        void ThrowErrorIfNotRealState () {
            if (!isRealState) {
                throw new InvalidOperationException($"This can only be called on the actual game state!");
            }
        }

        public override IReadOnlyList<GodfieldMove> GetPossibleMovesForCurrentPlayer () {
            ThrowErrorIfNotRealState();
            if (currentAttack != null) {
                if (currentPlayerWasHit) {
                    if (currentAttack.instigatorPlayerIndex == CurrentPlayerIndex) {
                        return new GodfieldMove[] { GodfieldMove.EmptyMove() };     // if an attack was bounced back to one self, it can't be blocked
                    }
                    return MoveUtils.GetDefensiveMovesForCurrentPlayer(this).ToArray();
                } else {
                    // if a percentage attack missed, this will happen
                    // game will ask for the possible results of null
                    // so that needs to be accounted for
                    return new GodfieldMove[0];
                }
            } else if (CurrentPlayerIndex == turnPlayer) {
                return MoveUtils.GetOwnTurnMovesForCurrentPlayer(this).ToArray();
            } else {
                throw new System.NotImplementedException("???");
            }
        }

        public override IReadOnlyList<PossibleOutcome<GodfieldGameState>> GetPossibleOutcomesForMove (GodfieldMove move) {
            ThrowErrorIfNotRealState(); // do i want this?
            if (currentAttack != null) {
                if (currentPlayerWasHit) {
                    return GameStateUtils.GetOutcomesOfTakenHit(this, move);
                } else {
                    if (move != null) throw new System.NotImplementedException("????");
                    return GameStateUtils.GetOutcomeOfMissedAttack(this);
                }
            }
            if (move.healValue > 0) {
                return GameStateUtils.GetHealMoveResult(this, move);
            }
            if (move.attack != null) {
                return GameStateUtils.GetAttackMoveOutcomes(this, move);
            }
            // in godfield you can "pray" to get MORE cards if you have no attacks available, but this increases the number of cards you have
            // because this runs the risk of adding more armor cards here and therefore more armor combinations, i have decided against adding this
            // and instead there is only "discard" which simply replaces a card with the unresolved one
            return GameStateUtils.GetDiscardResult(this, move);
        }

        public void Initialize (GodfieldGame game) {
            this.game = game;
            Initialize(game.PlayerCount);
        }

        // this one's here for testing
        public void Initialize (int playerCount) {
            this.isRealState = true;
            this.turnNumber = 0;
            this.attacks = new List<Attack>();
            this.playerStates = new GodfieldPlayerState[playerCount];
            for (int i = 0; i < playerStates.Length; i++) {
                playerStates[i] = new GodfieldPlayerState();
                playerStates[i].index = i;
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

        public void UpdatePlayerHealth (int playerIndex, int newHealth) {
            playerStates[playerIndex].health = Math.Clamp(newHealth, 0, MAX_HEALTH);
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
                            while (ps.health > 0 && Devils.CheckIfDevilAppears(rng, out var devilId, out var dmg)) {
                                var hpDelta = -dmg;
                                if (game != null && game.LogIsAllowed(Game.ConsoleOutputs.Move)) {
                                    game.TryLog(Game.ConsoleOutputs.Move, $"Player {i} pulled {devilId} ({(hpDelta > 0 ? "+" : "")}{hpDelta} hp)");
                                }
                                UpdatePlayerHealth(i, ps.health + hpDelta);
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
            var livingPlayerCount = this.livingPlayerCount;
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
