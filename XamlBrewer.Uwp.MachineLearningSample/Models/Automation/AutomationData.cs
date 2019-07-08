using Microsoft.ML.Data;

namespace XamlBrewer.Uwp.MachineLearningSample.Models
{
    public class AutomationData
    {
        [LoadColumn(0)]
        public float FixedAcidity;

        [LoadColumn(1)]
        public float VolatileAcidity;

        [LoadColumn(2)]
        public float CitricAcid;

        [LoadColumn(3)]
        public float ResidualSugar;

        [LoadColumn(4)]
        public float Chlorides;

        [LoadColumn(5)]
        public float FreeSulfurDioxide;

        [LoadColumn(6)]
        public float TotalSulfurDioxide;

        [LoadColumn(7)]
        public float Density;

        [LoadColumn(8)]
        public float Ph;

        [LoadColumn(9)]
        public float Sulphates;

        [LoadColumn(10)]
        public float Alcohol;

        [LoadColumn(11)]
        public float Label;
    }
}
