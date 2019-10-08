using Mvvm.Services;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.ComponentModel;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using XamlBrewer.Uwp.MachineLearningSample.ViewModels;

namespace XamlBrewer.Uwp.MachineLearningSample
{
    public sealed partial class AutomationPage : Page
    {
        private string _trainingDataPath;
        private string _validationDataPath;
        private int _experimentNumber;

        public AutomationPage()
        {
            this.InitializeComponent();
            var dataContext = new AutomationPageViewModel();
            this.DataContext = dataContext;
            dataContext.PropertyChanged += DataContext_PropertyChanged;

            Loaded += Page_Loaded;
        }

        private void DataContext_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CurrentExperiment")
            {
                _experimentNumber++;

                _ = Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                        () =>
                            {
                                var currentExperiment = (sender as AutomationPageViewModel).CurrentExperiment;
                                ProgressTextBlock.Text = currentExperiment.Trainer;

                                // Update diagram
                                (Diagram.Model.Series[0] as LineSeries).Points.Add(new DataPoint(_experimentNumber, currentExperiment.LogLoss == null ? 0 : currentExperiment.LogLoss.Value));
                                (Diagram.Model.Series[1] as LineSeries).Points.Add(new DataPoint(_experimentNumber, currentExperiment.LogLossReduction == null ? 0 : currentExperiment.LogLossReduction.Value));
                                (Diagram.Model.Series[2] as LineSeries).Points.Add(new DataPoint(_experimentNumber, currentExperiment.MicroAccuracy == null ? 0 : currentExperiment.MicroAccuracy.Value));
                                (Diagram.Model.Series[3] as LineSeries).Points.Add(new DataPoint(_experimentNumber, currentExperiment.MacroAccuracy == null ? 0 : currentExperiment.MacroAccuracy.Value));
                                Diagram.InvalidatePlot();
                            }
                        );
            }
        }

        private AutomationPageViewModel ViewModel => DataContext as AutomationPageViewModel;

        private async void Page_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            DatasetBox.IsChecked = false;
            DataViewBox.IsChecked = false;
            SetUpExperimentBox.IsChecked = false;
            RunExperimentBox.IsChecked = false;
            ProgressTextBlock.Text = string.Empty;
            StartButton.IsEnabled = false;
            AlgorithmTextBlock.Text = string.Empty;
            HyperButton.IsEnabled = false;
            _experimentNumber = 0;

            BusyIndicator.Visibility = Windows.UI.Xaml.Visibility.Visible;
            BusyIndicator.Resume();

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
            ProgressTextBlock.Text = "Starting";
            AlgorithmTextBlock.Text = "The winner is " + await ViewModel.RunExperiment() + ". 🏆";
            ProgressTextBlock.Text = string.Empty;

            StartButton.IsEnabled = true;
            HyperButton.IsEnabled = true;

            BusyIndicator.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            BusyIndicator.Pause();
        }

        private void PrepareDiagram()
        {
            var foreground = OxyColors.SteelBlue;
            var plotModel = new PlotModel
            {
                Subtitle = "Evaluation Metrics",
                PlotAreaBorderThickness = new OxyThickness(1, 0, 0, 1),
                PlotAreaBorderColor = foreground,
                TextColor = foreground,
                TitleColor = foreground,
                SubtitleColor = foreground,
                LegendPlacement = LegendPlacement.Outside,
                LegendPosition = LegendPosition.TopCenter,
                LegendOrientation = LegendOrientation.Horizontal
            };
            plotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                TextColor = foreground,
                TicklineColor = OxyColors.Transparent,
                TitleColor = foreground,
                Title = "Experiments",
                MinorStep = 1,
                MajorStep = 1
            });

            var linearAxis = new LinearAxis
            {
                Position = AxisPosition.Left,
                TextColor = foreground,
                TicklineColor = foreground,
                TitleColor = foreground
            };
            plotModel.Axes.Add(linearAxis);

            plotModel.Series.Add(new LineSeries
            {
                Title = "Log Loss",
                Color = OxyColors.DarkOrange,
                TextColor = OxyColors.Transparent
            });

            plotModel.Series.Add(new LineSeries
            {
                Title = "Log Loss Reduction",
                Color = OxyColors.Firebrick,
                TextColor = OxyColors.Transparent
            });

            plotModel.Series.Add(new LineSeries
            {
                Title = "Micro Accuracy",
                Color = OxyColors.MidnightBlue,
                TextColor = OxyColors.Transparent
            });

            plotModel.Series.Add(new LineSeries
            {
                Title = "Macro Accuracy",
                Color = OxyColors.MediumSeaGreen,
                TextColor = OxyColors.Transparent
            });

            Diagram.Model = plotModel;
        }

        private async void HyperParametrisation_Clicked(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            BusyIndicator.Visibility = Windows.UI.Xaml.Visibility.Visible;
            BusyIndicator.Resume();
            StartButton.IsEnabled = false;
            HyperButton.IsEnabled = false;
            _experimentNumber = 0;
            PrepareDiagram();
            await ViewModel.HyperParameterize();
            BusyIndicator.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            BusyIndicator.Pause();
            StartButton.IsEnabled = true;
            HyperButton.IsEnabled = true;
        }
    }
}
