using Mvvm.Services;
using OxyPlot;
using OxyPlot.Series;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;
using XamlBrewer.Uwp.MachineLearningSample.ViewModels;

namespace XamlBrewer.Uwp.MachineLearningSample
{
    public sealed partial class ClassificationPage : Page
    {
        private OxyColor OxyForeground => OxyColors.SteelBlue;

        private OxyColor OxyText => OxyColors.Wheat;

        private OxyColor OxyFill => OxyColors.Firebrick;

        private string[] Languages => new[]
                {
                    "German",
                    "English",
                    "French",
                    "Italian",
                    "Romanian",
                    "Spanish"
                };

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

            // Prepare diagram
            var plotModel = Diagram.Model;
            plotModel.PlotAreaBorderThickness = new OxyThickness(1, 0, 0, 1);
            Diagram.InvalidatePlot();

            // Prepare the input files
            DatasetBox.IsChecked = true;
            var testDataPath = await MlDotNet.FilePath(@"ms-appx:///Data/test.tsv");

            // Configure data transformations.
            SettingUpBox.IsChecked = true;
            var trainingDataPath = await MlDotNet.FilePath(@"ms-appx:///Data/training.tsv");
            await ViewModel.Build();

            // Create and train the model      
            TrainingBox.IsChecked = true;
            await ViewModel.Train(trainingDataPath);

            // Save the model.
            await ViewModel.Save("classificationModel.zip");

            // Test and evaluate the model
            TestingBox.IsChecked = true;
            var metrics = await ViewModel.Evaluate(testDataPath);

            // Diagram
            PlottingBox.IsChecked = true;

            var bars = new List<BarItem>();
            foreach (var logloss in metrics.PerClassLogLoss)
            {
                bars.Add(new BarItem { Value = logloss });
            }

            (plotModel.Series[0] as BarSeries).ItemsSource = bars;
            plotModel.InvalidatePlot(true);

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
