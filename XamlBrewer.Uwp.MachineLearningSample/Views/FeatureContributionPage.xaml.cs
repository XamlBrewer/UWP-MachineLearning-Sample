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
        private OxyColor OxyForeground => OxyColors.SteelBlue;

        private OxyColor OxyText => OxyColors.Wheat;

        private OxyColor OxyWeightsFill => OxyColors.MidnightBlue;

        private OxyColor OxyContributionsFill => OxyColors.Firebrick;

        public FeatureContributionPage()
        {
            this.InitializeComponent();
            this.DataContext = new FeatureContributionPageViewModel();

            Loaded += Page_Loaded;
        }

        private FeatureContributionPageViewModel ViewModel => DataContext as FeatureContributionPageViewModel;

        private async void Page_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            TrainingBox.IsChecked = false;
            WeightsBox.IsChecked = false;
            SavingBox.IsChecked = false;
            TestingBox.IsChecked = false;
            RestartButton.IsEnabled = false;

            BusyIndicator.Visibility = Windows.UI.Xaml.Visibility.Visible;
            BusyIndicator.PlayAnimation();

            var featureContributions = new List<FeatureContribution> {
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

            // Clear the diagram.
            Diagram.Model.PlotAreaBorderThickness = new OxyThickness(1, 0, 0, 1);
            Diagram.InvalidatePlot();

            // Create and train the regression model 
            TrainingBox.IsChecked = true;
            var dataPath = await MlDotNet.FilePath(@"ms-appx:///Data/winequality_white_train.csv");
            var featureWeights = await ViewModel.BuildAndTrain(dataPath);
            for (int i = 0; i < 11; i++)
            {
                featureContributions[i].Weight = featureWeights[i];
            }

            // Visualize the feature weights for the model.   

            WeightsBox.IsChecked = true;
            var categories = new List<string>();
            var bars = new List<BarItem>();
            foreach (var featureContribution in featureContributions.OrderBy(f => Math.Abs(f.Weight)))
            {
                categories.Add(featureContribution.Name);
                bars.Add(new BarItem { Value = featureContribution.Weight });
            }

            var plotModel = Diagram.Model;

            (plotModel.Axes[0] as CategoryAxis).ItemsSource = categories;
            (plotModel.Series[0] as BarSeries).ItemsSource = bars;
            plotModel.InvalidatePlot(true);

            //// Save the model.
            //SavingBox.IsChecked = true;
            //await ViewModel.Save("recommendationModel.zip");

            //// Test and evaluate the model
            //TestingBox.IsChecked = true;
            //var metrics = await ViewModel.Evaluate(dataPath);

            BusyIndicator.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            BusyIndicator.PauseAnimation();
            RestartButton.IsEnabled = true;
        }
    }
}
