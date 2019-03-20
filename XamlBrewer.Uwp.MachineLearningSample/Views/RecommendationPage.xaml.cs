using Mvvm.Services;
using OxyPlot;
using Windows.UI.Xaml.Controls;
using XamlBrewer.Uwp.MachineLearningSample.Models;
using XamlBrewer.Uwp.MachineLearningSample.ViewModels;

namespace XamlBrewer.Uwp.MachineLearningSample
{
    public sealed partial class RecommendationPage : Page
    {
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
            PlottingBox.IsChecked = false;
            RestartButton.IsEnabled = false;
            PredictButton.IsEnabled = false;

            BusyIndicator.Visibility = Windows.UI.Xaml.Visibility.Visible;
            BusyIndicator.PlayAnimation();

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
            // await ViewModel.Save("classificationModel.zip");

            // Test and evaluate the model
            TestingBox.IsChecked = true;
            // var metrics = await ViewModel.Evaluate(testDataPath);

            // Diagram
            PlottingBox.IsChecked = true;
            var foreground = OxyColors.SteelBlue;
            var plotModel = new PlotModel
            {
                PlotAreaBorderThickness = new OxyThickness(1, 0, 0, 1),
                PlotAreaBorderColor = foreground,
                TextColor = foreground,
                TitleColor = foreground,
                SubtitleColor = foreground
            };

            // ...

            Diagram.Model = plotModel;

            BusyIndicator.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            BusyIndicator.PauseAnimation();
            RestartButton.IsEnabled = true;
            PredictButton.IsEnabled = true;
        }

        private async void Calculate_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            // Predict
            var recommendationData = new RecommendationData
            {
                Hotel = HotelsCombo.SelectedValue.ToString(),
                TravelerType = TravelerTypesCombo.SelectedValue.ToString()
            };

            var result = await ViewModel.Predict(recommendationData);
            ResultBlock.Text = result.Score.ToString();
        }
    }
}
