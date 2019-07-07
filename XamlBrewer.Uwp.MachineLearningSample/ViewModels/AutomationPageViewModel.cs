using Microsoft.ML;
using Mvvm;
using System.Threading.Tasks;
using XamlBrewer.Uwp.MachineLearningSample.Models;

namespace XamlBrewer.Uwp.MachineLearningSample.ViewModels
{
    internal class AutomationPageViewModel : ViewModelBase
    {
        private AutomationModel _model = new AutomationModel();

        public MLContext MLContext => _model.MLContext;

        public Task CreateDataViews(string trainingDataPath, string validationDataPath)
        {
            return Task.Run(() =>
            {
                _model.CreateDataViews(trainingDataPath, validationDataPath);
            });
        }

        public Task SetUpExperiment()
        {
            return Task.Run(() =>
            {
                _model.SetUpExperiment();
            });
        }

        public Task RunExperiment()
        {
            return Task.Run(() =>
            {
                _model.RunExperiment();
            });
        }
    }
}
