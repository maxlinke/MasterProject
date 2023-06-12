namespace MasterProject.MachineLearning {

    public abstract class Individual {

        public struct TournamentResult {

            public int totalWins { get; set; }
            public int totalDraws { get; set; }
            public int totalLosses { get; set; }

        }

        public IndividualType IndividualType { get; set; }

        public string guid { get; set; }

        public string agentId { get; set; }

        public string[] parentGuids { get; set; }

        public float finalFitness { get; set; }

        public TournamentResult peerTournamentResult { get; set; }

        public TournamentResult randomTournamentResult { get; set; }

        protected static readonly System.Random rng = new System.Random();

        public abstract Agent CreateAgent ();

        public void InitializeWithRandomCoefficients () {
            RandomizeCoefficients();
            this.IndividualType = IndividualType.NewRandom;
            this.parentGuids = new string[0];
        }

        protected abstract void RandomizeCoefficients ();

        public Individual Clone () {
            var output = GetClone();
            output.IndividualType = IndividualType.Clone;
            output.parentGuids = new string[] { this.guid };
            return output;
        }

        protected abstract Individual GetClone ();

        public Individual CombinedClone (Individual other) {
            var output = GetClone();
            output.CombineCoefficients(other);
            output.IndividualType = IndividualType.Combination;
            output.parentGuids = new string[] { this.guid, other.guid };
            return output;
        }

        protected abstract void CombineCoefficients (Individual other);

        public Individual MutatedClone () {
            var output = GetClone();
            output.MutateCoefficients();
            output.IndividualType = IndividualType.Mutation;
            output.parentGuids = new string[] { this.guid };
            return output;
        }

        protected abstract void MutateCoefficients ();

        public Individual InvertedClone () {
            var output = GetClone();
            output.InvertCoefficients();
            output.IndividualType = IndividualType.InvertedClone;
            output.parentGuids = new string[] { this.guid };
            return output;
        }

        protected abstract void InvertCoefficients ();

    }

}
