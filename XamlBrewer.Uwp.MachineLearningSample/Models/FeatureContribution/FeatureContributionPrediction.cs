namespace XamlBrewer.Uwp.MachineLearningSample.Models
{
    class FeatureContributionPrediction : FeatureContributionData
    {
        public float Score { get; set; }

        public float[] FeatureContributions { get; set; }
    }
}
