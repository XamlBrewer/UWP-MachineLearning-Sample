using Microsoft.ML.Core.Data;
using Microsoft.ML.Runtime.Api;
using Microsoft.ML.Runtime.Data;
using Microsoft.ML.Runtime.FastTree;
using Mvvm;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Storage;

namespace XamlBrewer.Uwp.MachineLearningSample.Models
{
    internal class RegressionModel : ViewModelBase
    {
        private LocalEnvironment _mlContext = new LocalEnvironment(seed: null); // v0.6;

        private IDataView trainingData;

        private PredictionFunction<RegressionData, RegressionPrediction> predictionFunction;

        public ITransformer Model { get; private set; }

        public IEnumerable<RegressionData> Load(string trainingDataPath)
        {
            var reader = new TextLoader(_mlContext,
                                        new TextLoader.Arguments()
                                        {
                                            Separator = ",",
                                            HasHeader = true,
                                            Column = new[]
                                                {
                                                    new TextLoader.Column("Label", DataKind.R4, 1),
                                                    new TextLoader.Column("NBA_DraftNumber", DataKind.R4, 3),
                                                    new TextLoader.Column("Age", DataKind.R4, 4),
                                                    new TextLoader.Column("Ws", DataKind.R4, 22),
                                                    new TextLoader.Column("Bmp", DataKind.R4, 26)
                                                }
                                        });

            var file = _mlContext.OpenInputFile(trainingDataPath);
            var src = new FileHandleSource(file);
            trainingData = reader.Read(src);
            return trainingData.AsEnumerable<RegressionData>(_mlContext, false);
        }

        public void BuildAndTrain(string trainingDataPath)
        {
            //var t = new FastTreeRegressionTrainer(_mlContext, "Label", "Features"); // PlatformNotSupportedException

            var pipeline = new NAReplaceEstimator(_mlContext, "Age", "Age", NAReplaceTransform.ColumnInfo.ReplacementMode.Mean)
                .Append(new NAReplaceEstimator(_mlContext, "Ws", "Ws", NAReplaceTransform.ColumnInfo.ReplacementMode.Mean))
                .Append(new NAReplaceEstimator(_mlContext, "Bmp", "Bmp", NAReplaceTransform.ColumnInfo.ReplacementMode.Mean))
                .Append(new NAReplaceEstimator(_mlContext, "NBA_DraftNumber", "NBA_DraftNumber", NAReplaceTransform.ColumnInfo.ReplacementMode.Mean))
                .Append(new ConcatEstimator(
                _mlContext,
                "Features",
                "NBA_DraftNumber", "Age", "Ws", "Bmp"))
                .Append(new RegressionGamTrainer(_mlContext, "Label", "Features"));

            Model = pipeline.Fit(trainingData);

            predictionFunction = Model.MakePredictionFunction<RegressionData, RegressionPrediction>(_mlContext);
        }

        public void Save(string modelName)
        {
            var storageFolder = ApplicationData.Current.LocalFolder;
            using (var fs = new FileStream(
                    Path.Combine(storageFolder.Path, modelName),
                    FileMode.Create,
                    FileAccess.Write,
                    FileShare.Write))
            {
                Model.SaveTo(_mlContext, fs);
            }
        }

        public IEnumerable<RegressionPrediction> PredictTrainingData()
        {
            var res = Model.Transform(trainingData);
            return res.AsEnumerable<RegressionPrediction>(_mlContext, false);
        }

        public RegressionPrediction Predict(RegressionData data)
        {
            return predictionFunction.Predict(data);
        }
    }
}
