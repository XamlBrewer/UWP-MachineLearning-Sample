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

        private IDataView _allData;

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
                   Season = x[5],
                   TravelerType = x[6],
                   Hotel = x[13]
               });

            _allData = _mlContext.Data.LoadFromEnumerable(data);

            return _mlContext.Data.CreateEnumerable<FfmRecommendationData>(_allData, reuseRowObject: false);
        }

        public void Build()
        {
            // The following corresponds to the Recommendation sample:
            //var pipeline = _mlContext.Transforms.Categorical.OneHotEncoding("TravelerTypeOneHot", "TravelerType")
            //                .Append(_mlContext.Transforms.Categorical.OneHotEncoding("HotelOneHot", "Hotel"))
            //                .Append(_mlContext.Transforms.Concatenate("Features", "TravelerTypeOneHot", "HotelOneHot"))
            //                .Append(_mlContext.BinaryClassification.Trainers.FieldAwareFactorizationMachine(new string[] { "Features" }));

            var pipeline = _mlContext.Transforms.Categorical.OneHotEncoding("TravelerTypeOneHot", "TravelerType")
                .Append(_mlContext.Transforms.Categorical.OneHotEncoding("SeasonOneHot", "Season"))
                .Append(_mlContext.Transforms.Categorical.OneHotEncoding("HotelOneHot", "Hotel"))
                .Append(_mlContext.Transforms.Concatenate("Features", "TravelerTypeOneHot", "SeasonOneHot", "HotelOneHot"))
                .Append(_mlContext.BinaryClassification.Trainers.FieldAwareFactorizationMachine(new string[] { "Features" }));

            var trainingData = _mlContext.Data.ShuffleRows(_allData);
            trainingData = _mlContext.Data.TakeRows(trainingData, 450);

            // Place a breakpoint here to peek the training data.
            var preview = pipeline.Preview(trainingData, maxRows: 10);

            _model = pipeline.Fit(trainingData);
            _predictionEngine = _mlContext.Model.CreatePredictionEngine<FfmRecommendationData, FfmRecommendationPrediction>(_model);
        }

        public CalibratedBinaryClassificationMetrics Evaluate(string testDataPath)
        {
            var testData = _mlContext.Data.ShuffleRows(_allData);
            testData = _mlContext.Data.TakeRows(testData, 100);

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
            return _predictionEngine.Predict(recommendationData);
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
