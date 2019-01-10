using Microsoft.ML.Legacy;
using Microsoft.ML.Legacy.Data;
using Microsoft.ML.Legacy.Models;
using Microsoft.ML.Legacy.Transforms;
using Mvvm;
using System.Threading.Tasks;
using XamlBrewer.Uwp.MachineLearningSample.Models;

namespace XamlBrewer.Uwp.MachineLearningSample.ViewModels
{
    internal class BinaryClassificationPageViewModel : ViewModelBase
    {
        public Task<PredictionModel<BinaryClassificationData, BinaryClassificationPrediction>> BuildAndTrain(string trainingDataPath, ILearningPipelineItem algorithm)
        {
            return Task.Run(() =>
            {
                var pipeline = new LearningPipeline();
                pipeline.Add(new TextLoader(trainingDataPath).CreateFrom<BinaryClassificationData>(useHeader: true, separator: ';'));
                pipeline.Add(new MissingValueSubstitutor("FixedAcidity") { ReplacementKind = NAReplaceTransformReplacementKind.Mean });
                pipeline.Add(MakeNormalizer());
                pipeline.Add(new ColumnConcatenator("Features",
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
                                                     "Alcohol"));
                pipeline.Add(algorithm);

                return pipeline.Train<BinaryClassificationData, BinaryClassificationPrediction>();
            });
        }

        public Task<BinaryClassificationMetrics> Evaluate(PredictionModel<BinaryClassificationData, BinaryClassificationPrediction> model, string testDataLocation)
        {
            return Task.Run(() =>
            {
                var testData = new TextLoader(testDataLocation).CreateFrom<BinaryClassificationData>(useHeader: true, separator: ';');
                var metrics = new BinaryClassificationEvaluator().Evaluate(model, testData);
                return metrics;
            });
        }

        private ILearningPipelineItem MakeNormalizer()
        {
            var normalizer = new BinNormalizer
            {
                NumBins = 2
            };
            normalizer.AddColumn("Label");

            return normalizer;
        }
    }
}
