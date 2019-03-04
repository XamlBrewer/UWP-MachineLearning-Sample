using Microsoft.ML.Data;

namespace XamlBrewer.Uwp.MachineLearningSample.Models
{
    public class BinaryClassificationPrediction
    {
        [ColumnName("PredictedLabel")]
        public bool PredictedLabel;

        public int LabelAsNumber => PredictedLabel ? 1 : 0;
    }
}
