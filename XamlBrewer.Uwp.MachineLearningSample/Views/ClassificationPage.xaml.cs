using Microsoft.ML.Legacy;
using Microsoft.ML.Legacy.Models;
using Microsoft.ML.Legacy.Trainers;
using Microsoft.ML.Legacy.Transforms;
using Microsoft.ML.Runtime.Data;
// using Microsoft.ML.Runtime.Data;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using XamlBrewer.Uwp.MachineLearningSample.Models;
using TextLoader = Microsoft.ML.Legacy.Data.TextLoader; // !!! There's more than one TextLoader

namespace XamlBrewer.Uwp.MachineLearningSample
{
    public sealed partial class ClassificationPage : Page
    {
        private PredictionModel<ClassificationData, ClassPrediction> _model;
        private LocalEnvironment _mlContext;
        private List<OxyColor> _colors = new List<OxyColor>
                {
                    OxyColors.Black,
                    OxyColors.Red,
                    OxyColors.Yellow,
                    OxyColors.White,
                    OxyColors.Green,
                    OxyColors.Blue,
                    OxyColors.HotPink
                };

        public ClassificationPage()
        {
            this.InitializeComponent();
            Loaded += ClusteringPage_Loaded;
        }

        private async void ClusteringPage_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            // Dataset:
            // https://www.kaggle.com/ankiijindae/car-evaluation-classification/data

            // https://github.com/dotnet/machinelearning-samples/commit/6a939dd2d80d782220bf19def6902188bf4062a1#diff-e19afc69620b2166b1ce15dba6eb9422
            // https://docs.microsoft.com/en-us/dotnet/machine-learning/resources/transforms
            // https://docs.microsoft.com/en-us/dotnet/api/microsoft.ml.legacy.ilearningpipelineitem?view=ml-dotnet
            // https://www.codeproject.com/Articles/1249611/%2fArticles%2f1249611%2fMachine-Learning-with-ML-Net-and-Csharp-VB-Net

            SettingUpBox.IsChecked = false;
            LoadingBox.IsChecked = false;
            TrainingBox.IsChecked = false;
            TestingBox.IsChecked = false;
            PlottingBox.IsChecked = false;

            //Create the MLContext
            SettingUpBox.IsChecked = true;

            //Load training data.
            LoadingBox.IsChecked = true;
            StorageFile modelFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri(@"ms-appx:///Assets/training.tsv"));
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            StorageFile sf2 = await modelFile.CopyAsync(storageFolder, modelFile.Name, NameCollisionOption.ReplaceExisting);

            // Configure data transformations.
            // LearningPipeline allows you to add steps in order to keep everything together 
            // during the learning process.  
            var pipeline = new LearningPipeline();

            // The TextLoader loads a dataset with comments and corresponding postive or negative sentiment. 
            // When you create a loader, you specify the schema by passing a class to the loader containing
            // all the column names and their types. This is used to create the model, and train it. 
            pipeline.Add(new TextLoader(sf2.Path).CreateFrom<ClassificationData>());

            // Create buckets.
            pipeline.Add(new Dictionarizer("Label"));

            // TextFeaturizer is a transform that is used to featurize an input column. 
            // This is used to format and clean the data.
            pipeline.Add(new TextFeaturizer("Features", "Text"));

            pipeline.Add(new StochasticDualCoordinateAscentClassifier());
            //pipeline.Add(new LogisticRegressionClassifier());
            //pipeline.Add(new NaiveBayesClassifier());

            pipeline.Add(new PredictedLabelColumnOriginalValueConverter() { PredictedLabelColumn = "PredictedLabel" });

            // Create and train the model      
            TrainingBox.IsChecked = true;
            // Train the pipeline based on the dataset that has been loaded, transformed.
            _model = pipeline.Train<ClassificationData, ClassPrediction>();

            // Could save the model here.

            // Test the model
            TestingBox.IsChecked = true;
            // loads the new test dataset with the same schema.
            // You can evaluate the model using this dataset as a quality check.
            StorageFile testFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri(@"ms-appx:///Assets/test.tsv"));
            StorageFile sf2b = await modelFile.CopyAsync(storageFolder, testFile.Name, NameCollisionOption.ReplaceExisting);

            var testData = new TextLoader(sf2b.Path).CreateFrom<ClassificationData>();

            // Computes the quality metrics for the PredictionModel using the specified dataset.
            var evaluator = new ClassificationEvaluator();

            // The BinaryClassificationMetrics contains the overall metrics computed by binary
            // classification evaluators. To display these to determine the quality of the model,
            // you need to get the metrics first.
            var metrics = evaluator.Evaluate(_model, testData);

            // Diagram
            PlottingBox.IsChecked = true;
            var foreground = OxyColors.LightSteelBlue;
            var plotModel = new PlotModel { Title = "Model Quality", Subtitle = "Logarithmic Loss per Class" };

            var bars = new List<BarItem>();
            foreach (var logloss in metrics.PerClassLogLoss)
            {
                bars.Add(new BarItem { Value = logloss });
            }

            var barSeries = new BarSeries
            {
                ItemsSource = bars,
                LabelPlacement = LabelPlacement.Inside,
                LabelFormatString = "{0:.0000}",
                FillColor = OxyColors.DarkCyan
            };
            plotModel.Series.Add(barSeries);

            plotModel.Axes.Add(new CategoryAxis
            {
                Position = AxisPosition.Left,
                Key = "LogLossAxis",
                ItemsSource = new[]
                    {
                        "German",
                        "English",
                        "French",
                        "Italian",
                        "Romanian",
                        "Spanish"
                    },
                TextColor = foreground,
                TicklineColor = foreground,
                TitleColor = foreground
            });

            var linearAxisX = new LinearAxis() { Position = AxisPosition.Bottom }; ;
            linearAxisX.TextColor = foreground;
            linearAxisX.TicklineColor = foreground;
            linearAxisX.TitleColor = foreground;
            plotModel.Axes.Add(linearAxisX);

            plotModel.PlotAreaBorderThickness = new OxyThickness(1, 0, 0, 1);
            plotModel.PlotAreaBorderColor = foreground;
            plotModel.TextColor = foreground;
            plotModel.TitleColor = foreground;
            plotModel.SubtitleColor = foreground;

            Diagram.Model = plotModel;
        }

        private void Calculate_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            // Predict
            var result = _model.Predict(new List<ClassificationData> {
                new ClassificationData
                {
                    Text = TextInput.Text
                }
            }).First();

            TextPrediction.Text = result.PredictedLanguage;
        }
    }
}
