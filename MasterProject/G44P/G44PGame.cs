namespace MasterProject.G44P {

    public class G44PGame : Game<G44PGame, G44PGameState, G44PMove> {

        protected override int MinimumNumberOfAgentsRequired => G44PGameState.PLAYER_COUNT;

        protected override int MaximumNumberOfAgentsAllowed => G44PGameState.PLAYER_COUNT;

        protected override G44PGameState GetInitialGameState () {
            var output = new G44PGameState();
            output.Initialize();
            return output;
        }

    }

}
