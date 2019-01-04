using Microsoft.ML.Runtime.Api;

namespace XamlBrewer.Uwp.MachineLearningSample.Models
{
    public class ClusteringPrediction
    {
        [ColumnName("PredictedLabel")]
        public uint PredictedCluster;

        [ColumnName("Score")]
        public float[] Distances;

        public float AnnualIncome;

        public float SpendingScore;
    }
}
