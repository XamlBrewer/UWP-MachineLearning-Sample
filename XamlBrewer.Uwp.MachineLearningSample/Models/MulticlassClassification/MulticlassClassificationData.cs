namespace XamlBrewer.Uwp.MachineLearningSample.Models
{
    using Microsoft.ML.Runtime.Api;

    public class MulticlassClassificationData
    {
        [Column(ordinal: "0", name: "Label")]
        public float LanguageClass;

        [Column(ordinal: "1")]
        public string Text;
    }
}
