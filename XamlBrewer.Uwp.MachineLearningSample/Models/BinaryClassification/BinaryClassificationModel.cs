using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms;
using Microsoft.ML.Transforms.Normalizers;
using Mvvm;
using System;
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
                .Append(MakeNormalizer())
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
            return File.ReadAllLines(sampleDataPath)
               .Skip(1)
               .Select(x => x.Split(';'))
               .Select(x => new BinaryClassificationData
               {
                   FixedAcidity = float.Parse(x[0]),
                   VolatileAcidity = float.Parse(x[1]),
                   CitricAcid = float.Parse(x[2]),
                   ResidualSugar = float.Parse(x[3]),
                   Chlorides = float.Parse(x[4]),
                   FreeSulfurDioxide = float.Parse(x[5]),
                   TotalSulfurDioxide = float.Parse(x[6]),
                   Density = float.Parse(x[7]),
                   Ph = float.Parse(x[8]),
                   Sulphates = float.Parse(x[9]),
                   Alcohol = float.Parse(x[10]),
                   Label = float.Parse(x[11])
               });
        }

        private IEstimator<ITransformer> MakeNormalizer()
        {
            var normalizer = MLContext.Transforms.Normalize(
                new NormalizingEstimator.BinningColumnOptions("Label", numBins: 2));

            return normalizer.Append(MLContext.Transforms.CustomMapping(new MapFloatToBool().GetMapping(), "MapFloatToBool"));
        }

        public class LabelInput
        {
            public float Label { get; set; }
        }

        public class LabelOutput
        {
            public bool Label { get; set; }

            public static LabelOutput True = new LabelOutput() { Label = true };
            public static LabelOutput False = new LabelOutput() { Label = false };
        }

        [CustomMappingFactoryAttribute("MapFloatToBool")]
        public class MapFloatToBool : CustomMappingFactory<LabelInput, LabelOutput>
        {
            public override Action<LabelInput, LabelOutput> GetMapping()
            {
                return (input, output) =>
                {
                    if (input.Label > 0)
                        output.Label = true;
                    else
                        output.Label = false;
                };
            }
        }
    }
}
