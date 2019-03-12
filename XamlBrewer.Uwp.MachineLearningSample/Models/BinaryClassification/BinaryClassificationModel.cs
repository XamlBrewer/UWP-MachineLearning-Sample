using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms;
using Mvvm;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Storage;

namespace XamlBrewer.Uwp.MachineLearningSample.Models
{
    internal class BinaryClassificationModel : ViewModelBase
    {
        public MLContext MLContext { get; } = new MLContext(seed: null);

        public PredictionModel<BinaryClassificationData, BinaryClassificationPrediction> BuildAndTrain(string trainingDataPath, IEstimator<ITransformer> algorithm)
        {
            IEstimator<ITransformer> pipeline =
                MLContext.Transforms.ReplaceMissingValues("FixedAcidity", replacementKind: MissingValueReplacingEstimator.ColumnOptions.ReplacementMode.Mean)
                .Append(MLContext.FloatToBoolLabelNormalizer())
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
                .Append(algorithm);

            var trainData = MLContext.Data.LoadFromTextFile<BinaryClassificationData>(
                    trainingDataPath, separatorChar: ';', hasHeader: true);

            ITransformer model =  pipeline.Fit(trainData);
            return new PredictionModel<BinaryClassificationData, BinaryClassificationPrediction>(MLContext, model);
        }

        public void Save(PredictionModel<BinaryClassificationData, BinaryClassificationPrediction> model, string modelName)
        {
            var storageFolder = ApplicationData.Current.LocalFolder;
            using (var fs = new FileStream(Path.Combine(storageFolder.Path, modelName), FileMode.Create, FileAccess.Write, FileShare.Write))
                MLContext.Model.Save(model.Transformer, fs);
        }

        public CalibratedBinaryClassificationMetrics Evaluate(PredictionModel<BinaryClassificationData, BinaryClassificationPrediction> model, string testDataLocation)
        {
            var testData = MLContext.Data.LoadFromTextFile<BinaryClassificationData>(testDataLocation, separatorChar: ';', hasHeader: true);

            var scoredData = model.Transformer.Transform(testData);
            return MLContext.BinaryClassification.Evaluate(scoredData);
        }

        public BinaryClassificationMetrics EvaluateNonCalibrated(PredictionModel<BinaryClassificationData, BinaryClassificationPrediction> model, string testDataLocation)
        {
            var testData = MLContext.Data.LoadFromTextFile<BinaryClassificationData>(testDataLocation, separatorChar: ';', hasHeader: true);

            var scoredData = model.Transformer.Transform(testData);
            return MLContext.BinaryClassification.EvaluateNonCalibrated(scoredData);
        }

        public IEnumerable<BinaryClassificationPrediction> Predict(PredictionModel<BinaryClassificationData, BinaryClassificationPrediction> model, IEnumerable<BinaryClassificationData> data)
        {
            foreach (BinaryClassificationData datum in data)
                yield return model.Engine.Predict(datum);
        }

        public IEnumerable<BinaryClassificationData> GetSample(string sampleDataPath)
        {
            var testData = MLContext.Data.LoadFromTextFile<BinaryClassificationData>(
                sampleDataPath, 
                separatorChar: ';', 
                hasHeader: true);
            return MLContext.Data.CreateEnumerable<BinaryClassificationData>(
                data: testData, 
                reuseRowObject: false);
        }
    }
}
