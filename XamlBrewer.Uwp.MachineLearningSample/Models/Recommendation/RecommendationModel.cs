using Microsoft.Data.DataView;
using Microsoft.ML;
using Microsoft.ML.Data;
using Mvvm;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Storage;

namespace XamlBrewer.Uwp.MachineLearningSample.Models
{
    internal class RecommendationModel : ViewModelBase
    {
        private MLContext _mlContext = new MLContext(seed: null);

        private IDataView trainingData;

        private ITransformer _model;

        private PredictionEngine<RecommendationData, RecommendationPrediction> predictionEngine;

        public IEnumerable<RecommendationData> Load(string trainingDataPath)
        {
            // Populating an IDataView from an IEnumerable.
            var data = File.ReadAllLines(trainingDataPath)
               .Skip(1)
               .Select(x => x.Split(';'))
               .Select(x => new RecommendationData
               {
                   Label = uint.Parse(x[4]),
                   TravelerType = x[6],
                   Hotel = x[13]
               })
               .OrderBy(x => (x.GetHashCode())) // Cheap Randomization.
               .Take(400);

            trainingData = _mlContext.Data.LoadFromEnumerable(data);

            // Keep in memory.
            trainingData = _mlContext.Data.Cache(trainingData);

            return _mlContext.Data.CreateEnumerable<RecommendationData>(trainingData, reuseRowObject: false);
        }

        public void Build()
        {
            var pipeline = _mlContext.Transforms.Conversion.MapValueToKey("Hotel")
                            .Append(_mlContext.Transforms.Conversion.MapValueToKey("TravelerType"))
                            .Append(_mlContext.Recommendation().Trainers.MatrixFactorization(
                                              labelColumn: DefaultColumnNames.Label,
                                              matrixColumnIndexColumnName: "Hotel",
                                              matrixRowIndexColumnName: "TravelerType",
                                              // Optional fine tuning:
                                              numberOfIterations: 20,
                                              approximationRank: 8,
                                              learningRate: 0.4))
                            .Append(_mlContext.Transforms.Conversion.MapKeyToValue("Hotel"))
                            .Append(_mlContext.Transforms.Conversion.MapKeyToValue("TravelerType"));

            // Place a breakpoint here to peek the training data.
            // Throws a System.ExecutionEngineException in x86 mode.
            // var preview = pipeline.Preview(trainingData, maxRows: 10);

            // Throws a System.ArgumentNullException in x86 mode on the 'source' parameter,
            // or a System.ExecutionEngineException,
            // or yields strange scores.
            _model = pipeline.Fit(trainingData);

            // Place a breakpoint here to see the Schema.
            // var prediction = _model.Transform(trainingData);

            predictionEngine = _model.CreatePredictionEngine<RecommendationData, RecommendationPrediction>(_mlContext);
        }

        public RegressionMetrics Evaluate(string testDataPath)
        {
            //var testData = _mlContext.Data.LoadFromTextFile<RecommendationData>(testDataPath);
            var data = File.ReadAllLines(testDataPath)
               .Skip(1)
               .Select(x => x.Split(';'))
               .Select(x => new RecommendationData
               {
                   Label = uint.Parse(x[4]),
                   TravelerType = x[6],
                   Hotel = x[13]
               })
               .OrderBy(x => (x.GetHashCode())) // Cheap Randomization.
               .TakeLast(200);

            var testData = _mlContext.Data.LoadFromEnumerable(data);
            var scoredData = _model.Transform(testData);
            var metrics = _mlContext.Recommendation().Evaluate(scoredData);

            // Place a breakpoint here to inspect the quality metrics.
            return metrics;
        }

        public void Save(string modelName)
        {
            var storageFolder = ApplicationData.Current.LocalFolder;
            using (var fs = new FileStream(
                    Path.Combine(storageFolder.Path, modelName),
                    FileMode.Create,
                    FileAccess.Write,
                    FileShare.Write))
            {
                _model.SaveTo(_mlContext, fs);
            }
        }

        public RecommendationPrediction Predict(RecommendationData recommendationData)
        {
            if (predictionEngine == null)
            {
                return null;
            }

            // Single prediction
            return predictionEngine.Predict(recommendationData);
        }

        public IEnumerable<RecommendationPrediction> Predict(IEnumerable<RecommendationData> recommendationData)
        {
            if (_model == null)
            {
                return null;
            }

            // Group prediction
            var data = _mlContext.Data.LoadFromEnumerable(recommendationData);
            var predictions = _model.Transform(data);
            return _mlContext.Data.CreateEnumerable<RecommendationPrediction>(predictions, reuseRowObject: false);
        }
    }
}
