using Microsoft.ML.Legacy.Trainers;
using Mvvm.Services;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using Windows.UI.Xaml.Controls;
using XamlBrewer.Uwp.MachineLearningSample.Helpers;

namespace XamlBrewer.Uwp.MachineLearningSample
{
    public sealed partial class BinaryClassificationPage : Page
    {
        public BinaryClassificationPage()
        {
            this.InitializeComponent();
            Loaded += Page_Loaded;
        }

        private async void Page_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            DatasetBox.IsChecked = false;
            PerceptronBox.IsChecked = false;
            LinearSvmBox.IsChecked = false;
            LogisticRegressionBox.IsChecked = false;
            SdcaBox.IsChecked = false;

            // Prepare datasets.
            DatasetBox.IsChecked = true;
            var trainingDataLocation = await MlDotNet.FilePath(@"ms-appx:///Data/winequality_white_train.csv");
            var testDataLocation = await MlDotNet.FilePath(@"ms-appx:///Data/winequality_white_test.csv");

            // Prepare diagram.
            var foreground = OxyColors.LightSteelBlue;
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

            var accuracySeries = new ColumnSeries
            {
                Title = "Accuracy",
                LabelPlacement = LabelPlacement.Inside,
                LabelFormatString = "{0:.00}",
                FillColor = OxyColors.DarkGoldenrod
            };
            plotModel.Series.Add(accuracySeries);

            var entropySeries = new ColumnSeries
            {
                Title = "Entropy",
                LabelPlacement = LabelPlacement.Inside,
                LabelFormatString = "{0:.00}",
                FillColor = OxyColors.Firebrick
            };
            plotModel.Series.Add(entropySeries);

            var f1ScoreSeries = new ColumnSeries
            {
                Title = "F1 Score",
                LabelPlacement = LabelPlacement.Inside,
                LabelFormatString = "{0:.00}",
                FillColor = OxyColors.Teal
            };
            plotModel.Series.Add(f1ScoreSeries);

            Diagram.Model = plotModel;

            var modelEvaluator = new ModelEvaluator();

            // Perceptron
            PerceptronBox.IsChecked = true;
            var perceptronBinaryModel = new ModelBuilder(trainingDataLocation, new AveragedPerceptronBinaryClassifier()).BuildAndTrain();
            var metrics = modelEvaluator.Evaluate(perceptronBinaryModel, testDataLocation);
            accuracySeries.Items.Add(new ColumnItem { CategoryIndex = 0, Value = metrics.Accuracy });
            entropySeries.Items.Add(new ColumnItem { CategoryIndex = 0, Value = metrics.Entropy });
            f1ScoreSeries.Items.Add(new ColumnItem { CategoryIndex = 0, Value = metrics.F1Score });

            //// These raise an exception on System.Diagnostics.Process
            //// 'PlatformNotSupportedException: Retrieving information about local processes is not supported on this platform.'
            ////
            //// var fastForestBinaryModel = new ModelBuilder(trainingDataLocation, new FastForestBinaryClassifier()).BuildAndTrain();
            //// var fastTreeBinaryModel = new ModelBuilder(trainingDataLocation, new FastTreeBinaryClassifier()).BuildAndTrain();

            // Linear SVM
            LinearSvmBox.IsChecked = true;
            var linearSvmModel = new ModelBuilder(trainingDataLocation, new LinearSvmBinaryClassifier()).BuildAndTrain();
            metrics = modelEvaluator.Evaluate(linearSvmModel, testDataLocation);
            accuracySeries.Items.Add(new ColumnItem { CategoryIndex = 1, Value = metrics.Accuracy });
            entropySeries.Items.Add(new ColumnItem { CategoryIndex = 1, Value = metrics.Entropy });
            f1ScoreSeries.Items.Add(new ColumnItem { CategoryIndex = 1, Value = metrics.F1Score });

            // Logistic Regression
            LogisticRegressionBox.IsChecked = true;
            var logisticRegressionModel = new ModelBuilder(trainingDataLocation, new LogisticRegressionBinaryClassifier()).BuildAndTrain();
            metrics = modelEvaluator.Evaluate(logisticRegressionModel, testDataLocation);
            accuracySeries.Items.Add(new ColumnItem { CategoryIndex = 2, Value = metrics.Accuracy });
            entropySeries.Items.Add(new ColumnItem { CategoryIndex = 2, Value = metrics.Entropy });
            f1ScoreSeries.Items.Add(new ColumnItem { CategoryIndex = 2, Value = metrics.F1Score });

            // Stochastic Dual Coordinate Ascent
            SdcaBox.IsChecked = true;
            var sdcabModel = new ModelBuilder(trainingDataLocation, new StochasticDualCoordinateAscentBinaryClassifier()).BuildAndTrain();
            metrics = modelEvaluator.Evaluate(sdcabModel, testDataLocation);
            accuracySeries.Items.Add(new ColumnItem { CategoryIndex = 3, Value = metrics.Accuracy });
            entropySeries.Items.Add(new ColumnItem { CategoryIndex = 3, Value = metrics.Entropy });
            f1ScoreSeries.Items.Add(new ColumnItem { CategoryIndex = 3, Value = metrics.F1Score });

            // Update diagram
            plotModel.InvalidatePlot(true);
        }

        private void Calculate_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
        }
    }
}
