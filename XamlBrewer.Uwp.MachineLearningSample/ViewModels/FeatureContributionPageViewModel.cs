using Microsoft.ML.Data;
using Mvvm;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XamlBrewer.Uwp.MachineLearningSample.Models;

namespace XamlBrewer.Uwp.MachineLearningSample.ViewModels
{
    internal class FeatureContributionPageViewModel : ViewModelBase
    {
        private readonly FeatureContributionModel _model = new FeatureContributionModel();

        public Task<List<float>> BuildAndTrain(string trainingDataPath)
        {
            return Task.Run(() =>
            {
                return _model.BuildAndTrain(trainingDataPath);
            });
        }

        public Task CreatePredictionModel()
        {
            return Task.Run(() =>
            {
                _model.CreatePredictionModel();
            });
        }
    }
}
