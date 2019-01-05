using Microsoft.ML.Runtime.Api;

namespace XamlBrewer.Uwp.MachineLearningSample.Models
{
    public class BinaryClassificationPrediction
    {
        [ColumnName("PredictedLabel")]
        public bool PredictedLabel;
    }
}
