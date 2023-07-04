namespace MasterProject.Chess.Tests {

    public class SequencedTestAgent : ChessAgent {

        public override bool IsStateless => false;

        public override bool IsTournamentEligible => false;

        private readonly IReadOnlyList<string> moves;
        private int nextMoveIndex;

        public SequencedTestAgent (IReadOnlyList<string> moves) {
            this.moves = moves;
            this.nextMoveIndex = 0;
        }

        public override Agent Clone () {
            var output = new SequencedTestAgent(this.moves);
            output.nextMoveIndex = this.nextMoveIndex;
            return output;
        }

        public override int GetMoveIndex (ChessGameState gameState, IReadOnlyList<ChessMove> moves) {
            var nextMoveAsString = this.moves[nextMoveIndex];
            nextMoveIndex++;
            var split = nextMoveAsString.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var srcCoord = ChessGameState.CoordFromString(split[0]);
            var dstCoord = ChessGameState.CoordFromString(split[1]);
            for (int i = 0; i < moves.Count; i++) {
                if (moves[i].srcCoord == srcCoord && moves[i].dstCoord == dstCoord) {
                    return i;
                }
            }
            return -1;
        }
    }

}
