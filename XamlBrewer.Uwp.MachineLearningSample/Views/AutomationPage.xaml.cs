using Mvvm.Services;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using Windows.UI.Xaml.Controls;
using XamlBrewer.Uwp.MachineLearningSample.ViewModels;

namespace XamlBrewer.Uwp.MachineLearningSample
{
    public sealed partial class AutomationPage : Page
    {
        private string _trainingDataPath;
        private string _validationDataPath;

        public AutomationPage()
        {
            this.InitializeComponent();
            this.DataContext = new AutomationPageViewModel();

            Loaded += Page_Loaded;
        }

        private AutomationPageViewModel ViewModel => DataContext as AutomationPageViewModel;

        private async void Page_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            DatasetBox.IsChecked = false;
            DataViewBox.IsChecked = false;
            SetUpExperimentBox.IsChecked = false;
            RunExperimentBox.IsChecked = false;
            StartButton.IsEnabled = false;

            BusyIndicator.Visibility = Windows.UI.Xaml.Visibility.Visible;
            BusyIndicator.PlayAnimation();

            // Prepare diagram.
            PrepareDiagram();
            
            // Prepare datasets.
            DatasetBox.IsChecked = true;
            _trainingDataPath = await MlDotNet.FilePath(@"ms-appx:///Data/winequality_white_train.csv");
            _validationDataPath = await MlDotNet.FilePath(@"ms-appx:///Data/winequality_white_test.csv");

            // Create and load dataview.
            DataViewBox.IsChecked = true;
            await ViewModel.CreateDataViews(_trainingDataPath, _validationDataPath);

            // Set up experiment.
            SetUpExperimentBox.IsChecked = true;
            await ViewModel.SetUpExperiment();

            // Run experiment.
            RunExperimentBox.IsChecked = true;
            await ViewModel.RunExperiment();
                       
            // Update diagram
            Diagram.InvalidatePlot();

            StartButton.IsEnabled = true;

            BusyIndicator.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            BusyIndicator.PauseAnimation();
        }

        private void PrepareDiagram()
        {
            var foreground = OxyColors.SteelBlue;
            var plotModel = new PlotModel
            {
                Subtitle = "Model Comparison",
                PlotAreaBorderThickness = new OxyThickness(1, 0, 0, 1),
                PlotAreaBorderColor = foreground,
                TextColor = foreground,
                TitleColor = foreground,
                SubtitleColor = foreground,
                LegendPlacement = LegendPlacement.Outside,
                LegendPosition = LegendPosition.TopCenter,
                LegendOrientation = LegendOrientation.Horizontal
            };
            plotModel.Axes.Add(new CategoryAxis
            {
                Position = AxisPosition.Bottom,
                Key = "ModelAxis",
                ItemsSource = new[]
                    {
                        "Model 1",
                        "Model 2",
                        "Model 3",
                        "Model 4",
                        "Model 5",
                        "Model 6"
                    },
                TextColor = foreground,
                TicklineColor = foreground,
                TitleColor = foreground
            });

            var linearAxis = new LinearAxis
            {
                Position = AxisPosition.Left,
                TextColor = foreground,
                TicklineColor = foreground,
                TitleColor = foreground
            };
            plotModel.Axes.Add(linearAxis);

            var accuracySeries = new ColumnSeries
            {
                Title = "Accuracy",
                LabelPlacement = LabelPlacement.Inside,
                LabelFormatString = "{0:.00}",
                FillColor = OxyColors.DarkOrange,
                TextColor = OxyColors.Wheat
            };
            plotModel.Series.Add(accuracySeries);

            var areaUnderCurveSeries = new ColumnSeries
            {
                Title = "Area under ROC curve",
                LabelPlacement = LabelPlacement.Inside,
                LabelFormatString = "{0:.00}",
                FillColor = OxyColors.Firebrick,
                TextColor = OxyColors.Wheat
            };
            plotModel.Series.Add(areaUnderCurveSeries);

            var f1ScoreSeries = new ColumnSeries
            {
                Title = "F1 Score",
                LabelPlacement = LabelPlacement.Inside,
                LabelFormatString = "{0:.00}",
                FillColor = OxyColors.MidnightBlue,
                TextColor = OxyColors.Wheat
            };
            plotModel.Series.Add(f1ScoreSeries);

            var positiveRecallSeries = new ColumnSeries
            {
                Title = "Positive Recall",
                LabelPlacement = LabelPlacement.Inside,
                LabelFormatString = "{0:.00}",
                FillColor = OxyColors.MediumSeaGreen,
                TextColor = OxyColors.Wheat
            };
            plotModel.Series.Add(positiveRecallSeries);

            Diagram.Model = plotModel;
        }
    }
}
