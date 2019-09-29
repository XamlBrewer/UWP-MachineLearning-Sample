namespace XamlBrewer.Uwp.MachineLearningSample.Models
{
    public class FeatureImportance
    {
        public FeatureImportance(string name)
        {
            Name = name;
        }

        public string Name { get; set; }

        public double R2Decrease { get; set; }
    }
}
