using Mvvm.Services;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Controls;
using XamlBrewer.Uwp.MachineLearningSample.Models;
using XamlBrewer.Uwp.MachineLearningSample.ViewModels;

namespace XamlBrewer.Uwp.MachineLearningSample
{
    public sealed partial class RecommendationPage : Page
    {
        private OxyColor OxyForeground => OxyColors.SteelBlue;

        private OxyColor OxyText => OxyColors.Wheat;

        private OxyColor OxyFill => OxyColors.Firebrick;

        public RecommendationPage()
        {
            this.InitializeComponent();
            this.DataContext = new RecommendationPageViewModel();

            Loaded += Page_Loaded;
        }

        private RecommendationPageViewModel ViewModel => DataContext as RecommendationPageViewModel;

        private async void Page_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            DatasetBox.IsChecked = false;
            SettingUpBox.IsChecked = false;
            TrainingBox.IsChecked = false;
            TestingBox.IsChecked = false;
            RestartButton.IsEnabled = false;
            PredictButton.IsEnabled = false;

            BusyIndicator.Visibility = Windows.UI.Xaml.Visibility.Visible;
            BusyIndicator.PlayAnimation();

            // Prepare diagram
            Diagram.Model.PlotAreaBorderThickness = new OxyThickness(1, 0, 0, 1);
            Diagram.InvalidatePlot();

            // Prepare the input files
            DatasetBox.IsChecked = true;
            var dataPath = await MlDotNet.FilePath(@"ms-appx:///Data/LasVegasTripAdvisorReviews.csv");
            var data = await ViewModel.Load(dataPath);
            HotelsCombo.ItemsSource = ViewModel.Hotels;
            HotelsCombo.SelectedIndex = 0;
            TravelerTypesCombo.ItemsSource = ViewModel.TravelerTypes;
            TravelerTypesCombo.SelectedIndex = 0;

            // Configure data transformations.
            SettingUpBox.IsChecked = true;
            await ViewModel.Build();

            // Create and train the model      
            TrainingBox.IsChecked = true;
            // await ViewModel.Train();

            // Save the model.
            await ViewModel.Save("recommendationModel.zip");

            // Test and evaluate the model
            TestingBox.IsChecked = true;
            // var metrics = await ViewModel.Evaluate(testDataPath);

            BusyIndicator.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            BusyIndicator.PauseAnimation();
            RestartButton.IsEnabled = true;
            PredictButton.IsEnabled = true;
        }

        private async void Calculate_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            // Individual Prediction
            var recommendationData = new RecommendationData
            {
                Hotel = HotelsCombo.SelectedValue.ToString(),
                TravelerType = TravelerTypesCombo.SelectedValue.ToString()
            };

            var result = await ViewModel.Predict(recommendationData);
            ResultBlock.Text = result.Score.ToString();

            // Group Prediction
            var recommendations = new List<RecommendationData>();
            foreach (var hotel in ViewModel.Hotels)
            {
                recommendations.Add(new RecommendationData
                {
                    Hotel = hotel,
                    TravelerType = TravelerTypesCombo.SelectedValue.ToString()
                });
            }
            var predictions = await ViewModel.Predict(recommendations);
            var recommendationsResult = predictions
                    .Select(p => p)
                    .OrderByDescending(p => p.Score)
                    .ToList()
                    .Take(10)
                    .Reverse();

            // Update diagram
            var categories = new List<string>();
            var bars = new List<BarItem>();
            foreach (var prediction in recommendationsResult)
            {
                categories.Add(prediction.Hotel);
                bars.Add(new BarItem { Value = prediction.Score });
            }

            var plotModel = Diagram.Model;

            (plotModel.Axes[0] as CategoryAxis).ItemsSource = categories;
            (plotModel.Series[0] as BarSeries).ItemsSource = bars;
            plotModel.InvalidatePlot(true);
        }
    }
}
