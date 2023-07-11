using System.Collections.Generic;

namespace MasterProject.GodfieldLight {

    public static class BinaryPermutationUtils {

        public static IReadOnlyList<IReadOnlyList<T>> GetBinaryPermutations<T> (IReadOnlyList<T> input) {
            var permutationCount = 1 << input.Count;
            var output = new List<T>[permutationCount];
            for (int i = 0; i < permutationCount; i++) {
                var list = new List<T>();
                for (int j = 0; j < input.Count; j++) {
                    if (((i >> j) & 1) == 1) {
                        list.Add(input[j]);
                    }
                }
                output[i] = list;
            }
            return output;
        }

    }

}
