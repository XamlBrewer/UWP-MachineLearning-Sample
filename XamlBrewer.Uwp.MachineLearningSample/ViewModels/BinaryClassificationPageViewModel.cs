using Microsoft.ML.Legacy;
using Microsoft.ML.Legacy.Models;
using Microsoft.ML.Legacy.Transforms;
using Mvvm;
using System.Collections.Generic;
using System.Threading.Tasks;
using XamlBrewer.Uwp.MachineLearningSample.Models;

namespace XamlBrewer.Uwp.MachineLearningSample.ViewModels
{
    internal class BinaryClassificationPageViewModel : ViewModelBase
    {
        private BinaryClassificationModel _model = new BinaryClassificationModel();

        public Task<PredictionModel<BinaryClassificationData, BinaryClassificationPrediction>> BuildAndTrain(string trainingDataPath, ILearningPipelineItem algorithm)
        {
            return Task.Run(() =>
            {
                return _model.BuildAndTrain(trainingDataPath, algorithm);
            });
        }

        public Task Save(PredictionModel model, string modelName)
        {
            return Task.Run(() =>
            {
                _model.Save(model, modelName);
            });
        }

        public Task<BinaryClassificationMetrics> Evaluate(PredictionModel<BinaryClassificationData, BinaryClassificationPrediction> model, string testDataLocation)
        {
            return Task.Run(() =>
            {
                return _model.Evaluate(model, testDataLocation);
            });
        }

        public Task<IEnumerable<BinaryClassificationPrediction>> Predict(PredictionModel<BinaryClassificationData, BinaryClassificationPrediction> model, IEnumerable<BinaryClassificationData> data)
        {
            return Task.Run(() =>
            {
                return model.Predict(data);
            });
        }

        public Task<IEnumerable<BinaryClassificationData>> GetSample(string sampleDataPath)
        {
            return Task.Run(() =>
            {
                return _model.GetSample(sampleDataPath);
            });
        }

        private ILearningPipelineItem MakeNormalizer()
        {
            var normalizer = new BinNormalizer
            {
                NumBins = 2
            };
            normalizer.AddColumn("Label");

            return normalizer;
        }
    }
}
