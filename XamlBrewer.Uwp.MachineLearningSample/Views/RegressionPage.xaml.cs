using Microsoft.ML.Legacy;
using OxyPlot;
using Windows.UI.Xaml.Controls;
using XamlBrewer.Uwp.MachineLearningSample.Models;
using XamlBrewer.Uwp.MachineLearningSample.ViewModels;

namespace XamlBrewer.Uwp.MachineLearningSample
{
    public sealed partial class RegressionPage : Page
    {
        private PredictionModel<MulticlassClassificationData, MulticlassClassificationPrediction> _model;

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
            PredictButton.IsEnabled = false;

            BusyIndicator.Visibility = Windows.UI.Xaml.Visibility.Visible;
            BusyIndicator.PlayAnimation();

            // Prepare the input files
            DatasetBox.IsChecked = true;
            // var testDataPath = await MlDotNet.FilePath(@"ms-appx:///Data/test.tsv");

            // Configure data transformations.
            SettingUpBox.IsChecked = true;
            // var trainingDataPath = await MlDotNet.FilePath(@"ms-appx:///Data/training.tsv");
            // await ViewModel.Build(trainingDataPath);

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
            // var result = await ViewModel.Predict(TextInput.Text);
            // TextPrediction.Text = string.Format("{1}% sure this is {0}.", result.PredictedLanguage, result.Confidence);
        }
    }
}
