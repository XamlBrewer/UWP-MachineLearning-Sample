using Mvvm;
using System.Collections.Generic;
using System.Threading.Tasks;
using XamlBrewer.Uwp.MachineLearningSample.Models;

namespace XamlBrewer.Uwp.MachineLearningSample.ViewModels
{
    internal class RegressionPageViewModel : ViewModelBase
    {
        private RegressionModel _model = new RegressionModel();

        public Task<IEnumerable<RegressionData>> Load(string trainingDataPath)
        {
            return Task.Run(() =>
            {
                return _model.Load(trainingDataPath);
            });
        }

        public Task BuildAndTrain(string regressionTrainer)
        {
            return Task.Run(() =>
            {
                _model.BuildAndTrain(regressionTrainer);
            });
        }

        public Task Save(string modelName)
        {
            return Task.Run(() =>
            {
                _model.Save(modelName);
            });
        }

        public Task<IEnumerable<RegressionPrediction>> PredictTrainingData()
        {
            return Task.Run(() =>
            {
                return _model.PredictTrainingData();
            });
        }

        public Task<RegressionPrediction> Predict(RegressionData data)
        {
            return Task.Run(() =>
            {
                return _model.Predict(data);
            });
        }
    }
}
