using Microsoft.ML.Legacy;
using Microsoft.ML.Legacy.Models;
using Microsoft.ML.Legacy.Trainers;
using Microsoft.ML.Legacy.Transforms;
using Mvvm;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Storage;
using TextLoader = Microsoft.ML.Legacy.Data.TextLoader; // !!! There's more than one TextLoader

namespace XamlBrewer.Uwp.MachineLearningSample.Models
{
    internal class MulticlassClassificationModel : ViewModelBase
    {
        public LearningPipeline Pipeline { get; private set; }

        public PredictionModel<MulticlassClassificationData, MulticlassClassificationPrediction> Model { get; private set; }

        public void Build(string trainingDataPath)
        {
            Pipeline = new LearningPipeline();
            Pipeline.Add(new TextLoader(trainingDataPath).CreateFrom<MulticlassClassificationData>());

            // Create buckets.
            Pipeline.Add(new Dictionarizer("Label"));

            // Transform the text into a feature vector.
            Pipeline.Add(new TextFeaturizer("Features", "Text"));

            Pipeline.Add(new StochasticDualCoordinateAscentClassifier());

            // Alternative algorithms:
            //Pipeline.Add(new LogisticRegressionClassifier());
            //Pipeline.Add(new NaiveBayesClassifier());

            Pipeline.Add(new PredictedLabelColumnOriginalValueConverter() { PredictedLabelColumn = "PredictedLabel" });
        }

        public void Train()
        {
            Model = Pipeline.Train<MulticlassClassificationData, MulticlassClassificationPrediction>();
        }

        public void Save(string modelName)
        {
            var storageFolder = ApplicationData.Current.LocalFolder;
            using (var fs = new FileStream(Path.Combine(storageFolder.Path, modelName), FileMode.Create, FileAccess.Write, FileShare.Write))
                Model.WriteAsync(fs);
        }

        public ClassificationMetrics Evaluate(string testDataPath)
        {
            var testData = new TextLoader(testDataPath).CreateFrom<MulticlassClassificationData>();

            // Computes the quality metrics for the PredictionModel using the specified dataset.
            var evaluator = new ClassificationEvaluator();
            var metrics = evaluator.Evaluate(Model, testData);
            return metrics;
        }

        public MulticlassClassificationPrediction Predict(string text)
        {
            return Model.Predict(new List<MulticlassClassificationData>
                {
                    new MulticlassClassificationData
                    {
                        Text = text
                    }
                }).First();
        }
    }
}
