using Microsoft.Data.DataView;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers.Recommender;
using Mvvm;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
               });

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
                                              matrixRowIndexColumnName: "TravelerType"))
                            .Append(_mlContext.Transforms.Conversion.MapKeyToValue("Hotel"))
                            .Append(_mlContext.Transforms.Conversion.MapKeyToValue("TravelerType"));
            //approximationRank: 10,
            //learningRate: 0.2,
            //numberOfIterations: 10));

            // Place a breakpoint here to peek the training data.
            // Throws a System.ExecutionEngineException in x86 mode.
            // var preview = pipeline.Preview(trainingData, maxRows: 10);

            // Throws a System.ArgumentNullException in x86 mode on the 'source' parameter,
            // or a System.ExecutionEngineException,
            // or yields strange scores.
            _model = pipeline.Fit(trainingData);

            // Place a breakpoint here to see the Schema.
            var prediction = _model.Transform(trainingData);

            predictionEngine = _model.CreatePredictionEngine<RecommendationData, RecommendationPrediction>(_mlContext);
        }

        public RecommendationPrediction Predict(RecommendationData recommendationData)
        {
            // Single prediction
            return predictionEngine.Predict(recommendationData);
        }

        public IEnumerable<RecommendationPrediction> Predict(IEnumerable<RecommendationData> recommendationData)
        {
            // Group prediction
            var data = _mlContext.Data.LoadFromEnumerable(recommendationData);
            var predictions = _model.Transform(data);
            return _mlContext.Data.CreateEnumerable<RecommendationPrediction>(predictions, reuseRowObject: false);
        }
    }
}
