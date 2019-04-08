using Microsoft.ML;
using Microsoft.ML.Data;
using Mvvm;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Storage;

namespace XamlBrewer.Uwp.MachineLearningSample.Models
{
    internal class FfmRecommendationModel : ViewModelBase
    {
        private readonly double _ratingTreshold = 3;

        private MLContext _mlContext = new MLContext(seed: null);

        private IDataView _trainingData;

        private ITransformer _model;

        private PredictionEngine<FfmRecommendationData, FfmRecommendationPrediction> _predictionEngine;

        public IEnumerable<FfmRecommendationData> Load(string trainingDataPath)
        {
            // Populating an IDataView from an IEnumerable.
            var data = File.ReadAllLines(trainingDataPath)
               .Skip(1)
               .Select(x => x.Split(';'))
               .Select(x => new FfmRecommendationData
               {
                   Label = double.Parse(x[4]) > _ratingTreshold,
                   TravelerType = x[6],
                   Hotel = x[13]
               });

            _trainingData = _mlContext.Data.LoadFromEnumerable(data);

            // Keep in memory.
            _trainingData = _mlContext.Data.Cache(_trainingData);

            return _mlContext.Data.CreateEnumerable<FfmRecommendationData>(_trainingData, reuseRowObject: false);
        }

        public void Build()
        {
            var pipeline = _mlContext.Transforms.Categorical.OneHotEncoding("TravelerTypeOneHot", "TravelerType")
                            .Append(_mlContext.Transforms.Categorical.OneHotEncoding("HotelOneHot", "Hotel"))
                            .Append(_mlContext.Transforms.Concatenate("Features", "TravelerTypeOneHot", "HotelOneHot"))
                            .Append(_mlContext.BinaryClassification.Trainers.FieldAwareFactorizationMachine(new string[] { "Features" }));

            // Place a breakpoint here to peek the training data.
            var preview = pipeline.Preview(_trainingData, maxRows: 10);

            _model = pipeline.Fit(_trainingData);
            _predictionEngine = _mlContext.Model.CreatePredictionEngine<FfmRecommendationData, FfmRecommendationPrediction>(_model);
        }

        public CalibratedBinaryClassificationMetrics Evaluate(string testDataPath)
        {
            var data = File.ReadAllLines(testDataPath)
               .Skip(1)
               .Select(x => x.Split(';'))
               .Select(x => new FfmRecommendationData
               {
                   Label = double.Parse(x[4]) > _ratingTreshold,
                   TravelerType = x[6],
                   Hotel = x[13]
               })
               .OrderBy(x => (x.GetHashCode())) // Cheap Randomization.
               .TakeLast(200);

            var testData = _mlContext.Data.LoadFromEnumerable(data);
            var scoredData = _model.Transform(testData);
            var metrics = _mlContext.BinaryClassification.Evaluate(
                data: scoredData, 
                labelColumnName: "Label", 
                scoreColumnName: "Probability", 
                predictedLabelColumnName: "PredictedLabel");

            // Place a breakpoint here to inspect the quality metrics.
            return metrics;
        }

        public void Save(string modelName)
        {
            var storageFolder = ApplicationData.Current.LocalFolder;
            string modelPath = Path.Combine(storageFolder.Path, modelName);

            _mlContext.Model.Save(_model, inputSchema: null, filePath: modelPath);
        }

        public FfmRecommendationPrediction Predict(FfmRecommendationData recommendationData)
        {
            if (_predictionEngine == null)
            {
                return null;
            }

            // Single prediction
            var recommendationPrediction = _predictionEngine.Predict(recommendationData);
            if (!recommendationPrediction.PredictedLabel)
            {
                // Reverse probability if not recommended.
                recommendationPrediction.Probability = recommendationPrediction.Probability * -1;
            }

            return recommendationPrediction;
        }

        public IEnumerable<FfmRecommendationPrediction> Predict(IEnumerable<FfmRecommendationData> recommendationData)
        {
            if (_model == null)
            {
                return null;
            }

            // Group prediction
            var data = _mlContext.Data.LoadFromEnumerable(recommendationData);
            var predictions = _model.Transform(data);
            return _mlContext.Data.CreateEnumerable<FfmRecommendationPrediction>(predictions, reuseRowObject: false);
        }
    }
}
