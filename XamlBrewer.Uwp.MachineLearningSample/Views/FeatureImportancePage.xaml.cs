using Mvvm.Services;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Controls;
using XamlBrewer.Uwp.MachineLearningSample.Models;
using XamlBrewer.Uwp.MachineLearningSample.ViewModels;

namespace XamlBrewer.Uwp.MachineLearningSample
{
    public sealed partial class FeatureImportancePage : Page
    {
        public FeatureImportancePage()
        {
            this.InitializeComponent();
            this.DataContext = new FeatureImportancePageViewModel();

            Loaded += Page_Loaded;
        }

        private OxyColor OxyForeground => OxyColors.SteelBlue;

        private OxyColor OxyText => OxyColors.Wheat;

        private OxyColor OxyFill => OxyColors.Goldenrod;

        private FeatureImportancePageViewModel ViewModel => DataContext as FeatureImportancePageViewModel;

        private async void Page_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            TrainingBox.IsChecked = false;
            PlotBox.IsChecked = false;
            RestartButton.IsEnabled = false;

            BusyIndicator.Visibility = Windows.UI.Xaml.Visibility.Visible;
            BusyIndicator.Resume();

            // Clear the diagram.
            Diagram.Model.PlotAreaBorderThickness = new OxyThickness(1, 0, 0, 1);
            Diagram.InvalidatePlot();

            // Create and train the regression model 
            TrainingBox.IsChecked = true;
            var dataPath = await MlDotNet.FilePath(@"ms-appx:///Data/winequality_white_train.csv");
            var featureImportances = await ViewModel.ComputePermutationMetrics(dataPath);

            // Visualize the R-Squared decrease for the model features.   
            PlotBox.IsChecked = true;
            UpdatePlot(featureImportances);

            BusyIndicator.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            BusyIndicator.Pause();
            RestartButton.IsEnabled = true;
        }

        private void UpdatePlot(List<FeatureImportance> featureImportances)
        {
            var categories = new List<string>();
            var importanceBars = new List<BarItem>();
            foreach (var featureImportance in featureImportances.OrderBy(f => Math.Abs(f.R2Decrease)))
            {
                categories.Add(featureImportance.Name);
                importanceBars.Add(new BarItem { Value = featureImportance.R2Decrease });
            }

            var plotModel = Diagram.Model;

            (plotModel.Axes[0] as CategoryAxis).ItemsSource = categories;
            (plotModel.Series[0] as BarSeries).ItemsSource = importanceBars;
            plotModel.InvalidatePlot(true);
        }
    }
}
