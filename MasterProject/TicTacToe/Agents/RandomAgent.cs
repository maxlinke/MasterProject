using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterProject.TicTacToe.Agents {

    public class RandomAgent : TTTAgent {

        public override void OnGameStarted (TTTGame game) { }

        public override int GetMoveIndex (IReadOnlyList<TTTMove> moves) {
            return GetRandomMoveIndex(moves);
        }

    }

}
