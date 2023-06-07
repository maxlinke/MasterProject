using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterProject.MachineLearning {

    public abstract class Individual {

        public IndividualType IndividualType { get; set; }

        public float fitness { get; set; }

        public abstract Agent CreateAgent ();

        public void InitializeWithRandomCoefficients () {
            RandomizeCoefficients();
            this.IndividualType = IndividualType.NewRandom;
        }

        protected abstract void RandomizeCoefficients ();

        public Individual Clone () {
            var output = GetClone();
            output.IndividualType = IndividualType.Clone;
            return output;
        }

        protected abstract Individual GetClone ();

        public Individual CombinedClone (Individual other) {
            var output = GetClone();
            output.CombineCoefficients(other);
            output.IndividualType = IndividualType.Combination;
            return output;
        }

        protected abstract void CombineCoefficients (Individual other);

        public Individual MutatedClone () {
            var output = GetClone();
            output.MutateCoefficients();
            output.IndividualType = IndividualType.Mutation;
            return output;
        }

        protected abstract void MutateCoefficients ();

        public Individual InvertedClone () {
            var output = GetClone();
            output.InvertCoefficients();
            output.IndividualType = IndividualType.InvertedClone;
            return output;
        }

        protected abstract void InvertCoefficients ();

    }

}
