using Microsoft.ML;
using Microsoft.ML.AutoML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers;
using Microsoft.ML.Transforms;
using Mvvm;
using System;
using System.IO;
using System.Linq;
using Windows.Storage;
using XamlBrewer.Uwp.MachineLearningSample.Models.Automation;

namespace XamlBrewer.Uwp.MachineLearningSample.Models
{
    internal class AutomationModel : ViewModelBase, IProgress<RunDetail<MulticlassClassificationMetrics>>
    {
        private IDataView _trainingDataView;
        private IDataView _validationDataView;
        private MulticlassClassificationExperiment _experiment;

        public event EventHandler<ProgressEventArgs> Progressed;

        public MLContext MLContext { get; } = new MLContext(seed: null);

        public void CreateDataViews(string trainingDataPath, string validationDataPath)
        {
            // Pipeline
            IEstimator<ITransformer> pipeline =
                MLContext.Transforms.ReplaceMissingValues(
                    outputColumnName: "FixedAcidity",
                    inputColumnName: "OriginalFixedAcidity",
                    replacementMode: MissingValueReplacingEstimator.ReplacementMode.Mean)
                .Append(MLContext.Transforms.DropColumns("OriginalFixedAcidity"));

            // No need to add this, it will be done automatically.
            //.Append(MLContext.Transforms.Concatenate("Features",
            //    new[]
            //    {
            //        "FixedAcidity",
            //        "VolatileAcidity",
            //        "CitricAcid",
            //        "ResidualSugar",
            //        "Chlorides",
            //        "FreeSulfurDioxide",
            //        "TotalSulfurDioxide",
            //        "Density",
            //        "Ph",
            //        "Sulphates",
            //        "Alcohol"}));

            // Training data
            var trainingData = MLContext.Data.LoadFromTextFile<AutomationData>(
                    path: trainingDataPath,
                    separatorChar: ';',
                    hasHeader: true);
            ITransformer model = pipeline.Fit(trainingData);
            _trainingDataView = model.Transform(trainingData);
            _trainingDataView = MLContext.Data.Cache(_trainingDataView);

            // Check the content on a breakpoint:
            var sneakPeek = _trainingDataView.Preview();

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
            var settings = new MulticlassExperimentSettings
            {
                MaxExperimentTimeInSeconds = 180,
                OptimizingMetric = MulticlassClassificationMetric.LogLoss,
                CacheDirectory = null
            };

            // These two trainers yield no metrics in UWP:
            settings.Trainers.Remove(MulticlassClassificationTrainer.FastTreeOva);
            settings.Trainers.Remove(MulticlassClassificationTrainer.FastForestOva);

            _experiment = MLContext.Auto().CreateMulticlassClassificationExperiment(settings);
        }

        public string RunExperiment()
        {
            // Yields a silly exception on schema mismatch.
            // var result = _experiment.Execute(_trainingDataView, _validationDataView);

            var result = _experiment.Execute(
                trainData: _trainingDataView,
                labelColumnName: "Label",
                progressHandler: this);

            return result.BestRun.TrainerName;
        }

        public void HyperParameterize()
        {
            var settings = new MulticlassExperimentSettings
            {
                MaxExperimentTimeInSeconds = 180,
                OptimizingMetric = MulticlassClassificationMetric.LogLoss,
                CacheDirectory = null
            };

            // There can be only one.
            settings.Trainers.Clear();

            // It's hard to discover its parameters.
            // And there's a bug in 1.3.1 ...
            // settings.Trainers.Add(MulticlassClassificationTrainer.LightGbm);

            // This one's easier:
            settings.Trainers.Add(MulticlassClassificationTrainer.LbfgsMaximumEntropy);

            var experiment = MLContext.Auto().CreateMulticlassClassificationExperiment(settings);

            var result = experiment.Execute(
                trainData: _trainingDataView,
                labelColumnName: "Label",
                progressHandler: this);

            var model = result.BestRun.Model as TransformerChain<ITransformer>;

            var storageFolder = ApplicationData.Current.LocalFolder;
            string modelPath = Path.Combine(storageFolder.Path, "Automation.zip");

            MLContext.Model.Save(
                model: model,
                inputSchema: null,
                filePath: modelPath);

            var singleFeaturePredictor = model.First() as TransformerChain<ISingleFeaturePredictionTransformer<object>>;
            /// var multiclassPredictor = singleFeaturePredictor.LastTransformer as MulticlassPredictionTransformer<OneVersusAllModelParameters>;
            // When using MulticlassClassificationTrainer.LbfgsMaximumEntropy:
            var multiclassPredictor = singleFeaturePredictor.LastTransformer as MulticlassPredictionTransformer<MaximumEntropyModelParameters>;
            var algorithm = multiclassPredictor.Model;
            // ... and the rest is not publicly exposed.
            // So it's breakpoint time.
        }

        public void Report(RunDetail<MulticlassClassificationMetrics> value)
        {
            Progressed?.Invoke(this, new ProgressEventArgs
            {
                Model = new AutomationExperiment
                {
                    Trainer = value.TrainerName,
                    LogLoss = value.ValidationMetrics?.LogLoss,
                    LogLossReduction = value.ValidationMetrics?.LogLossReduction,
                    MicroAccuracy = value.ValidationMetrics?.MicroAccuracy,
                    MacroAccuracy = value.ValidationMetrics?.MacroAccuracy
                }
            });
        }
    }

    internal class ProgressEventArgs : EventArgs
    {
        public AutomationExperiment Model { get; set; }
    }
}
