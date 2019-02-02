using Microsoft.ML.Core.Data;
using Microsoft.ML.Legacy;
using Microsoft.ML.Legacy.Trainers;
using Microsoft.ML.Legacy.Transforms;
using Microsoft.ML.Runtime.Api;
using Microsoft.ML.Runtime.Data;
using Microsoft.ML.Runtime.FastTree;
using Mvvm;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace XamlBrewer.Uwp.MachineLearningSample.Models
{
    internal class RegressionModel : ViewModelBase
    {
        private LocalEnvironment _mlContext = new LocalEnvironment(seed: null); // v0.6;

        public PredictionModel<RegressionData, RegressionPrediction> Model { get; private set; }

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
            return reader.Read(src).AsEnumerable<RegressionData>(_mlContext, false);
        }

        public void BuildAndTrain(string trainingDataPath)
        {
            // FastTreeRegressionTrainer in v0.6 does not run in debug mode on UWP.
            // https://stackoverflow.com/questions/50829968/ml-net-fails-to-run-on-uwp
            // So we're back to the legacy API

            var pipeline = new LearningPipeline();
            pipeline.Add(new Microsoft.ML.Legacy.Data.TextLoader(trainingDataPath).CreateFrom<RegressionData>(useHeader: true, separator: ';'));
            pipeline.Add(new MissingValueSubstitutor("NBA_DraftNumber") { ReplacementKind = NAReplaceTransformReplacementKind.Mean });
            pipeline.Add(new MissingValueSubstitutor("Age") { ReplacementKind = NAReplaceTransformReplacementKind.Mean });
            pipeline.Add(new MissingValueSubstitutor("Ws") { ReplacementKind = NAReplaceTransformReplacementKind.Mean });
            pipeline.Add(new MissingValueSubstitutor("Bmp") { ReplacementKind = NAReplaceTransformReplacementKind.Mean });
            pipeline.Add(new MinMaxNormalizer("NBA_DraftNumber", "Age", "Ws", "Bmp"));
            pipeline.Add(new ColumnConcatenator("Features",
                                                "NBA_DraftNumber",
                                                "Age",
                                                "Ws",
                                                "Bmp"
                                                ));
            //pipeline.Add(new OnlineGradientDescentRegressor()
            pipeline.Add(new StochasticDualCoordinateAscentRegressor()
                {
                    LabelColumn = "Label", 
                    FeatureColumn = "Features"
                }
            );

            Model = pipeline.Train<RegressionData, RegressionPrediction>();
        }

        public async Task Save(string modelName)
        {
            var storageFolder = ApplicationData.Current.LocalFolder;
            using (var fs = new FileStream(
                    Path.Combine(storageFolder.Path, modelName),
                    FileMode.Create,
                    FileAccess.Write,
                    FileShare.Write))
            {
                await Model.WriteAsync(fs);
            }
        }

        public IEnumerable<RegressionPrediction> Predict(IEnumerable<RegressionData> data)
        {
            return Model.Predict(data);
        }
    }
}
