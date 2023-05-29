using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterProject.G44P {

    public class G44PGameState : GameState<G44PGameState, G44PMove, G44PPlayerState> {

        public const int PLAYER_COUNT = 4;

        public const int FIELD_SIZE = 8;

        public override IReadOnlyList<G44PPlayerState> PlayerStates => throw new NotImplementedException();

        public override bool GameOver => throw new NotImplementedException();

        public override int CurrentPlayerIndex => throw new NotImplementedException();

        public override IReadOnlyList<G44PMove> GetPossibleMovesForCurrentPlayer () {
            throw new NotImplementedException();
        }

        public override IReadOnlyList<PossibleOutcome<G44PGameState>> GetPossibleOutcomesForMove (G44PMove move) {
            throw new NotImplementedException();
        }

        public override G44PGameState GetVisibleGameStateForPlayer (int playerIndex) {
            throw new NotImplementedException();
        }
    }

}
