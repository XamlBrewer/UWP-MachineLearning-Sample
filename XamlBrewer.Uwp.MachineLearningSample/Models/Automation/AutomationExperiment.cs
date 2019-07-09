namespace XamlBrewer.Uwp.MachineLearningSample.Models.Automation
{
    public class AutomationExperiment
    {
        public string Trainer { get; set; }

        public double? LogLoss { get; set; }

        public double? LogLossReduction { get; set; }

        public double? MicroAccuracy { get; set; }

        public double? MacroAccuracy { get; set; }
    }
}
