using Microsoft.ML.Legacy;
using Microsoft.ML.Legacy.Models;
using Microsoft.ML.Legacy.Trainers;
using Microsoft.ML.Legacy.Transforms;
using Mvvm;
using System.IO;
using Windows.Storage;
using TextLoader = Microsoft.ML.Legacy.Data.TextLoader; // !!! This is the old TextLoader

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

            // Create a dictionary for the languages. (no pun intended)
            Pipeline.Add(new Dictionarizer("Label"));

            // Transform the text into a feature vector.
            Pipeline.Add(new TextFeaturizer("Features", "Text"));

            // Main algorithm
            Pipeline.Add(new StochasticDualCoordinateAscentClassifier());
            // or
            // Pipeline.Add(new LogisticRegressionClassifier());
            // or
            // Pipeline.Add(new NaiveBayesClassifier()); // yields weird metrics...

            // Convert the predicted value back into a language.
            Pipeline.Add(new PredictedLabelColumnOriginalValueConverter() { PredictedLabelColumn = "PredictedLabel" });
        }

        public void Train()
        {
            Model = Pipeline.Train<MulticlassClassificationData, MulticlassClassificationPrediction>();
        }

        public void Save(string modelName)
        {
            var storageFolder = ApplicationData.Current.LocalFolder;
            using (var fs = new FileStream(
                Path.Combine(storageFolder.Path, modelName),
                FileMode.Create,
                FileAccess.Write,
                FileShare.Write))
                Model.WriteAsync(fs);
        }

        public ClassificationMetrics Evaluate(string testDataPath)
        {
            var testData = new TextLoader(testDataPath).CreateFrom<MulticlassClassificationData>();

            var evaluator = new ClassificationEvaluator();
            return evaluator.Evaluate(Model, testData);
        }

        public MulticlassClassificationPrediction Predict(string text)
        {
            return Model.Predict(new MulticlassClassificationData(text));
        }
    }
}
