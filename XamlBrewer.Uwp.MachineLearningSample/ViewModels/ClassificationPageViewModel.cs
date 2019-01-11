using Microsoft.ML.Legacy;
using Microsoft.ML.Legacy.Models;
using Microsoft.ML.Legacy.Trainers;
using Microsoft.ML.Legacy.Transforms;
using Mvvm;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using XamlBrewer.Uwp.MachineLearningSample.Models;
using TextLoader = Microsoft.ML.Legacy.Data.TextLoader; // !!! There's more than one TextLoader

namespace XamlBrewer.Uwp.MachineLearningSample.ViewModels
{
    internal class ClassificationPageViewModel : ViewModelBase
    {
        public LearningPipeline Pipeline { get; private set; }

        public PredictionModel<MulticlassClassificationData, MulticlassClassificationPrediction> Model { get; private set; }

        public Task Build(string trainingDataPath)
        {
            return Task.Run(() =>
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
            });
        }

        public Task Train()
        {
            return Task.Run(() =>
            {
                Model = Pipeline.Train<MulticlassClassificationData, MulticlassClassificationPrediction>();
            });
        }

        public Task Save(string modelName)
        {
            return Task.Run(() =>
            {
                var storageFolder = ApplicationData.Current.LocalFolder;
                using (var fs = new FileStream(Path.Combine(storageFolder.Path, modelName), FileMode.Create, FileAccess.Write, FileShare.Write))
                    Model.WriteAsync(fs);
            });
        }

        public Task<ClassificationMetrics> Evaluate(string testDataPath)
        {
            return Task.Run(() =>
            {
                var testData = new TextLoader(testDataPath).CreateFrom<MulticlassClassificationData>();

                // Computes the quality metrics for the PredictionModel using the specified dataset.
                var evaluator = new ClassificationEvaluator();
                var metrics = evaluator.Evaluate(Model, testData);
                return metrics;
            });
        }

        public Task<MulticlassClassificationPrediction> Predict(string text)
        {
            return Task.Run(() =>
            {
                return Model.Predict(new List<MulticlassClassificationData>
                {
                    new MulticlassClassificationData
                    {
                        Text = text
                    }
                }).First();
            });
        }
    }
}
