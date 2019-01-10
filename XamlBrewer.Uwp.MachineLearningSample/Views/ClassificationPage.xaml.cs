using Microsoft.ML.Legacy;
using Mvvm.Services;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;
using XamlBrewer.Uwp.MachineLearningSample.Models;
using XamlBrewer.Uwp.MachineLearningSample.ViewModels;

namespace XamlBrewer.Uwp.MachineLearningSample
{
    public sealed partial class ClassificationPage : Page
    {
        private PredictionModel<MulticlassClassificationData, MulticlassClassificationPrediction> _model;

        public ClassificationPage()
        {
            this.InitializeComponent();
            this.DataContext = new ClassificationPageViewModel();

            Loaded += Page_Loaded;
        }

        private ClassificationPageViewModel ViewModel => DataContext as ClassificationPageViewModel;

        private async void Page_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            DatasetBox.IsChecked = false;    
            SettingUpBox.IsChecked = false;
            TrainingBox.IsChecked = false;
            TestingBox.IsChecked = false;
            PlottingBox.IsChecked = false;
            RestartButton.IsEnabled = false;
            PredictButton.IsEnabled = false;

            BusyIndicator.Visibility = Windows.UI.Xaml.Visibility.Visible;
            BusyIndicator.PlayAnimation();

            // Prepare the input files
            DatasetBox.IsChecked = true;
            var testDataPath = await MlDotNet.FilePath(@"ms-appx:///Data/test.tsv");

            // Configure data transformations.
            SettingUpBox.IsChecked = true;
            var trainingDataPath = await MlDotNet.FilePath(@"ms-appx:///Data/training.tsv");
            await ViewModel.Build(trainingDataPath);

            // Create and train the model      
            TrainingBox.IsChecked = true;
            await ViewModel.Train();

            // Could save the model here.
            // ...

            // Test and evaluate the model
            TestingBox.IsChecked = true;
            var metrics = await ViewModel.Evaluate(testDataPath);

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

            BusyIndicator.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            BusyIndicator.PauseAnimation();
            RestartButton.IsEnabled = true;
            PredictButton.IsEnabled = true;
        }

        private async void Calculate_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            // Predict
            var result = await ViewModel.Predict(TextInput.Text);
            TextPrediction.Text = string.Format("{1}% sure this is {0}.", result.PredictedLanguage, result.Confidence);
        }
    }
}
