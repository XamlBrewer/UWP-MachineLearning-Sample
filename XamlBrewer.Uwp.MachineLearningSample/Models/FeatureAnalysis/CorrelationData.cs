using Microsoft.ML.Data;

namespace XamlBrewer.Uwp.MachineLearningSample.Models
{
    public class CorrelationData
    {
        [LoadColumn(1)]
        public float Survived;

        [LoadColumn(2)]
        public float PClass;

        [LoadColumn(5)]
        public float Age;

        [LoadColumn(6)]
        public float SibSp;

        [LoadColumn(7)]
        public float Parch;

        [LoadColumn(9)]
        public float Fare;
    }
}
