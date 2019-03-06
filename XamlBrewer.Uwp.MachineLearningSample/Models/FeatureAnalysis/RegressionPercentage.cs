using Microsoft.ML.Data;

namespace XamlBrewer.Uwp.MachineLearningSample.Models
{
    public class RegressionPercentage
    {
        [LoadColumn(9)]
        public float Ts;

        [LoadColumn(12)]
        public float Orb;

        [LoadColumn(13)]
        public float Drb;

        [LoadColumn(14)]
        public float Trb;

        [LoadColumn(15)]
        public float Ast;

        [LoadColumn(16)]
        public float Stl;

        [LoadColumn(17)]
        public float Blk;

        [LoadColumn(18)]
        public float Tov;

        [LoadColumn(19)]
        public float Usg;

        [LoadColumn(4)]
        public float Age;
    }
}
