using Mvvm.Services;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using XamlBrewer.Uwp.MachineLearningSample.Models;
using XamlBrewer.Uwp.MachineLearningSample.ViewModels;

namespace XamlBrewer.Uwp.MachineLearningSample
{
    public sealed partial class FeatureContributionPage : Page
    {
        private readonly List<FeatureContribution> _featureContributions = new List<FeatureContribution> {
                        new FeatureContribution("FixedAcidity"),
                        new FeatureContribution("VolatileAcidity"),
                        new FeatureContribution("CitricAcid"),
                        new FeatureContribution("ResidualSugar"),
                        new FeatureContribution("Chlorides"),
                        new FeatureContribution("FreeSulfurDioxide"),
                        new FeatureContribution("TotalSulfurDioxide"),
                        new FeatureContribution("Density"),
                        new FeatureContribution("Ph"),
                        new FeatureContribution("Sulphates"),
                        new FeatureContribution("Alcohol")};

        public FeatureContributionPage()
        {
            this.InitializeComponent();
            this.DataContext = new FeatureContributionPageViewModel();

            Loaded += Page_Loaded;
        }

        private OxyColor OxyForeground => OxyColors.SteelBlue;

        private OxyColor OxyText => OxyColors.Wheat;

        private OxyColor OxyWeightsFill => OxyColors.MidnightBlue;

        private OxyColor OxyContributionsFill => OxyColors.Firebrick;

        private FeatureContributionPageViewModel ViewModel => DataContext as FeatureContributionPageViewModel;

        private async void Page_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            TrainingBox.IsChecked = false;
            WeightsBox.IsChecked = false;
            PredictionBox.IsChecked = false;
            RestartButton.IsEnabled = false;
            PredictButton.IsEnabled = false;
            LabelText.Text = string.Empty;
            ScoreText.Text = string.Empty;

            BusyIndicator.Visibility = Windows.UI.Xaml.Visibility.Visible;
            BusyIndicator.Resume();

            // Clear the diagram.
            Diagram.Model.PlotAreaBorderThickness = new OxyThickness(1, 0, 0, 1);
            Diagram.InvalidatePlot();

            // Create and train the regression model 
            TrainingBox.IsChecked = true;
            var dataPath = await MlDotNet.FilePath(@"ms-appx:///Data/winequality_white_train.csv");
            var featureWeights = await ViewModel.BuildAndTrain(dataPath);
            for (int i = 0; i < 11; i++)
            {
                _featureContributions[i].Weight = featureWeights[i];
                _featureContributions[i].Contribution = 0;
            }

            // Visualize the feature weights for the model.   
            WeightsBox.IsChecked = true;
            Diagram.Model.Series[1].IsVisible = false;
            UpdatePlot();

            // Creating the prediction model
            PredictionBox.IsChecked = true;
            await ViewModel.CreatePredictionModel();

            BusyIndicator.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            BusyIndicator.Pause();
            RestartButton.IsEnabled = true;
            PredictButton.IsEnabled = true;
        }

        private void UpdatePlot()
        {
            var categories = new List<string>();
            var weightBars = new List<BarItem>();
            var contributionBars = new List<BarItem>();
            foreach (var featureContribution in _featureContributions.OrderBy(f => Math.Abs(f.Contribution)).ThenBy(f => Math.Abs(f.Weight)))
            {
                categories.Add(featureContribution.Name);
                weightBars.Add(new BarItem { Value = featureContribution.Weight });
                contributionBars.Add(new BarItem { Value = featureContribution.Contribution });
            }

            var plotModel = Diagram.Model;

            (plotModel.Axes[0] as CategoryAxis).ItemsSource = categories;
            (plotModel.Series[0] as BarSeries).ItemsSource = weightBars;
            (plotModel.Series[1] as BarSeries).ItemsSource = contributionBars;
            plotModel.InvalidatePlot(true);
        }

        private async void Prediction_Clicked(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var prediction = await ViewModel.GetRandomPrediction();
            for (int i = 0; i < 11; i++)
            {
                _featureContributions[i].Contribution = prediction.FeatureContributions[i];
            }

            LabelText.Text = $"Label: {prediction.Label}";
            ScoreText.Text = $"Score: {prediction.Score:N1}";

            Diagram.Model.Series[1].IsVisible = true;
            UpdatePlot();
        }
    }
}
