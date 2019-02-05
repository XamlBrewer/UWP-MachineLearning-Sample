using Microsoft.ML.Runtime.Api;

namespace XamlBrewer.Uwp.MachineLearningSample.Models
{
    public class RegressionPrediction
    {
        [ColumnName("Label")]
        public float Salary;

        [ColumnName("Score")]
        public float Score;
    }
}
