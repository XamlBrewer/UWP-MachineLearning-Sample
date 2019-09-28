using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers;
using Microsoft.ML.Transforms;
using Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;

namespace XamlBrewer.Uwp.MachineLearningSample.Models
{
    // Inspiration:
    // https://docs.microsoft.com/en-us/dotnet/api/microsoft.ml.explainabilitycatalog.calculatefeaturecontribution?view=ml-dotnet

    internal class FeatureContributionModel : ViewModelBase
    {
        public MLContext MLContext { get; } = new MLContext(seed: null);

        private IEnumerable<FeatureContributionData> _trainData;
        private IDataView _transformedData;
        private ITransformer _transformationModel;
        private RegressionPredictionTransformer<LinearRegressionModelParameters> _regressionModel;
        private PredictionEngine<FeatureContributionData, FeatureContributionPrediction> _predictionEngine;

        public List<float> BuildAndTrain(string trainingDataPath)
        {
            IEstimator<ITransformer> pipeline =
                MLContext.Transforms.ReplaceMissingValues(
                    outputColumnName: "FixedAcidity",
                    replacementMode: MissingValueReplacingEstimator.ReplacementMode.Mean)
                .Append(MLContext.Transforms.Concatenate("Features",
                    new[]
                    {
                        "FixedAcidity",
                        "VolatileAcidity",
                        "CitricAcid",
                        "ResidualSugar",
                        "Chlorides",
                        "FreeSulfurDioxide",
                        "TotalSulfurDioxide",
                        "Density",
                        "Ph",
                        "Sulphates",
                        "Alcohol"}))
                .Append(MLContext.Transforms.NormalizeMeanVariance("Features"));

            var trainData = MLContext.Data.LoadFromTextFile<FeatureContributionData>(
                   path: trainingDataPath,
                   separatorChar: ';',
                   hasHeader: true);

            // Keep the data avalailable.
            _trainData = MLContext.Data.CreateEnumerable<FeatureContributionData>(trainData, true);

            // Cache the data view in memory. For an iterative algorithm such as SDCA this makes a huge difference.
            // For OLS id does not matter.
            // trainData = MLContext.Data.Cache(trainData);

            _transformationModel = pipeline.Fit(trainData);

            // Prepare the data for the algorithm.
            _transformedData = _transformationModel.Transform(trainData);

            // Choose a regression algorithm.
            // Compatible trainers: https://docs.microsoft.com/en-us/dotnet/api/microsoft.ml.transforms.featurecontributioncalculatingestimator?view=ml-dotnet
            var algorithm = MLContext.Regression.Trainers.Sdca();

            // Train the model and score it on the transformed data.
            _regressionModel = algorithm.Fit(_transformedData);

            return _regressionModel.Model.Weights.ToList();
        }

        public void CreatePredictionModel()
        {
            // Define a feature contribution calculator for all the features.
            // Don't normalize the contributions.
            // https://docs.microsoft.com/en-us/dotnet/api/microsoft.ml.transforms.featurecontributioncalculatingestimator?view=ml-dotnet
            // "Does this estimator need to look at the data to train its parameters? No"
            var regressionData = _regressionModel.Transform(MLContext.Data.TakeRows(_transformedData, 1));

            var featureContributionCalculator = MLContext.Transforms
                .CalculateFeatureContribution(_regressionModel, normalize: false) // Estimator
                .Fit(regressionData); // Transformer

            // Create the full transformer chain.
            var scoringPipeline = _transformationModel
                .Append(_regressionModel)
                .Append(featureContributionCalculator);

            // Create the prediction engine.
            _predictionEngine = MLContext.Model.CreatePredictionEngine<FeatureContributionData, FeatureContributionPrediction>(scoringPipeline);
        }

        public FeatureContributionPrediction GetRandomPrediction()
        {
            return _predictionEngine.Predict(_trainData.ElementAt(new Random().Next(3918)));
        }
    }
}
