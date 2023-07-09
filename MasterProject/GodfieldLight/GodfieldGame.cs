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
            output.Initialize(this.PlayerCount);
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
            }
        }

    }

}
