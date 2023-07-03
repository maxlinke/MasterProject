namespace MasterProject.Chess {

    public class ChessGame : Game<ChessGame, ChessGameState, ChessMove> {

        public override int MinimumNumberOfAgentsRequired => ChessGameState.PLAYER_COUNT;

        public override int MaximumNumberOfAgentsAllowed => ChessGameState.PLAYER_COUNT;

        public override Agent GetRandomAgent () {
            return new MasterProject.Chess.Agents.RandomAgent();
        }

        protected override ChessGameState GetInitialGameState () {
            var output = new ChessGameState();
            output.Initialize();
            return output;
        }

        protected override void OnGameOver () {
            base.OnGameOver();
            var gs = (ChessGameState)GetFinalGameState();
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

        protected override void OnGameStarted () {
            base.OnGameStarted();
            TryDebugLog($"\n{CurrentGameState.ToPrintableString()}");
        }

        protected override void OnGameStateUpdated () {
            base.OnGameStateUpdated();
            TryDebugLog($"\n{CurrentGameState.ToPrintableString()}");
        }

        protected override void OnAfterMoveChosen (int agentIndex, IReadOnlyList<ChessMove> moves, int chosenMove) {
            base.OnAfterMoveChosen(agentIndex, moves, chosenMove);
            var move = moves[chosenMove];
            TryDebugLog($"{ChessGameState.CoordToString(move.srcCoord)} to {ChessGameState.CoordToString(move.dstCoord)}");
        }

    }

}
