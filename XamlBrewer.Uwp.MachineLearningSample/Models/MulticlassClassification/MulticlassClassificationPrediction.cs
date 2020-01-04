using Microsoft.ML.Data;

namespace XamlBrewer.Uwp.MachineLearningSample.Models
{
    public class MulticlassClassificationPrediction
    {
        private readonly string[] classNames = { "German", "English", "French", "Italian", "Romanian", "Spanish" };

        [ColumnName("PredictedLabel")]
        public float Class;

        [ColumnName("Score")]
        public float[] Probabilities;

        public string PredictedLanguage => classNames[(int)Class];

        public int Confidence => (int)(Probabilities[(int)Class] * 100);
    }
}
