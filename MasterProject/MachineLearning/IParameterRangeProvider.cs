﻿namespace MasterProject.MachineLearning {
    
    public interface IParameterRangeProvider<T> {

        ParameterRange<T> GetRangeForParameterAtIndex (int index);

    }

}
