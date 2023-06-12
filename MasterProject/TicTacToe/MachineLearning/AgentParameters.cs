namespace MasterProject.TicTacToe.MachineLearning {

    public struct AgentParameters {

        public float winScore { get; set; }
        public float drawScore { get; set; }
        public float lossScore { get; set; }
        public float randomProbability { get; set; }

        public override bool Equals (object? obj) {
            return obj is AgentParameters parameters &&
                   winScore == parameters.winScore &&
                   drawScore == parameters.drawScore &&
                   lossScore == parameters.lossScore &&
                   randomProbability == parameters.randomProbability;
        }

        // the default hashcode combine thing is not consistent between running the program multiple times... what?
        public override int GetHashCode () {
            return winScore.GetHashCode()
                ^ drawScore.GetHashCode()
                ^ lossScore.GetHashCode()
                ^ randomProbability.GetHashCode()
            ;
        }
    }

}
