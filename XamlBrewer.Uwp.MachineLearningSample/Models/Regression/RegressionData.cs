using Microsoft.ML.Data;

namespace XamlBrewer.Uwp.MachineLearningSample.Models
{
    public class RegressionData
    {
        [LoadColumn(1), ColumnName("Label")]
        public float Salary;

        [LoadColumn(3)]
        public float NBA_DraftNumber;

        [LoadColumn(4)]
        public float Age;

        [LoadColumn(22)]
        public float Ws;

        [LoadColumn(26)]
        public float Bmp;
    }
}
