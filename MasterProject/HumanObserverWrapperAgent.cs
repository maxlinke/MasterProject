namespace MasterProject {

    public class HumanObserverWrapperAgent<TGame, TGameState, TMove> : Agent<TGame, TGameState, TMove>
        where TGame : Game
        where TGameState : GameState<TGameState, TMove>
    {

        private readonly Agent<TGame, TGameState, TMove> innerAgent;

        public override string Id => innerAgent.Id;

        public HumanObserverWrapperAgent (Agent<TGame, TGameState, TMove> innerAgent) {
            this.innerAgent = innerAgent;
        }

        public override bool IsStateless => innerAgent.IsStateless;

        public override bool IsTournamentEligible => false;

        public override Agent Clone () {
            return new HumanObserverWrapperAgent<TGame, TGameState, TMove>(this.innerAgent);
        }

        public override int GetMoveIndex (TGameState gameState, IReadOnlyList<TMove> moves) {
            var innerMove = innerAgent.GetMoveIndex(gameState, moves);
            Console.WriteLine("Press return to continue");
            Console.ReadLine();
            return innerMove;
        }
    }

}
