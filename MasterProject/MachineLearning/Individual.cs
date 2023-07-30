using System.Text.Json.Serialization;

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

        public void InitializeWithRandomParameters () {
            RandomizeParameters();
            this.IndividualType = IndividualType.NewRandom;
            this.parentGuids = new string[0];
        }

        protected abstract void RandomizeParameters ();

        public Individual Clone () {
            var output = GetClone();
            output.IndividualType = IndividualType.Clone;
            output.parentGuids = new string[] { this.guid };
            return output;
        }

        protected abstract Individual GetClone ();

        public Individual CombinedClone (Individual other) {
            var output = GetClone();
            output.CombineParameters(other);
            output.IndividualType = IndividualType.Combination;
            output.parentGuids = new string[] { this.guid, other.guid };
            return output;
        }

        protected abstract void CombineParameters (Individual other);

        public Individual MutatedClone () {
            var output = GetClone();
            output.MutateParameters();
            output.IndividualType = IndividualType.Mutation;
            output.parentGuids = new string[] { this.guid };
            return output;
        }

        protected abstract void MutateParameters ();

        public Individual InvertedClone () {
            var output = GetClone();
            output.InvertParameters();
            output.IndividualType = IndividualType.InvertedClone;
            output.parentGuids = new string[] { this.guid };
            return output;
        }

        protected abstract void InvertParameters ();

        public static IReadOnlyList<T> RandomlyPickFromBoth<T> (IReadOnlyList<T> a, IReadOnlyList<T> b, double ratio) {
            if (a.Count != b.Count) {
                throw new System.ArgumentException($"Both collections must be the same size but the first had {a.Count} elements and the second {b.Count}!");
            }
            var src = new IReadOnlyList<T>[] { a, b };
            var output = new T[a.Count];
            var remainingFromA = (int)(Math.Round(Math.Clamp(ratio, 0, 1) * a.Count));
            var sourceIndices = new int[output.Length];
            for (int i = 0; i < remainingFromA; i++) {
                sourceIndices[i] = 0;
            }
            for (int i = remainingFromA; i < output.Length; i++) {
                sourceIndices[i] = 1;
            }
            // https://en.wikipedia.org/wiki/Fisher%E2%80%93Yates_shuffle
            for (int i = 0; i < output.Length; i++) {
                var j = rng.Next(output.Length);
                var temp = sourceIndices[i];
                sourceIndices[i] = sourceIndices[j];
                sourceIndices[j] = temp;
            }
            for (int i = 0; i < output.Length; i++) {
                output[i] = src[sourceIndices[i]][i];
            }
            return output;
        }

    }

    public abstract class Individual<TParams, TParam> : Individual where TParams : IParameterListConvertible<TParam>, new() {
        
        public TParams agentParams { get; set; }

        protected override void CombineParameters (Individual other) {
            if (!(other is Individual<TParams, TParam>)) {
                throw new ArgumentException($"Can't automatically combine with agent of type {other.GetType()}!");
            }
            agentParams = agentParams ?? new TParams();
            var thisParams = agentParams.GetParameterList();
            var otherParams = ((Individual<TParams, TParam>)other).agentParams.GetParameterList();
            var combinedParams = RandomlyPickFromBoth(thisParams, otherParams, 0.5);
            agentParams.ApplyParameterList(combinedParams);
        }

    }

    public abstract class NumericallyParametrizedIndividual<TParams> : Individual<TParams, float> where TParams : IParameterListConvertible<float>, IParameterRangeProvider<float>, new() {

        public abstract float GetMaximumMutationStrength ();

        protected override void InvertParameters () {
            agentParams = agentParams ?? new TParams();
            var oldParams = agentParams.GetParameterList();
            var newParams = new float[oldParams.Count];
            for (int i = 0; i < newParams.Length; i++) {
                if (!agentParams.GetParameterIsInvertible(i)) {
                    newParams[i] = oldParams[i];
                    continue;
                }
                var range = agentParams.GetRangeForParameterAtIndex(i);
                var normed = (oldParams[i] - range.min) / (range.max - range.min);
                var flipped = 1f - normed;
                newParams[i] = ((flipped * (range.max - range.min)) + range.min);
            }
            agentParams.ApplyParameterList(newParams);
        }

        protected override void MutateParameters () {
            agentParams = agentParams ?? new TParams();
            var oldParams = agentParams.GetParameterList();
            var newParams = new float[oldParams.Count];
            for (int i = 0; i < newParams.Length; i++) {
                var range = agentParams.GetRangeForParameterAtIndex(i);
                var normed = (oldParams[i] - range.min) / (range.max - range.min);
                var bidiOffset = (float)(rng.NextDouble() - 0.5) * 2;
                var mutated = Math.Clamp(normed + (bidiOffset * GetMaximumMutationStrength()), 0, 1);
                newParams[i] = ((mutated * (range.max - range.min)) + range.min);
            }
            agentParams.ApplyParameterList(newParams);
        }

        protected override void RandomizeParameters () {
            agentParams = agentParams ?? new TParams();
            var newParams = new float[agentParams.GetParameterList().Count];
            for (int i = 0; i < newParams.Length; i++) {
                var range = agentParams.GetRangeForParameterAtIndex(i);
                newParams[i] = (float)((rng.NextDouble() * (range.max - range.min)) + range.min);
            }
            agentParams.ApplyParameterList(newParams);
        }
    }

}
