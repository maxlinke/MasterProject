using System.Collections.Generic;

namespace MasterProject.MachineLearning {

    public interface IParameterListConvertible<T> {

        IReadOnlyList<T> GetParameterList();

        void ApplyParameterList(IReadOnlyList<T> parameterList);

    }
}
