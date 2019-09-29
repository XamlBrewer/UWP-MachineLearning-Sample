using Microsoft.ML;
using Microsoft.ML.Transforms;
using Mvvm;
using System.Collections.Generic;
using System.Linq;

namespace XamlBrewer.Uwp.MachineLearningSample.Models
{
    // Inspiration:
    // https://docs.microsoft.com/en-us/dotnet/machine-learning/how-to-guides/explain-machine-learning-model-permutation-feature-importance-ml-net

    internal class FeatureImportanceModel : ViewModelBase
    {
        public MLContext MLContext { get; } = new MLContext(seed: null);

        public List<FeatureImportance> ComputePermutationMetrics(string trainingDataPath)
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

            var trainData = MLContext.Data.LoadFromTextFile<FeatureImportanceData>(
                   path: trainingDataPath,
                   separatorChar: ';',
                   hasHeader: true);

            // Cache the data view in memory. For an iterative algorithm such as SDCA this makes a huge difference.
            trainData = MLContext.Data.Cache(trainData);

            var transformationModel = pipeline.Fit(trainData);

            // Prepare the data for the algorithm.
            var transformedData = transformationModel.Transform(trainData);

            // Choose a regression algorithm.
            var algorithm = MLContext.Regression.Trainers.Sdca();

            // Train the model and score it on the transformed data.
            var regressionModel = algorithm.Fit(transformedData);

            // Calculate the PFI metrics.
            var permutationMetrics = MLContext.Regression.PermutationFeatureImportance(
                regressionModel, 
                transformedData, 
                permutationCount: 50);

            // List of evaluation metrics:
            // https://docs.microsoft.com/en-us/dotnet/machine-learning/resources/metrics

            var result = new List <FeatureImportance> {
                        new FeatureImportance("FixedAcidity"),
                        new FeatureImportance("VolatileAcidity"),
                        new FeatureImportance("CitricAcid"),
                        new FeatureImportance("ResidualSugar"),
                        new FeatureImportance("Chlorides"),
                        new FeatureImportance("FreeSulfurDioxide"),
                        new FeatureImportance("TotalSulfurDioxide"),
                        new FeatureImportance("Density"),
                        new FeatureImportance("Ph"),
                        new FeatureImportance("Sulphates"),
                        new FeatureImportance("Alcohol")};

            for (int i = 0; i < permutationMetrics.Length; i++)
            {
                result[i].R2Decrease = permutationMetrics[i].RSquared.Mean;
            }

            return result;
        }
    }
}
