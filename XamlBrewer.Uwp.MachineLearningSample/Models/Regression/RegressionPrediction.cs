using Microsoft.ML.Data;

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
