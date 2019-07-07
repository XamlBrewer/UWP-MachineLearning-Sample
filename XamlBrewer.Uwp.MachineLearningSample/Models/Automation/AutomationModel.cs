using Microsoft.ML;
using Microsoft.ML.AutoML;
using Microsoft.ML.Transforms;
using Mvvm;
using System.Linq;

namespace XamlBrewer.Uwp.MachineLearningSample.Models
{
    internal class AutomationModel : ViewModelBase
    {
        private IDataView _trainingDataView;
        private IDataView _validationDataView;
        private BinaryClassificationExperiment _experiment;

        public MLContext MLContext { get; } = new MLContext(seed: null);

        public void CreateDataViews(string trainingDataPath, string validationDataPath)
        {
            // Pipeline
            IEstimator<ITransformer> pipeline =
                MLContext.Transforms.ReplaceMissingValues(
                    outputColumnName: "FixedAcidity",
                    replacementMode: MissingValueReplacingEstimator.ReplacementMode.Mean)
                .Append(MLContext.ValuationToBoolLabelNormalizer()) //--> multiple 'Label' columns confuse the experiment.
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
                        "Alcohol"}));

            // Training data
            var trainingData = MLContext.Data.LoadFromTextFile<AutomationData>(
                    path: trainingDataPath,
                    separatorChar: ';',
                    hasHeader: true);
            ITransformer model = pipeline.Fit(trainingData);
            _trainingDataView = model.Transform(trainingData);
            _trainingDataView = MLContext.Data.Cache(_trainingDataView);

            // Test data
            var validationData = MLContext.Data.LoadFromTextFile<AutomationData>(
                path: validationDataPath,
                separatorChar: ';',
                hasHeader: true);
            model = pipeline.Fit(validationData);
            _validationDataView = model.Transform(validationData);
            //_validationDataView = MLContext.Data.Cache(_validationDataView);
        }

        public void SetUpExperiment()
        {
            var settings = new BinaryExperimentSettings
            {
                MaxExperimentTimeInSeconds = 120,
                OptimizingMetric = BinaryClassificationMetric.AreaUnderRocCurve,
                CacheDirectory = null
            };

            _experiment = MLContext.Auto().CreateBinaryClassificationExperiment(settings);
        }

        public void RunExperiment()
        {
            // Yields a silly exception on schema mismatch.
            // var result = _experiment.Execute(_trainingDataView, _validationDataView);

            var result = _experiment.Execute(_trainingDataView);
        }
    }
}
