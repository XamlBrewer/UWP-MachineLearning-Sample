namespace XamlBrewer.Uwp.MachineLearningSample.Models
{
    public class FeatureContribution
    {
        public FeatureContribution(string name)
        {
            Name = name;
        }

        public string Name { get; set; }

        public float Weight { get; set; }

        public float Contribution { get; set; }
    }
}
