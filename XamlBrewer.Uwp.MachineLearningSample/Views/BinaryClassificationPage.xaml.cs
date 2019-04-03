using Microsoft.ML;
using Mvvm.Services;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Linq;
using Windows.UI.Xaml.Controls;
using XamlBrewer.Uwp.MachineLearningSample.Models;
using XamlBrewer.Uwp.MachineLearningSample.ViewModels;

namespace XamlBrewer.Uwp.MachineLearningSample
{
    public sealed partial class BinaryClassificationPage : Page
    {
        private string _testDataPath;
        //private PredictionModel<BinaryClassificationData, BinaryClassificationPrediction> _priorModel;
        private PredictionModel<BinaryClassificationData, BinaryClassificationPrediction> _perceptronBinaryModel;
        private PredictionModel<BinaryClassificationData, BinaryClassificationPrediction> _linearSvmModel;
        private PredictionModel<BinaryClassificationData, BinaryClassificationPrediction> _logisticRegressionModel;
        private PredictionModel<BinaryClassificationData, BinaryClassificationPrediction> _sdcabModel;

        public BinaryClassificationPage()
        {
            this.InitializeComponent();
            this.DataContext = new BinaryClassificationPageViewModel();

            Loaded += Page_Loaded;
        }

        private BinaryClassificationPageViewModel ViewModel => DataContext as BinaryClassificationPageViewModel;

        private async void Page_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            DatasetBox.IsChecked = false;
            PerceptronBox.IsChecked = false;
            LinearSvmBox.IsChecked = false;
            LogisticRegressionBox.IsChecked = false;
            SdcaBox.IsChecked = false;
            StartButton.IsEnabled = false;
            CalculateButton.IsEnabled = false;

            BusyIndicator.Visibility = Windows.UI.Xaml.Visibility.Visible;
            BusyIndicator.PlayAnimation();

            // Prepare datasets.
            DatasetBox.IsChecked = true;
            var trainingDataLocation = await MlDotNet.FilePath(@"ms-appx:///Data/winequality_white_train.csv");
            _testDataPath = await MlDotNet.FilePath(@"ms-appx:///Data/winequality_white_test.csv");

            // Prepare diagram.
            PrepareDiagram(out ColumnSeries accuracySeries, out ColumnSeries entropySeries, out ColumnSeries f1ScoreSeries);

            // Prior
            // blocked by https://github.com/dotnet/machinelearning/issues/3119
            //_priorModel = await ViewModel.BuildAndTrain(trainingDataLocation, ViewModel.MLContext.BinaryClassification.Trainers.Prior());
            //await ViewModel.Save(_priorModel, "priorModel.zip");
            //var metrics = await ViewModel.Evaluate(_priorModel, _testDataPath);
            //accuracySeries.Items.Add(new ColumnItem { CategoryIndex = 0, Value = metrics.Accuracy });
            //entropySeries.Items.Add(new ColumnItem { CategoryIndex = 0, Value = metrics.Entropy });
            //f1ScoreSeries.Items.Add(new ColumnItem { CategoryIndex = 0, Value = metrics.F1Score });

            // Perceptron
            PerceptronBox.IsChecked = true;
            _perceptronBinaryModel = await ViewModel.BuildAndTrain(trainingDataLocation, ViewModel.MLContext.BinaryClassification.Trainers.AveragedPerceptron());
            await ViewModel.Save(_perceptronBinaryModel, "perceptronModel.zip");
            var nonCalibratedMetrics = await ViewModel.EvaluateNonCalibrated(_perceptronBinaryModel, _testDataPath);
            accuracySeries.Items.Add(new ColumnItem { CategoryIndex = 1, Value = nonCalibratedMetrics.Accuracy });
            entropySeries.Items.Add(new ColumnItem { CategoryIndex = 1, Value = double.NaN });// metrics.Entropy });
            f1ScoreSeries.Items.Add(new ColumnItem { CategoryIndex = 1, Value = nonCalibratedMetrics.F1Score });

            // Update diagram
            Diagram.InvalidatePlot();

            //// These raise an exception on System.Diagnostics.Process
            //// 'PlatformNotSupportedException: Retrieving information about local processes is not supported on this platform.'
            ////
            //// var fastForestBinaryModel = new ModelBuilder(trainingDataLocation, new FastForestBinaryClassifier()).BuildAndTrain();
            //// var fastTreeBinaryModel = new ModelBuilder(trainingDataLocation, new FastTreeBinaryClassifier()).BuildAndTrain();

            // Linear SVM
            LinearSvmBox.IsChecked = true;
            _linearSvmModel = await ViewModel.BuildAndTrain(trainingDataLocation, ViewModel.MLContext.BinaryClassification.Trainers.LinearSvm());
            await ViewModel.Save(_linearSvmModel, "linearSvmModel.zip");
            nonCalibratedMetrics = await ViewModel.EvaluateNonCalibrated(_linearSvmModel, _testDataPath);
            accuracySeries.Items.Add(new ColumnItem { CategoryIndex = 2, Value = nonCalibratedMetrics.Accuracy });
            entropySeries.Items.Add(new ColumnItem { CategoryIndex = 2, Value = double.NaN });// metrics.Entropy });
            f1ScoreSeries.Items.Add(new ColumnItem { CategoryIndex = 2, Value = nonCalibratedMetrics.F1Score });

            // Update diagram
            Diagram.InvalidatePlot();

            // Logistic Regression
            LogisticRegressionBox.IsChecked = true;
            _logisticRegressionModel = await ViewModel.BuildAndTrain(trainingDataLocation, ViewModel.MLContext.BinaryClassification.Trainers.LbfgsLogisticRegression());
            await ViewModel.Save(_logisticRegressionModel, "logisticRegressionModel.zip");
            var metrics = await ViewModel.Evaluate(_logisticRegressionModel, _testDataPath);
            accuracySeries.Items.Add(new ColumnItem { CategoryIndex = 3, Value = metrics.Accuracy });
            entropySeries.Items.Add(new ColumnItem { CategoryIndex = 3, Value = metrics.Entropy });
            f1ScoreSeries.Items.Add(new ColumnItem { CategoryIndex = 3, Value = metrics.F1Score });

            // Update diagram
            Diagram.InvalidatePlot();

            // Stochastic Dual Coordinate Ascent
            SdcaBox.IsChecked = true;
            _sdcabModel = await ViewModel.BuildAndTrain(trainingDataLocation, ViewModel.MLContext.BinaryClassification.Trainers.SdcaLogisticRegression());
            await ViewModel.Save(_sdcabModel, "sdcabModel.zip");
            metrics = await ViewModel.Evaluate(_sdcabModel, _testDataPath);
            accuracySeries.Items.Add(new ColumnItem { CategoryIndex = 4, Value = metrics.Accuracy });
            entropySeries.Items.Add(new ColumnItem { CategoryIndex = 4, Value = metrics.Entropy });
            f1ScoreSeries.Items.Add(new ColumnItem { CategoryIndex = 4, Value = metrics.F1Score });

            // Update diagram
            Diagram.InvalidatePlot();

            StartButton.IsEnabled = true;
            CalculateButton.IsEnabled = true;

            BusyIndicator.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            BusyIndicator.PauseAnimation();
        }

        private void PrepareDiagram(out ColumnSeries accuracySeries, out ColumnSeries entropySeries, out ColumnSeries f1ScoreSeries)
        {
            var foreground = OxyColors.SteelBlue;
            var plotModel = new PlotModel
            {
                Subtitle = "Model Comparison",
                PlotAreaBorderThickness = new OxyThickness(1, 0, 0, 1),
                PlotAreaBorderColor = foreground,
                TextColor = foreground,
                TitleColor = foreground,
                SubtitleColor = foreground,
                LegendPlacement = LegendPlacement.Outside,
                LegendPosition = LegendPosition.TopCenter,
                LegendOrientation = LegendOrientation.Horizontal
            };
            plotModel.Axes.Add(new CategoryAxis
            {
                Position = AxisPosition.Bottom,
                Key = "ModelAxis",
                ItemsSource = new[]
                    {
                        "Prior",
                        "Perceptron",
                        "Linear SVM",
                        "Logistic Regression",
                        "SDCA"
                    },
                TextColor = foreground,
                TicklineColor = foreground,
                TitleColor = foreground
            });

            var linearAxis = new LinearAxis
            {
                Position = AxisPosition.Left,
                TextColor = foreground,
                TicklineColor = foreground,
                TitleColor = foreground
            };
            plotModel.Axes.Add(linearAxis);

            accuracySeries = new ColumnSeries
            {
                Title = "Accuracy",
                LabelPlacement = LabelPlacement.Inside,
                LabelFormatString = "{0:.00}",
                FillColor = OxyColors.DarkOrange,
                TextColor = OxyColors.Wheat
            };
            plotModel.Series.Add(accuracySeries);

            entropySeries = new ColumnSeries
            {
                Title = "Entropy",
                LabelPlacement = LabelPlacement.Inside,
                LabelFormatString = "{0:.00}",
                FillColor = OxyColors.Firebrick,
                TextColor = OxyColors.Wheat
            };
            plotModel.Series.Add(entropySeries);

            f1ScoreSeries = new ColumnSeries
            {
                Title = "F1 Score",
                LabelPlacement = LabelPlacement.Inside,
                LabelFormatString = "{0:.00}",
                FillColor = OxyColors.MidnightBlue,
                TextColor = OxyColors.Wheat
            };
            plotModel.Series.Add(f1ScoreSeries);

            Diagram.Model = plotModel;
        }

        private async void Calculate_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Votes.Text = string.Empty;

            var testDataLocation = await MlDotNet.FilePath(@"ms-appx:///Data/winequality_white_test.csv");
            var tests = await ViewModel.GetSample(testDataLocation);
            var size = 50;
            var data = tests.ToList().Take(size);

            var perceptronPrediction = (await ViewModel.Predict(_perceptronBinaryModel, data)).ToList();
            var linearSvmPrediction = (await ViewModel.Predict(_linearSvmModel, data)).ToList();
            var logisticRegressionPrediction = (await ViewModel.Predict(_logisticRegressionModel, data)).ToList();
            var sdcabPrediction = (await ViewModel.Predict(_sdcabModel, data)).ToList();

            for (int i = 0; i < size; i++)
            {
                var vote = perceptronPrediction[i].LabelAsNumber +
                           linearSvmPrediction[i].LabelAsNumber +
                           logisticRegressionPrediction[i].LabelAsNumber +
                           sdcabPrediction[i].LabelAsNumber;

                if (vote > 0 && vote < 4)
                {
                    Votes.Text += 
                        i.ToString("000     ") +
                        BoolVisual(perceptronPrediction[i].PredictedLabel) +
                        BoolVisual(linearSvmPrediction[i].PredictedLabel) +
                        BoolVisual(logisticRegressionPrediction[i].PredictedLabel) +
                        BoolVisual(sdcabPrediction[i].PredictedLabel) + Environment.NewLine;
                }
            }
        }

        private string BoolVisual(bool value)
        {
            return value ? " + " : " - ";
        }
    }
}
