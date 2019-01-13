using Mvvm.Services;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Controls;
using XamlBrewer.Uwp.MachineLearningSample.ViewModels;

namespace XamlBrewer.Uwp.MachineLearningSample
{
    public sealed partial class BoxPlotPage : Page
    {
        public BoxPlotPage()
        {
            this.InitializeComponent();
            this.DataContext = new BoxPlotPageViewModel();

            Loaded += Page_Loaded;
        }

        private BoxPlotPageViewModel ViewModel => DataContext as BoxPlotPageViewModel;

        private async void Page_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            // Prepare diagram
            var plotModel = PrepareDiagram();

            // Read data
            var trainingDataPath = await MlDotNet.FilePath(@"ms-appx:///Data/Mall_Customers.csv");
            var data = await ViewModel.Load(trainingDataPath);

            // Populate diagram
            var ages = new List<double>();
            var incomes = new List<double>();
            var scores = new List<double>();
            foreach (var value in data)
            {
                ages.Add(value.Age);
                incomes.Add(value.AnnualIncome);
                scores.Add(value.SpendingScore);
            }

            AddItem(plotModel, ages, 0);
            AddItem(plotModel, incomes, 1);
            AddItem(plotModel, scores, 2);

            // Update diagram
            Diagram.InvalidatePlot();
        }

        private void AddItem(PlotModel plotModel, List<double> values, int slot)
        {
            values.Sort();
            var median = values.Median(true);
            int r = values.Count % 2;
            var firstQuartile = values.Take((values.Count + r) / 2).Median(true);
            var thirdQuartile = values.Skip((values.Count - r) / 2).Median(true);

            var interQuartileRange = thirdQuartile - firstQuartile;
            var step = interQuartileRange * 1.5;
            var upperWhisker = thirdQuartile + step;
            upperWhisker = values.Where(v => v <= upperWhisker).Max();
            var lowerWhisker = firstQuartile - step;
            lowerWhisker = values.Where(v => v >= lowerWhisker).Min();

            var outliers = values.Where(v => v > upperWhisker || v < lowerWhisker).ToList();

            var item = new BoxPlotItem(slot, lowerWhisker, firstQuartile, median, thirdQuartile, upperWhisker)
            {
                Outliers = outliers
            };
            if (outliers.Any())
            {
                (plotModel.Series[1] as BoxPlotSeries).Items.Add(item);
            }
            else
            {
                (plotModel.Series[0] as BoxPlotSeries).Items.Add(item);
            }
        }

        private PlotModel PrepareDiagram()
        {
            var foreground = OxyColors.LightSteelBlue;

            var plotModel = new PlotModel
            {
                PlotAreaBorderThickness = new OxyThickness(1, 0, 0, 1),
                PlotAreaBorderColor = foreground,
                TextColor = foreground,
                TitleColor = foreground,
                SubtitleColor = foreground
            };

            var series = new BoxPlotSeries
            {
                Stroke = foreground,
                Fill = OxyColors.DarkGoldenrod
            };
            plotModel.Series.Add(series);

            var outlinerSeries = new BoxPlotSeries
            {
                Stroke = foreground,
                Fill = OxyColors.Firebrick,
                OutlierSize = 4
            };
            plotModel.Series.Add(outlinerSeries);

            plotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                TextColor = foreground,
                TicklineColor = foreground,
                TitleColor = foreground
            });

            plotModel.Axes.Add(new CategoryAxis
            {
                Position = AxisPosition.Bottom,
                ItemsSource = new[]
                    {
                        "Age",
                        "Annual Income",
                        "Spending Score"
                    },
                TextColor = foreground,
                TicklineColor = OxyColors.Transparent,
                TitleColor = foreground
            });

            Diagram.Model = plotModel;

            return plotModel;
        }
    }
}
