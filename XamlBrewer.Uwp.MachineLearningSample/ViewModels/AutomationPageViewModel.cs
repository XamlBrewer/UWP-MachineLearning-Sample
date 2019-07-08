using Microsoft.ML;
using Mvvm;
using System.Threading.Tasks;
using XamlBrewer.Uwp.MachineLearningSample.Models;

namespace XamlBrewer.Uwp.MachineLearningSample.ViewModels
{
    internal class AutomationPageViewModel : ViewModelBase
    {
        private AutomationModel _model = new AutomationModel();
        private string currentExperiment = string.Empty;

        public AutomationPageViewModel()
        {
            _model.Progressed += (s, e) => CurrentExperiment = e.CurrentExperiment;
        }

        public MLContext MLContext => _model.MLContext;

        public string CurrentExperiment
        {
            get { return currentExperiment; }
            set { SetProperty(ref currentExperiment, value); }
        }

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
