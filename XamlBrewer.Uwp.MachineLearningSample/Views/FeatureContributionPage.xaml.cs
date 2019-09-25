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
    public sealed partial class FeatureContributionPage : Page
    {
        private OxyColor OxyForeground => OxyColors.SteelBlue;

        private OxyColor OxyText => OxyColors.Wheat;

        private OxyColor OxyFill => OxyColors.Firebrick;

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
            TrainingBox2.IsChecked = false;
            SavingBox.IsChecked = false;
            TestingBox.IsChecked = false;
            RestartButton.IsEnabled = false;

            BusyIndicator.Visibility = Windows.UI.Xaml.Visibility.Visible;
            BusyIndicator.PlayAnimation();

            // Create and train the regression model 
            Diagram.Model.PlotAreaBorderThickness = new OxyThickness(1, 0, 0, 1);
            Diagram.InvalidatePlot();

            // Prepare the input files
            TrainingBox.IsChecked = true;
            var dataPath = await MlDotNet.FilePath(@"ms-appx:///Data/winequality_white_train.csv");
            await ViewModel.BuildAndTrain(dataPath);


            //// Create and train the model      
            //TrainingBox.IsChecked = true;
            //await ViewModel.Build();

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
