using Microsoft.ML.Runtime.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XamlBrewer.Uwp.MachineLearningSample.Models
{
    public class ClusterPrediction
    {
        [ColumnName("PredictedLabel")]
        public uint PredictedCluster;

        [ColumnName("Score")]
        public float[] Distances;

        public float AnnualIncome;

        public float SpendingScore;
    }
}
