using Microsoft.ML.Runtime.Api;

namespace XamlBrewer.Uwp.MachineLearningSample.Models
{
    public class RegressionData
    {
        [Column("1"), ColumnName("Label")]
        public float Salary;

        [Column("3")]
        public float NBA_DraftNumber;

        [Column("4")]
        public float Age;

        [Column("22")]
        public float Ws;

        [Column("26")]
        public float Bmp;
    }
}
