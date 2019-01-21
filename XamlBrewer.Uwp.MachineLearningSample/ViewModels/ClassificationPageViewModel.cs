using Microsoft.ML.Legacy.Models;
using Mvvm;
using System.Threading.Tasks;
using XamlBrewer.Uwp.MachineLearningSample.Models;

namespace XamlBrewer.Uwp.MachineLearningSample.ViewModels
{
    internal class ClassificationPageViewModel : ViewModelBase
    {
        private MulticlassClassificationModel _model = new MulticlassClassificationModel();

        public Task Build(string trainingDataPath)
        {
            return Task.Run(() =>
            {
                _model.Build(trainingDataPath);
            });
        }

        public Task Train()
        {
            return Task.Run(() =>
            {
                _model.Train();
            });
        }

        public Task Save(string modelName)
        {
            return Task.Run(() =>
            {
                _model.Save(modelName);
            });
        }

        public Task<ClassificationMetrics> Evaluate(string testDataPath)
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
