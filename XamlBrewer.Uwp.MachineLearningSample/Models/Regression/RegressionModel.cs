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
    internal class RegressionModel : ViewModelBase
    {
        private MLContext _mlContext = new MLContext(seed: null);

        private IDataView trainingData;

        private PredictionEngine<RegressionData, RegressionPrediction> predictionEngine;

        public ITransformer Model { get; private set; }

        public IEnumerable<RegressionData> Load(string trainingDataPath)
        {
            var readerOptions = new TextLoader.Options()
            {
                Separators = new[] { ',' },
                HasHeader = true,
                Columns = new[]
                    {
                        new TextLoader.Column("Label", DataKind.Single, 1),
                        new TextLoader.Column("NBA_DraftNumber", DataKind.Single, 3),
                        new TextLoader.Column("Age", DataKind.Single, 4),
                        new TextLoader.Column("Ws", DataKind.Single, 22),
                        new TextLoader.Column("Bmp", DataKind.Single, 26)
                    }
            };

            trainingData = _mlContext.Data.LoadFromTextFile(trainingDataPath, readerOptions);

            return _mlContext.Data.CreateEnumerable<RegressionData>(trainingData, reuseRowObject: false);
        }

        public void BuildAndTrain(string regressionTrainer)
        {
            var prepipeline = _mlContext.Transforms.ReplaceMissingValues("Age", "Age", MissingValueReplacingEstimator.ReplacementMode.Mean)
                .Append(_mlContext.Transforms.ReplaceMissingValues("Ws", "Ws", MissingValueReplacingEstimator.ReplacementMode.Mean))
                .Append(_mlContext.Transforms.ReplaceMissingValues("Bmp", "Bmp", MissingValueReplacingEstimator.ReplacementMode.Mean))
                .Append(_mlContext.Transforms.ReplaceMissingValues("NBA_DraftNumber", "NBA_DraftNumber", MissingValueReplacingEstimator.ReplacementMode.Mean))
                .Append(_mlContext.Transforms.NormalizeBinning("NBA_DraftNumber", "NBA_DraftNumber"))
                .Append(_mlContext.Transforms.NormalizeMinMax("Age", "Age"))
                .Append(_mlContext.Transforms.NormalizeMeanVariance("Ws", "Ws"))
                .Append(_mlContext.Transforms.NormalizeMeanVariance("Bmp", "Bmp"))
                .Append(_mlContext.Transforms.Concatenate(
                    "Features",
                    new[] { "NBA_DraftNumber", "Age", "Ws", "Bmp" }));
            // .Append(_mlContext.Regression.Trainers.FastTree()); // PlatformNotSupportedException
            // .Append(_mlContext.Regression.Trainers.OnlineGradientDescent(new OnlineGradientDescentTrainer.Options { })); // InvalidOperationException if you don't normalize.
            // .Append(_mlContext.Regression.Trainers.StochasticDualCoordinateAscent());       
            // .Append(_mlContext.Regression.Trainers.PoissonRegression());
            //.Append(_mlContext.Regression.Trainers.Gam());
            switch (regressionTrainer)
            {
                //case "FastTree": // PlatformNotSupportedException
                //    var pipelineFastTree = prepipeline.Append(_mlContext.Regression.Trainers.FastTree());
                //    Model = pipelineFastTree.Fit(trainingData);
                //    break;
                //case "FastTreeTweedie": // PlatformNotSupportedException
                //    var pipelineFastTreeTweedie = prepipeline.Append(_mlContext.Regression.Trainers.FastTreeTweedie());
                //    Model = pipelineFastTreeTweedie.Fit(trainingData);
                //    break;
                case "Gam":
                    var pipelineGam = prepipeline.Append(_mlContext.Regression.Trainers.Gam());
                    Model = pipelineGam.Fit(trainingData);
                    break;
                case "LightGbm":
                    var pipelineLightGbm = prepipeline.Append(_mlContext.Regression.Trainers.LightGbm());
                    Model = pipelineLightGbm.Fit(trainingData);
                    break;
                case "Ols":
                    var pipelineOls = prepipeline.Append(_mlContext.Regression.Trainers.Ols());
                    Model = pipelineOls.Fit(trainingData);
                    break;
                case "Sdca":
                    var pipelineSdca = prepipeline.Append(_mlContext.Regression.Trainers.Sdca());
                    Model = pipelineSdca.Fit(trainingData);
                    break;
                default:
                    var pipeline = prepipeline.Append(_mlContext.Regression.Trainers.Gam());
                    Model = pipeline.Fit(trainingData);
                    break;
            }
            predictionEngine = _mlContext.Model.CreatePredictionEngine<RegressionData, RegressionPrediction>(Model);
        }

        public void Save(string modelName)
        {
            var storageFolder = ApplicationData.Current.LocalFolder;
            string modelPath = Path.Combine(storageFolder.Path, modelName);

            _mlContext.Model.Save(Model, inputSchema: null, filePath: modelPath);
        }

        public IEnumerable<RegressionPrediction> PredictTrainingData()
        {
            var res = Model.Transform(trainingData);
            return _mlContext.Data.CreateEnumerable<RegressionPrediction>(res, reuseRowObject: false);
        }

        public RegressionPrediction Predict(RegressionData data)
        {
            return predictionEngine.Predict(data);
        }
    }
}
