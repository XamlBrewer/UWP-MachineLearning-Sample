using Microsoft.ML;
using Microsoft.ML.Data;
using Mvvm;
using System.Collections.Generic;
using System.Threading.Tasks;
using XamlBrewer.Uwp.MachineLearningSample.Models;

namespace XamlBrewer.Uwp.MachineLearningSample.ViewModels
{
    internal class BinaryClassificationPageViewModel : ViewModelBase
    {
        private BinaryClassificationModel _model = new BinaryClassificationModel();

        public MLContext MLContext => _model.MLContext;

        public Task<PredictionModel<BinaryClassificationData, BinaryClassificationPrediction>> BuildAndTrain(string trainingDataPath, IEstimator<ITransformer> algorithm)
        {
            return Task.Run(() =>
            {
                return _model.BuildAndTrain(trainingDataPath, algorithm);
            });
        }

        public Task Save(PredictionModel<BinaryClassificationData, BinaryClassificationPrediction> model, string modelName)
        {
            return Task.Run(() =>
            {
                _model.Save(model, modelName);
            });
        }

        public Task<CalibratedBinaryClassificationMetrics> Evaluate(PredictionModel<BinaryClassificationData, BinaryClassificationPrediction> model, string testDataLocation)
        {
            return Task.Run(() =>
            {
                return _model.Evaluate(model, testDataLocation);
            });
        }

        public Task<BinaryClassificationMetrics> EvaluateNonCalibrated(PredictionModel<BinaryClassificationData, BinaryClassificationPrediction> model, string testDataLocation)
        {
            return Task.Run(() =>
            {
                return _model.EvaluateNonCalibrated(model, testDataLocation);
            });
        }

        public Task<IEnumerable<BinaryClassificationPrediction>> Predict(PredictionModel<BinaryClassificationData, BinaryClassificationPrediction> model, IEnumerable<BinaryClassificationData> data)
        {
            return Task.Run(() =>
            {
                return _model.Predict(model, data);
            });
        }

        public Task<IEnumerable<BinaryClassificationData>> GetSample(string sampleDataPath)
        {
            return Task.Run(() =>
            {
                return _model.GetSample(sampleDataPath);
            });
        }
    }
}
