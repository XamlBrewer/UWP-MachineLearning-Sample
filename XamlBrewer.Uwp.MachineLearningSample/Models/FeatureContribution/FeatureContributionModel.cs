using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers;
using Microsoft.ML.Transforms;
using Mvvm;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Storage;

namespace XamlBrewer.Uwp.MachineLearningSample.Models
{
    // Inspiration:
    // https://docs.microsoft.com/en-us/dotnet/api/microsoft.ml.explainabilitycatalog.calculatefeaturecontribution?view=ml-dotnet

    internal class FeatureContributionModel : ViewModelBase
    {
        public MLContext MLContext { get; } = new MLContext(seed: null);

        private RegressionPredictionTransformer<LinearRegressionModelParameters> _regressionModel;

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

            // Cache the data view in memory. For an iterative algorithm such as SDCA this makes a huge difference.
            trainData = MLContext.Data.Cache(trainData);

            ITransformer transformationModel = pipeline.Fit(trainData);

            // Prepare the data for the algorithm.
            var transformedData = transformationModel.Transform(trainData);

            // Choose a regression algorithm.
            // Compatible trainers: https://docs.microsoft.com/en-us/dotnet/api/microsoft.ml.transforms.featurecontributioncalculatingestimator?view=ml-dotnet
            var algorithm = MLContext.Regression.Trainers.Sdca();

            // Train the model and score it on the transformed data.
            _regressionModel = algorithm.Fit(transformedData);

            return _regressionModel.Model.Weights.ToList();
        }
    }
}
