namespace MasterProject.G44P {

    public class G44PGame : Game<G44PGame, G44PGameState, G44PMove> {

        public override int MinimumNumberOfAgentsRequired => G44PGameState.PLAYER_COUNT;

        public override int MaximumNumberOfAgentsAllowed => G44PGameState.PLAYER_COUNT;

        public override Agent GetRandomAgent () => new MasterProject.G44P.Agents.RandomAgent();

        protected override G44PGameState GetInitialGameState () {
            var output = new G44PGameState();
            output.Initialize(new List<string>(this.Agents.Select((agent) => agent.Id)));
            return output;
        }

        protected override void OnGameStateUpdated () {
            base.OnGameStateUpdated();
            if (DebugLogIsAllowed() && GameStates.Count > 0) {
                var previousGameState = GameStates[GameStates.Count - 1];
                var previousPlayerIndex = previousGameState.currentPlayerIndex;
                var latestMoveRecord = MoveRecords[MoveRecords.Count - 1];
                var latestMoveField = ((G44PMove)(latestMoveRecord.AvailableMoves[latestMoveRecord.ChosenMoveIndex])).fieldIndex;
                var log = previousGameState.ToPrintableString(previousPlayerIndex, latestMoveField, false);
                log = log.HorizontalConcat("\n\n\n ->  ".Replace("\n", System.Environment.NewLine));
                log = log.HorizontalConcat(CurrentGameState.ToPrintableString(CurrentGameState.CurrentPlayerIndex, true));
                TryDebugLog($"\n{log}\n");
            }
        }

    }

}
