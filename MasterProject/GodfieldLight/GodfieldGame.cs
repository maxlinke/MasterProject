namespace MasterProject.GodfieldLight {

    public class GodfieldGame : Game<GodfieldGame, GodfieldGameState, GodfieldMove> {

        // a reduced version of https://godfield.net/

        private int previousTurn;

        public override int MinimumNumberOfAgentsRequired => 2;

        public override int MaximumNumberOfAgentsAllowed => 9;

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

        protected override void OnGameStateUpdated () {
            base.OnGameStateUpdated();
            if (CurrentGameState.turnNumber != previousTurn) {
                CurrentGameState.ResolveUnresolvedCards();
                CurrentGameState.UpdateGameOver();
            }
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
