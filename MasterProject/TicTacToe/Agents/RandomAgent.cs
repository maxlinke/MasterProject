using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterProject.TicTacToe.Agents {

    public class RandomAgent : TTTAgent {

        protected Random rng { get; private set; }

        public override void OnGameStarted (TTTGame game) {
            rng = new Random();
        }

        public override async Task<int> GetMoveIndex (IReadOnlyList<TTTMove> moves) {
            return rng.Next(0, moves.Count);
        }

    }

}
