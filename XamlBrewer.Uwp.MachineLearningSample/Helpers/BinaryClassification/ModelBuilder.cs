using Microsoft.ML.Legacy;
using Microsoft.ML.Legacy.Data;
using Microsoft.ML.Legacy.Transforms;
using XamlBrewer.Uwp.MachineLearningSample.Models;

namespace XamlBrewer.Uwp.MachineLearningSample.Helpers
{
    public class ModelBuilder
    {
        private readonly string _trainingDataLocation;
        private readonly ILearningPipelineItem _algorythm;

        public ModelBuilder(string trainingDataLocation, ILearningPipelineItem algorythm)
        {
            _trainingDataLocation = trainingDataLocation;
            _algorythm = algorythm;
        }

        /// <summary>
        /// Using training data location that is passed trough constructor this method is building
        /// and training machine learning model.
        /// </summary>
        /// <returns>Trained machine learning model.</returns>
        public PredictionModel<BinaryClassificationData, BinaryClassificationPrediction> BuildAndTrain()
        {
            var pipeline = new LearningPipeline();
            pipeline.Add(new TextLoader(_trainingDataLocation).CreateFrom<BinaryClassificationData>(useHeader: true, separator: ';'));
            pipeline.Add(new MissingValueSubstitutor("FixedAcidity") { ReplacementKind = NAReplaceTransformReplacementKind.Mean});
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
            pipeline.Add(_algorythm);

            return pipeline.Train<BinaryClassificationData, BinaryClassificationPrediction>();
        }

        private ILearningPipelineItem MakeNormalizer()
        {
            var normalizer = new BinNormalizer();
            normalizer.NumBins = 2;
            normalizer.AddColumn("Label");

            return normalizer;
        }
    }
}
