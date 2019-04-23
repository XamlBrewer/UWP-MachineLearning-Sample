using Mvvm.Services;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using XamlBrewer.Uwp.MachineLearningSample.Models;
using XamlBrewer.Uwp.MachineLearningSample.ViewModels;

namespace XamlBrewer.Uwp.MachineLearningSample
{
    public sealed partial class FieldAwareFactorizationPage : Page
    {
        private OxyColor OxyForeground => OxyColors.SteelBlue;

        private OxyColor OxyText => OxyColors.Wheat;

        private OxyColor OxyFill => OxyColors.Firebrick;

        public FieldAwareFactorizationPage()
        {
            this.InitializeComponent();
            this.DataContext = new FieldAwareFactorizationPageViewModel();

            Loaded += Page_Loaded;
        }

        private FieldAwareFactorizationPageViewModel ViewModel => DataContext as FieldAwareFactorizationPageViewModel;

        private async void Page_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            DatasetBox.IsChecked = false;
            TrainingBox.IsChecked = false;
            SavingBox.IsChecked = false;
            TestingBox.IsChecked = false;
            RestartButton.IsEnabled = false;
            TravelerTypesCombo.SelectedIndex = -1;
            SeasonsCombo.SelectedIndex = -1;
            ResultBlock.Text = string.Empty;

            BusyIndicator.Visibility = Windows.UI.Xaml.Visibility.Visible;
            BusyIndicator.PlayAnimation();

            // Prepare diagram
            Diagram.Model.PlotAreaBorderThickness = new OxyThickness(1, 0, 0, 1);
            Diagram.InvalidatePlot();

            // Prepare the input files
            DatasetBox.IsChecked = true;
            var dataPath = await MlDotNet.FilePath(@"ms-appx:///Data/LasVegasTripAdvisorReviews.csv");
            var data = await ViewModel.Load(dataPath);
            TravelerTypesCombo.ItemsSource = ViewModel.TravelerTypes;
            SeasonsCombo.ItemsSource = ViewModel.Seasons;
            HotelsCombo.ItemsSource = ViewModel.Hotels;

            // Create and train the model      
            TrainingBox.IsChecked = true;
            await ViewModel.Build();

            // Save the model.
            SavingBox.IsChecked = true;
            await ViewModel.Save("FfmRecommendationModel.zip");

            // Test and evaluate the model
            TestingBox.IsChecked = true;
            var metrics = await ViewModel.Evaluate(dataPath);

            BusyIndicator.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            BusyIndicator.PauseAnimation();
            RestartButton.IsEnabled = true;
            TravelerTypesCombo.SelectedIndex = 0;
            SeasonsCombo.SelectedIndex = 0;
        }

        private async void TravelerTypesCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await MakeIndividualPrediction();
            await MakeGroupPrediction();
        }

        private async void SeasonsCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await MakeIndividualPrediction();
            await MakeGroupPrediction();
        }

        private async void HotelsCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await MakeIndividualPrediction();
        }

        private async Task MakeIndividualPrediction()
        {
            if (HotelsCombo.SelectedValue == null || TravelerTypesCombo.SelectedValue == null || SeasonsCombo.SelectedValue == null)
            {
                return;
            }

            // Individual Prediction
            var recommendationData = new FfmRecommendationData
            {
                Hotel = HotelsCombo.SelectedValue.ToString(),
                TravelerType = TravelerTypesCombo.SelectedValue.ToString(),
                Season = SeasonsCombo.SelectedValue.ToString()
            };

            var result = await ViewModel.Predict(recommendationData);
            if (!result.PredictedLabel)
            {
                // Bring to a range from -1 (highly discouraged) to +1 (highly recommended).
                result.Probability = -result.Probability;
            }

            ResultBlock.Text = result != null ? result.Probability.ToString() : string.Empty;
        }

        private async Task MakeGroupPrediction()
        {
            if (TravelerTypesCombo.SelectedValue == null || SeasonsCombo.SelectedValue == null)
            {
                return;
            }

            // Group Prediction
            var recommendations = new List<FfmRecommendationData>();
            foreach (var hotel in ViewModel.Hotels)
            {
                recommendations.Add(new FfmRecommendationData
                {
                    Hotel = hotel,
                    TravelerType = TravelerTypesCombo.SelectedValue.ToString(),
                    Season = SeasonsCombo.SelectedValue.ToString()
                });
            }
            var predictions = await ViewModel.Predict(recommendations);
            if (predictions == null)
            {
                return;
            }

            var recommendationsResult = predictions
                    .Select(p => p)
                    .Where(p => p.PredictedLabel)
                    .OrderByDescending(p => p.Probability)
                    .ToList()
                    .Take(10)
                    .Reverse();

            // Update diagram
            var categories = new List<string>();
            var bars = new List<BarItem>();
            foreach (var prediction in recommendationsResult)
            {
                categories.Add(prediction.Hotel);
                bars.Add(new BarItem { Value = prediction.Probability });
            }

            var plotModel = Diagram.Model;

            (plotModel.Axes[0] as CategoryAxis).ItemsSource = categories;
            (plotModel.Series[0] as BarSeries).ItemsSource = bars;
            plotModel.InvalidatePlot(true);
        }
    }
}
