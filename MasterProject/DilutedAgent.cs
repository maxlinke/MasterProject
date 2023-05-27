using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterProject {

    public class DilutedAgent<TGame, TGameState, TMove> : Agent<TGame, TGameState, TMove>
        where TGame : Game
        where TGameState : GameState<TGameState, TMove>
    {

        private readonly Agent<TGame, TGameState, TMove> innerAgent;
        private readonly float concentration;

        public DilutedAgent (Agent<TGame, TGameState, TMove> innerAgent, float concentration) {
            this.innerAgent = innerAgent;
            this.concentration = concentration;
        }

        public override string Id => $"{innerAgent.Id}_{(100 * concentration):F2}%";
        public override bool IsTournamentEligible => innerAgent.IsTournamentEligible;
        public override bool IsStateless => innerAgent.IsStateless;

        public override Agent Clone () {
            var innerClone = (Agent<TGame, TGameState, TMove>)innerAgent.Clone();
            return new DilutedAgent<TGame, TGameState, TMove>(innerClone, concentration);
        }

        public override void OnGameStarted (TGame game) {
            base.OnGameStarted(game);
        }

        public override int GetMoveIndex (TGameState gameState, IReadOnlyList<TMove> moves) {
            var returnRandom = rng.NextDouble() > concentration;
            var agentMove = (innerAgent.IsStateless && returnRandom)
                ? default
                : innerAgent.GetMoveIndex(gameState, moves)
            ;
            return returnRandom
                ? GetRandomMoveIndex(moves)
                : agentMove
            ;
        }

    }

}
