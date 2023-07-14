namespace MasterProject.GodfieldLight {

    public class GodfieldGame : Game<GodfieldGame, GodfieldGameState, GodfieldMove> {

        // a reduced version of https://godfield.net/

        public const int MIN_PLAYER_COUNT = 2;
        public const int MAX_PLAYER_COUNT = 9;

        private int previousTurn;

        public override int MinimumNumberOfAgentsRequired => MIN_PLAYER_COUNT;

        public override int MaximumNumberOfAgentsAllowed => MAX_PLAYER_COUNT;

        public override Agent GetRandomAgent () {
            return new MasterProject.GodfieldLight.Agents.RandomAgent();
        }

        protected override GodfieldGameState GetInitialGameState () {
            var output = new GodfieldGameState();
            output.Initialize(this);
            return output;
        }

        protected override void OnGameStarted () {
            base.OnGameStarted();
            previousTurn = CurrentGameState.turnNumber;
        }

        protected override void OnBeforeMoveChosen (int agentIndex, IReadOnlyList<GodfieldMove> moves) {
            base.OnBeforeMoveChosen(agentIndex, moves);
            if (LogIsAllowed(ConsoleOutputs.Debug)) {
                if (CurrentGameState.currentAttack != null) {
                    TryDebugLog("Ongoing attack!");
                    if (CurrentGameState.currentPlayerWasHit) {
                        TryDebugLog("Player was hit!");
                    } else {
                        TryDebugLog("Attack missed current player!");
                    }
                } else {
                    TryDebugLog("No attack ongoing!");
                }
                if (moves.Count < 1) {
                    TryDebugLog("No moves available!");
                } else {
                    var sb = new System.Text.StringBuilder();
                    var cardsSb = new System.Text.StringBuilder();
                    sb.AppendLine("Available moves:");
                    var ps = CurrentGameState.playerStates[agentIndex];
                    for (int i = 0; i < moves.Count; i++) {
                        var move = moves[i];
                        cardsSb.Clear();
                        if (move.usedCardIndices.Length < 1) {
                            cardsSb.Append($"<no cards>");
                        } else {
                            foreach (var index in move.usedCardIndices) {
                                cardsSb.Append($"{ps.cards[index].id}, ");
                            }
                            cardsSb.Remove(cardsSb.Length - 2, 2);
                        }

                        if (CurrentGameState.currentPlayerWasHit) {
                            if (move.reflectAttack) {
                                sb.AppendLine($" {i}:\tReflect attack using {cardsSb}");
                            } else if (move.bounceAttack) {
                                sb.AppendLine($" {i}:\tBounce attack using {cardsSb}");
                            } else {
                                sb.AppendLine($" {i}:\tDefend for {move.defenseValue} points using {cardsSb} (take {Math.Max(0, CurrentGameState.currentAttack.damage - move.defenseValue)} dmg)");
                            }
                        } else {
                            if (move.attack != null) {
                                sb.AppendLine($" {i}:\tAttack for {move.attack.damage} damage ({(100 * move.attack.hitProbability):F2}%) using {cardsSb}");
                            } else if (move.healValue > 0) {
                                sb.AppendLine($" {i}:\tHeal for {move.healValue} hp using {cardsSb}");
                            } else {
                                sb.AppendLine($" {i}:\tDiscard {cardsSb}");
                            }
                        }
                    }
                    TryDebugLog(sb.ToString());
                }
            }
        }

        protected override void OnAfterMoveChosen (int agentIndex, IReadOnlyList<GodfieldMove> moves, int chosenMove) {
            base.OnAfterMoveChosen(agentIndex, moves, chosenMove);
            if (LogIsAllowed(ConsoleOutputs.Move)) {
                var ps = CurrentGameState.playerStates[agentIndex];
                var move = moves[chosenMove];
                if (move == default) {
                    TryLog(ConsoleOutputs.Move, "No move!");
                } else {
                    var cardsSb = new System.Text.StringBuilder();
                    foreach (var usedCardIndex in move.usedCardIndices) {
                        cardsSb.Append($"{ps.cards[usedCardIndex].id}, ");
                    }
                    if (cardsSb.Length > 0) {
                        cardsSb.Remove(cardsSb.Length - 2, 2);
                    } else {
                        cardsSb.Append("<no cards>");
                    }
                    if (CurrentGameState.currentPlayerWasHit) {
                        if (move.reflectAttack) {
                            TryLog(ConsoleOutputs.Move, $"Player chose move {chosenMove}: Reflect attack with {cardsSb}");
                        } else if (move.bounceAttack) {
                            TryLog(ConsoleOutputs.Move, $"Player chose move {chosenMove}: Bounce attack with {cardsSb}");
                        } else {
                            TryLog(ConsoleOutputs.Move, $"Player chose move {chosenMove}: Defend attack with {cardsSb} for value {move.defenseValue}");
                        }
                    } else {
                        if (move.attack != null) {
                            TryLog(ConsoleOutputs.Move, $"Player chose move {chosenMove}: Attack with {cardsSb} for {move.attack.damage} damage with {(100 * move.attack.hitProbability):F2}% hit probability");
                        } else if (move.healValue > 0) {
                            TryLog(ConsoleOutputs.Move, $"Player chose move {chosenMove}: Heal with {cardsSb} for {move.healValue} hp");
                        } else {
                            TryLog(ConsoleOutputs.Move, $"Player chose move {chosenMove}: Discard {cardsSb}");
                        }
                    }
                }
            }
        }

        protected override void OnGameStateUpdated () {
            base.OnGameStateUpdated();
            if (CurrentGameState.turnNumber != previousTurn) {
                CurrentGameState.ResolveUnresolvedCards();
                CurrentGameState.UpdateGameOver();
                if (!CurrentGameState.GameOver && LogIsAllowed(ConsoleOutputs.Move)) {
                    var sb = new System.Text.StringBuilder();
                    sb.AppendLine($"New Turn: {CurrentGameState.turnNumber}");
                    for (int i = 0; i < CurrentGameState.playerStates.Length; i++) {
                        sb.Append($"Player {i}: ");
                        var ps = CurrentGameState.playerStates[i];
                        if (ps.health > 0) {
                            sb.AppendLine($"{ps.health} HP");
                            sb.Append("\t");
                            foreach (var card in ps.cards) {
                                sb.Append($"{card.id}, ");
                            }
                            sb.Remove(sb.Length - 2, 2);
                            sb.AppendLine();
                        } else {
                            sb.AppendLine($"<DEAD>");
                        }
                    }
                    TryLog(ConsoleOutputs.Move, sb.ToString());
                }
            }
            previousTurn = CurrentGameState.turnNumber;
        }

        protected override void OnGameOver () {
            base.OnGameOver();
            var gs = CurrentGameState;
            for (int i = 0; i < gs.PlayerStates.Count; i++) {
                if (gs.PlayerStates[i].HasWon) {
                    TryLog(ConsoleOutputs.GameOver, $"Player {i} won!");
                }
                if (gs.PlayerStates[i].HasLost) {
                    TryLog(ConsoleOutputs.GameOver, $"Player {i} lost!");
                }
                if (gs.PlayerStates[i].HasDrawn) {
                    TryLog(ConsoleOutputs.GameOver, $"Player {i} drew!");
                }
            }
        }

    }

}
