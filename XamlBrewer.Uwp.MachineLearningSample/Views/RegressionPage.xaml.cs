using Mvvm.Services;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Series;
using System.Linq;
using Windows.UI.Xaml.Controls;
using XamlBrewer.Uwp.MachineLearningSample.Models;
using XamlBrewer.Uwp.MachineLearningSample.ViewModels;

namespace XamlBrewer.Uwp.MachineLearningSample
{
    public sealed partial class RegressionPage : Page
    {
        public RegressionPage()
        {
            this.InitializeComponent();
            this.DataContext = new RegressionPageViewModel();

            Loaded += Page_Loaded;
        }

        private RegressionPageViewModel ViewModel => DataContext as RegressionPageViewModel;

        private async void Page_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            DatasetBox.IsChecked = false;
            SettingUpBox.IsChecked = false;
            TrainingBox.IsChecked = false;
            TestingBox.IsChecked = false;
            PlottingBox.IsChecked = false;
            RestartButton.IsEnabled = false;
            DraftSlider.IsEnabled = false;
            AgeSlider.IsEnabled = false;
            WinsSlider.IsEnabled = false;
            BoxSlider.IsEnabled = false;

            BusyIndicator.Visibility = Windows.UI.Xaml.Visibility.Visible;
            BusyIndicator.PlayAnimation();

            // Prepare the input files
            DatasetBox.IsChecked = true;
            var trainingDataPath = await MlDotNet.FilePath(@"ms-appx:///Data/2017-18_NBA_salary.csv");
            // Read training data
            var trainingData = await ViewModel.Load(trainingDataPath);

            // Configure data transformations.
            SettingUpBox.IsChecked = true;

            // Create and train the model      
            TrainingBox.IsChecked = true;
            await ViewModel.BuildAndTrain();

            // Save the model.
            await ViewModel.Save("regressionModel.zip");

            // Visual evaluation of the model.
            TestingBox.IsChecked = true;
            var predictions = await ViewModel.PredictTrainingData();
            var result = predictions.OrderBy((p) => p.Salary).ToList();

            // Diagram
            PlottingBox.IsChecked = true;
            var foreground = OxyColors.SteelBlue;
            var plotModel = new PlotModel
            {
                PlotAreaBorderThickness = new OxyThickness(1, 0, 0, 1),
                PlotAreaBorderColor = foreground,
                TextColor = foreground,
                TitleColor = foreground,
                SubtitleColor = foreground,
                LegendPosition = LegendPosition.TopCenter,
                LegendOrientation = LegendOrientation.Horizontal
            };

            var axisX = new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Title = "Test Data",
                TextColor = foreground,
                TicklineColor = foreground,
                TitleColor = foreground
            };

            plotModel.Axes.Add(axisX);

            var axisY = new LinearAxis
            {
                Title = "Salary",
                TextColor = foreground,
                TicklineColor = foreground,
                TitleColor = foreground
            };
            plotModel.Axes.Add(axisY);

            var realSeries = new ScatterSeries
            {
                Title = "Real",
                MarkerType = MarkerType.Circle,
                MarkerSize = 2,
                MarkerFill = OxyColors.SteelBlue
            };

            plotModel.Series.Add(realSeries);

            var predictedSeries = new ScatterSeries
            {
                Title = "Predicted",
                MarkerType = MarkerType.Circle,
                MarkerSize = 2,
                MarkerFill = OxyColors.Firebrick
            };

            plotModel.Series.Add(predictedSeries);

            for (int i = 0; i < result.Count; i++)
            {
                realSeries.Points.Add(new ScatterPoint(i, result[i].Salary));
                predictedSeries.Points.Add(new ScatterPoint(i, result[i].Score));
            }

            // Just to put an entry in the Legend.
            var singlePredictionSeries = new ScatterSeries
            {
                Title = "Single Prediction",
                MarkerType = MarkerType.Circle,
                MarkerSize = 2,
                MarkerFill = OxyColors.Green
            };

            plotModel.Series.Add(singlePredictionSeries);

            Diagram.Model = plotModel;

            Slider_ValueChanged(this, null);

            BusyIndicator.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            BusyIndicator.PauseAnimation();
            RestartButton.IsEnabled = true;
            DraftSlider.IsEnabled = true;
            AgeSlider.IsEnabled = true;
            WinsSlider.IsEnabled = true;
            BoxSlider.IsEnabled = true;
        }

        private async void Slider_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            if (DraftSlider == null || AgeSlider == null || WinsSlider == null || BoxSlider == null)
            {
                return;
            }

            // Predict
            var result = await ViewModel.Predict(new RegressionData
            {
                NBA_DraftNumber = (float)DraftSlider.Value,
                Age = (float)AgeSlider.Value,
                Ws = (float)WinsSlider.Value,
                Bmp = (float)BoxSlider.Value
            });

            var annotation = new LineAnnotation
            {
                X = 0,
                Y = result.Score,
                Type = LineAnnotationType.Horizontal,
                Color = OxyColors.Green,
                LineStyle = LineStyle.Solid
            };
            Diagram.Model.Annotations.Clear();
            Diagram.Model.Annotations.Add(annotation);
            Diagram.InvalidatePlot();
        }
    }
}
