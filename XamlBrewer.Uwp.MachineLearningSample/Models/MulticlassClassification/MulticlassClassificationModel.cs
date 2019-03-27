using Microsoft.ML;
using Microsoft.ML.Data;
using Mvvm;
using System.IO;
using Windows.Storage;

namespace XamlBrewer.Uwp.MachineLearningSample.Models
{
    internal class MulticlassClassificationModel : ViewModelBase
    {
        private MLContext MLContext { get; } = new MLContext(seed: null);

        private PredictionModel<MulticlassClassificationData, MulticlassClassificationPrediction> Model { get; set; }

        private IEstimator<ITransformer> _pipeline;

        public void Build()
        {
            _pipeline = MLContext.Transforms.Conversion.MapValueToKey("Label")
                .Append(MLContext.Transforms.Text.FeaturizeText("Features", "Text"))
            // Main algorithm
            // .Append(MLContext.MulticlassClassification.Trainers.StochasticDualCoordinateAscent())
            // or
                .Append(MLContext.MulticlassClassification.Trainers.LbfgsMaximumEntropy())
            // or
            // .Append(MLContext.MulticlassClassification.Trainers.NaiveBayes()) // yields weird metrics...

                
            // Convert the predicted value back into a language.
                .Append(MLContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));
        }

        public void Train(string trainingDataPath)
        {
            var trainData = MLContext.Data.LoadFromTextFile<MulticlassClassificationData>(trainingDataPath);
            ITransformer transformer = _pipeline.Fit(trainData);
            Model = new PredictionModel<MulticlassClassificationData, MulticlassClassificationPrediction>(MLContext, transformer);
        }

        public void Save(string modelName)
        {
            var storageFolder = ApplicationData.Current.LocalFolder;
            string modelPath = Path.Combine(storageFolder.Path, modelName);

            MLContext.Model.Save(Model.Transformer, inputSchema: null, filePath: modelPath);
        }

        public MulticlassClassificationMetrics Evaluate(string testDataPath)
        {
            var testData = MLContext.Data.LoadFromTextFile<MulticlassClassificationData>(testDataPath);

            var scoredData = Model.Transformer.Transform(testData);
            return MLContext.MulticlassClassification.Evaluate(scoredData);
        }

        public MulticlassClassificationPrediction Predict(string text)
        {
            return Model.Engine.Predict(new MulticlassClassificationData(text));
        }
    }
}
