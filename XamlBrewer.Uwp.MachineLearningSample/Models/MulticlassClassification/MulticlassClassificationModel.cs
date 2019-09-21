using Microsoft.ML;
using Microsoft.ML.Data;
using Mvvm;
using System.IO;
using Windows.Storage;

namespace XamlBrewer.Uwp.MachineLearningSample.Models
{
    internal class MulticlassClassificationModel : ViewModelBase
    {
        // Console app style
        private void AllTheCode(string trainingDataPath, string testDataPath, string zipFilePath)
        {
            var mlContext = new MLContext(seed: null);

            var trainingData = mlContext.Data.LoadFromTextFile<MulticlassClassificationData>(trainingDataPath);

            var pipeline = MLContext.Transforms.Conversion.MapValueToKey("Label")
                .Append(MLContext.Transforms.Text.FeaturizeText("Features", "Text"))
                // Main algorithm
                // .Append(MLContext.MulticlassClassification.Trainers.StochasticDualCoordinateAscent())
                // or
                .Append(MLContext.MulticlassClassification.Trainers.LbfgsMaximumEntropy())
                // or
                // .Append(MLContext.MulticlassClassification.Trainers.NaiveBayes())
                .Append(MLContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

            var model = pipeline.Fit(_trainingData);

            var testData = MLContext.Data.LoadFromTextFile<MulticlassClassificationData>(testDataPath);
            var scoredData = model.Transform(testData);
            var qualityMetrics = mlContext.MulticlassClassification.Evaluate(scoredData);

            mlContext.Model.Save(
                model: model,
                inputSchema: trainingData.Schema,
                filePath: zipFilePath);

            var modelFromZip = mlContext.Model.Load(
                filePath: zipFilePath,
                inputSchema: out DataViewSchema inputSchema);

            var predictionModel = new PredictionModel<MulticlassClassificationData, MulticlassClassificationPrediction>
                (mlContext, modelFromZip);
            var prediction = predictionModel.Engine.Predict(new MulticlassClassificationData("text"));
        }

        private MLContext MLContext { get; } = new MLContext(seed: null);

        private PredictionModel<MulticlassClassificationData, MulticlassClassificationPrediction> Model { get; set; }

        private IEstimator<ITransformer> _pipeline;

        private IDataView _trainingData;

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
            _trainingData = MLContext.Data.LoadFromTextFile<MulticlassClassificationData>(trainingDataPath);
            ITransformer transformer = _pipeline.Fit(_trainingData);
            Model = new PredictionModel<MulticlassClassificationData, MulticlassClassificationPrediction>(MLContext, transformer);
        }

        public void Save(string modelName)
        {
            var storageFolder = ApplicationData.Current.LocalFolder;
            string modelPath = Path.Combine(storageFolder.Path, modelName);

            MLContext.Model.Save(
                model: Model.Transformer,
                inputSchema: _trainingData.Schema,
                filePath: modelPath);

            // For the sake of argument: reload.
            var model = MLContext.Model.Load(
                filePath: modelPath,
                inputSchema: out DataViewSchema inputSchema);

            // Place breakpoint here to inspect model and input schema:
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
