using Microsoft.ML.Legacy;
using Microsoft.ML.Legacy.Models;
using Microsoft.ML.Legacy.Trainers;
using Microsoft.ML.Legacy.Transforms;
using Mvvm.Services;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Controls;
using XamlBrewer.Uwp.MachineLearningSample.Models;
using TextLoader = Microsoft.ML.Legacy.Data.TextLoader; // !!! There's more than one TextLoader

namespace XamlBrewer.Uwp.MachineLearningSample
{
    public sealed partial class ClassificationPage : Page
    {
        private PredictionModel<MulticlassClassificationData, MulticlassClassificationPrediction> _model;
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
            DatasetBox.IsChecked = false;    
            SettingUpBox.IsChecked = false;
            TrainingBox.IsChecked = false;
            TestingBox.IsChecked = false;
            PlottingBox.IsChecked = false;

            // Prepare the input files
            DatasetBox.IsChecked = true;
            var trainingDataPath = await MlDotNet.FilePath(@"ms-appx:///Data/training.tsv");
            var testDataPath = await MlDotNet.FilePath(@"ms-appx:///Data/test.tsv");

            // Configure data transformations.
            SettingUpBox.IsChecked = true;
            var pipeline = new LearningPipeline();
            pipeline.Add(new TextLoader(trainingDataPath).CreateFrom<MulticlassClassificationData>());

            // Create buckets.
            pipeline.Add(new Dictionarizer("Label"));

            // Transform the text into a feature vector.
            pipeline.Add(new TextFeaturizer("Features", "Text"));

            pipeline.Add(new StochasticDualCoordinateAscentClassifier());

            // Alternative algorithms:
            //pipeline.Add(new LogisticRegressionClassifier());
            //pipeline.Add(new NaiveBayesClassifier());

            pipeline.Add(new PredictedLabelColumnOriginalValueConverter() { PredictedLabelColumn = "PredictedLabel" });

            // Create and train the model      
            TrainingBox.IsChecked = true;
            _model = pipeline.Train<MulticlassClassificationData, MulticlassClassificationPrediction>();

            // Could save the model here.
            // ...

            // Test and evaluate the model
            TestingBox.IsChecked = true;
            var testData = new TextLoader(testDataPath).CreateFrom<MulticlassClassificationData>();

            // Computes the quality metrics for the PredictionModel using the specified dataset.
            var evaluator = new ClassificationEvaluator();
            var metrics = evaluator.Evaluate(_model, testData);

            // Diagram
            PlottingBox.IsChecked = true;
            var foreground = OxyColors.LightSteelBlue;
            var plotModel = new PlotModel
            {
                Subtitle = "Model Quality",
                PlotAreaBorderThickness = new OxyThickness(1, 0, 0, 1),
                PlotAreaBorderColor = foreground,
                TextColor = foreground,
                TitleColor = foreground,
                SubtitleColor = foreground
            };

            var bars = new List<BarItem>();
            foreach (var logloss in metrics.PerClassLogLoss)
            {
                bars.Add(new BarItem { Value = logloss });
            }

            var barSeries = new BarSeries
            {
                ItemsSource = bars,
                LabelPlacement = LabelPlacement.Inside,
                LabelFormatString = "{0:0.00} ",
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

            var linearAxisX = new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Title = "Logarithmic loss per class (lower is better)",
                TextColor = foreground,
                TicklineColor = foreground,
                TitleColor = foreground
            };
            plotModel.Axes.Add(linearAxisX);

            Diagram.Model = plotModel;
        }

        private void Calculate_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            // Predict
            var result = _model.Predict(new List<MulticlassClassificationData> {
                new MulticlassClassificationData
                {
                    Text = TextInput.Text
                }
            }).First();

            TextPrediction.Text = string.Format("{1}% sure this is {0}.", result.PredictedLanguage, result.Confidence);
        }
    }
}
