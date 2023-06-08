using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterProject.MachineLearning {

    public abstract class Individual {

        public IndividualType IndividualType { get; set; }

        public int index { get; set; }

        public int[] parentIndices { get; set; }

        public float finalFitness { get; set; }

        public abstract Agent CreateAgent ();

        public void InitializeWithRandomCoefficients () {
            RandomizeCoefficients();
            this.IndividualType = IndividualType.NewRandom;
            this.parentIndices = new int[0];
        }

        protected abstract void RandomizeCoefficients ();

        public Individual Clone () {
            var output = GetClone();
            output.IndividualType = IndividualType.Clone;
            output.parentIndices = new int[] { this.index };
            return output;
        }

        protected abstract Individual GetClone ();

        public Individual CombinedClone (Individual other) {
            var output = GetClone();
            output.CombineCoefficients(other);
            output.IndividualType = IndividualType.Combination;
            output.parentIndices = new int[] { this.index, other.index };
            return output;
        }

        protected abstract void CombineCoefficients (Individual other);

        public Individual MutatedClone () {
            var output = GetClone();
            output.MutateCoefficients();
            output.IndividualType = IndividualType.Mutation;
            output.parentIndices = new int[] { this.index };
            return output;
        }

        protected abstract void MutateCoefficients ();

        public Individual InvertedClone () {
            var output = GetClone();
            output.InvertCoefficients();
            output.IndividualType = IndividualType.InvertedClone;
            output.parentIndices = new int[] { this.index };
            return output;
        }

        protected abstract void InvertCoefficients ();

    }

}
