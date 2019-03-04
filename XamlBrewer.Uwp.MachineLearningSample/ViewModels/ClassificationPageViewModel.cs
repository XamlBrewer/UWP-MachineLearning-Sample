using Microsoft.ML.Data;
using Mvvm;
using System.Threading.Tasks;
using XamlBrewer.Uwp.MachineLearningSample.Models;

namespace XamlBrewer.Uwp.MachineLearningSample.ViewModels
{
    internal class ClassificationPageViewModel : ViewModelBase
    {
        private MulticlassClassificationModel _model = new MulticlassClassificationModel();

        public Task Build()
        {
            return Task.Run(() =>
            {
                _model.Build();
            });
        }

        public Task Train(string trainingDataPath)
        {
            return Task.Run(() =>
            {
                _model.Train(trainingDataPath);
            });
        }

        public Task Save(string modelName)
        {
            return Task.Run(() =>
            {
                _model.Save(modelName);
            });
        }

        public Task<MultiClassClassifierMetrics> Evaluate(string testDataPath)
        {
            return Task.Run(() =>
            {
                return _model.Evaluate(testDataPath);
            });
        }

        public Task<MulticlassClassificationPrediction> Predict(string text)
        {
            return Task.Run(() =>
            {
                return _model.Predict(text);
            });
        }
    }
}
