using Mvvm;
using System.Collections.Generic;
using System.Threading.Tasks;
using XamlBrewer.Uwp.MachineLearningSample.Models;

namespace XamlBrewer.Uwp.MachineLearningSample.ViewModels
{
    internal class FeatureImportancePageViewModel : ViewModelBase
    {
        private readonly FeatureImportanceModel _model = new FeatureImportanceModel();

        public Task<List<FeatureImportance>> ComputePermutationMetrics(string trainingDataPath)
        {
            return Task.Run(() =>
            {
                return _model.ComputePermutationMetrics(trainingDataPath);
            });
        }
    }
}
