using Microsoft.ML.Legacy.Trainers;
using Mvvm.Services;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using Windows.UI.Xaml.Controls;
using XamlBrewer.Uwp.MachineLearningSample.ViewModels;

namespace XamlBrewer.Uwp.MachineLearningSample
{
    public sealed partial class BinaryClassificationPage : Page
    {
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

            // Prepare datasets.
            DatasetBox.IsChecked = true;
            var trainingDataLocation = await MlDotNet.FilePath(@"ms-appx:///Data/winequality_white_train.csv");
            var testDataLocation = await MlDotNet.FilePath(@"ms-appx:///Data/winequality_white_test.csv");

            // Prepare diagram.
            PrepareDiagram(out ColumnSeries accuracySeries, out ColumnSeries entropySeries, out ColumnSeries f1ScoreSeries);

            // Perceptron
            PerceptronBox.IsChecked = true;
            var perceptronBinaryModel = await ViewModel.BuildAndTrain(trainingDataLocation, new AveragedPerceptronBinaryClassifier());
            var metrics = await ViewModel.Evaluate(perceptronBinaryModel, testDataLocation);
            accuracySeries.Items.Add(new ColumnItem { CategoryIndex = 0, Value = metrics.Accuracy });
            entropySeries.Items.Add(new ColumnItem { CategoryIndex = 0, Value = metrics.Entropy });
            f1ScoreSeries.Items.Add(new ColumnItem { CategoryIndex = 0, Value = metrics.F1Score });

            // Update diagram
            Diagram.InvalidatePlot();

            //// These raise an exception on System.Diagnostics.Process
            //// 'PlatformNotSupportedException: Retrieving information about local processes is not supported on this platform.'
            ////
            //// var fastForestBinaryModel = new ModelBuilder(trainingDataLocation, new FastForestBinaryClassifier()).BuildAndTrain();
            //// var fastTreeBinaryModel = new ModelBuilder(trainingDataLocation, new FastTreeBinaryClassifier()).BuildAndTrain();

            // Linear SVM
            LinearSvmBox.IsChecked = true;
            var linearSvmModel = await ViewModel.BuildAndTrain(trainingDataLocation, new LinearSvmBinaryClassifier());
            metrics = await ViewModel.Evaluate(linearSvmModel, testDataLocation);
            accuracySeries.Items.Add(new ColumnItem { CategoryIndex = 1, Value = metrics.Accuracy });
            entropySeries.Items.Add(new ColumnItem { CategoryIndex = 1, Value = metrics.Entropy });
            f1ScoreSeries.Items.Add(new ColumnItem { CategoryIndex = 1, Value = metrics.F1Score });

            // Update diagram
            Diagram.InvalidatePlot();

            // Logistic Regression
            LogisticRegressionBox.IsChecked = true;
            var logisticRegressionModel = await ViewModel.BuildAndTrain(trainingDataLocation, new LogisticRegressionBinaryClassifier());
            metrics = await ViewModel.Evaluate(logisticRegressionModel, testDataLocation);
            accuracySeries.Items.Add(new ColumnItem { CategoryIndex = 2, Value = metrics.Accuracy });
            entropySeries.Items.Add(new ColumnItem { CategoryIndex = 2, Value = metrics.Entropy });
            f1ScoreSeries.Items.Add(new ColumnItem { CategoryIndex = 2, Value = metrics.F1Score });

            // Update diagram
            Diagram.InvalidatePlot();

            // Stochastic Dual Coordinate Ascent
            SdcaBox.IsChecked = true;
            var sdcabModel = await ViewModel.BuildAndTrain(trainingDataLocation, new StochasticDualCoordinateAscentBinaryClassifier());
            metrics = await ViewModel.Evaluate(sdcabModel, testDataLocation);
            accuracySeries.Items.Add(new ColumnItem { CategoryIndex = 3, Value = metrics.Accuracy });
            entropySeries.Items.Add(new ColumnItem { CategoryIndex = 3, Value = metrics.Entropy });
            f1ScoreSeries.Items.Add(new ColumnItem { CategoryIndex = 3, Value = metrics.F1Score });

            // Update diagram
            Diagram.InvalidatePlot();
        }

        private void PrepareDiagram(out ColumnSeries accuracySeries, out ColumnSeries entropySeries, out ColumnSeries f1ScoreSeries)
        {
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

            accuracySeries = new ColumnSeries
            {
                Title = "Accuracy",
                LabelPlacement = LabelPlacement.Inside,
                LabelFormatString = "{0:.00}",
                FillColor = OxyColors.DarkGoldenrod
            };
            plotModel.Series.Add(accuracySeries);

            entropySeries = new ColumnSeries
            {
                Title = "Entropy",
                LabelPlacement = LabelPlacement.Inside,
                LabelFormatString = "{0:.00}",
                FillColor = OxyColors.Firebrick
            };
            plotModel.Series.Add(entropySeries);

            f1ScoreSeries = new ColumnSeries
            {
                Title = "F1 Score",
                LabelPlacement = LabelPlacement.Inside,
                LabelFormatString = "{0:.00}",
                FillColor = OxyColors.Teal
            };
            plotModel.Series.Add(f1ScoreSeries);

            Diagram.Model = plotModel;
        }

        private void Calculate_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
        }
    }
}
