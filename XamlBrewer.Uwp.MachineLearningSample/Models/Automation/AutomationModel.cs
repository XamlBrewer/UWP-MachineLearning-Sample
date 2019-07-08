﻿using Microsoft.ML;
using Microsoft.ML.AutoML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms;
using Mvvm;
using System;
using System.Linq;

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
                        "Alcohol"}));

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

            // Because I can.
            //settings.Trainers.Remove(BinaryClassificationTrainer.AveragedPerceptron);

            _experiment = MLContext.Auto().CreateMulticlassClassificationExperiment(settings);
        }

        public void RunExperiment()
        {
            // Yields a silly exception on schema mismatch.
            // var result = _experiment.Execute(_trainingDataView, _validationDataView);

            var result = _experiment.Execute(
                trainData: _trainingDataView,
                labelColumnName: "Label",
                progressHandler: this);
        }

        public void Report(RunDetail<MulticlassClassificationMetrics> value)
        {
            Progressed?.Invoke(this, new ProgressEventArgs { CurrentExperiment = value.TrainerName });
        }
    }

    internal class ProgressEventArgs : EventArgs
    {
        public string CurrentExperiment { get; set; }
    }
}