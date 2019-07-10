using Microsoft.ML;
using Mvvm;
using System.Threading.Tasks;
using XamlBrewer.Uwp.MachineLearningSample.Models;
using XamlBrewer.Uwp.MachineLearningSample.Models.Automation;

namespace XamlBrewer.Uwp.MachineLearningSample.ViewModels
{
    internal class AutomationPageViewModel : ViewModelBase
    {
        private AutomationModel _model = new AutomationModel();
        private AutomationExperiment currentExperiment;
        private AutomationExperiment bestExperiment;

        public AutomationPageViewModel()
        {
            _model.Progressed += (s, e) =>
            {
                CurrentExperiment = e.Model;
                if (BestExperiment == null || BestExperiment.LogLoss > CurrentExperiment.LogLoss)
                {
                    BestExperiment = CurrentExperiment;
                }
            };
        }

        public MLContext MLContext => _model.MLContext;

        public AutomationExperiment CurrentExperiment
        {
            get { return currentExperiment; }
            set { SetProperty(ref currentExperiment, value); }
        }

        public AutomationExperiment BestExperiment
        {
            get { return bestExperiment; }
            set { SetProperty(ref bestExperiment, value); }
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

        public Task HyperParameterize()
        {
            return Task.Run(() =>
            {
                _model.HyperParameterize();
            });
        }
    }
}
