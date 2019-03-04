using Microsoft.ML.Data;

namespace XamlBrewer.Uwp.MachineLearningSample.Models
{
    public class MulticlassClassificationData
    {
        [LoadColumn(0), ColumnName("Label")]
        public float LanguageClass;

        [LoadColumn(1)]
        public string Text;

        public MulticlassClassificationData(string text)
        {
            Text = text;
        }
    }
}
