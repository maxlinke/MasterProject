namespace MasterProject.MachineLearning {

    public struct ParameterRange<T> {

        public T min;
        public T max;

        public ParameterRange (T min, T max) {
            this.min = min;
            this.max = max;
        }

    }

}
