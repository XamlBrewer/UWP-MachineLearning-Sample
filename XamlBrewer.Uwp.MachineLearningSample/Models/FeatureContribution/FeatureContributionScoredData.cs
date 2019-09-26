namespace XamlBrewer.Uwp.MachineLearningSample.Models
{
    class FeatureContributionScoredData : FeatureContributionData
    {
        public float Score { get; set; }
        public float[] Features { get; set; }
        public float[] FeatureContributions { get; set; }
    }
}
