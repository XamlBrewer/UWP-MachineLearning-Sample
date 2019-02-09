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
            //
            // Left: unprepared data.
            //

            // Prepare diagram
            var plotModel = PrepareDiagram(new[]
                    {
                        "TS",
                        "ORB",
                        "DRB",
                        "TRB",
                        "AST",
                        "STL",
                        "BLK",
                        "TOV",
                        "USG",
                        "Age"
                    });

            plotModel.Title = "Regression Sample Input Data";
            plotModel.Subtitle = "Very dispersed";

            // Read data
            var regressionData = await ViewModel.LoadRegressionData();

            // Populate diagram
            for (int i = 0; i < regressionData.Count; i++)
            {
                AddItem(plotModel, regressionData[i], i);
            }

            // Update diagram
            RegressionDiagram.Model = plotModel;
            RegressionDiagram.InvalidatePlot();

            //
            // Right: prepared data.
            //

            // Prepare diagram
            plotModel = PrepareDiagram(new[]
                    {
                        "Age",
                        "Annual Income",
                        "SpendingScore"
                    });

            plotModel.Title = "Clustering Sample Input Data";
            plotModel.Subtitle = "Well prepared";

            // Read data
            var clusteringData = await ViewModel.LoadClusteringData();

            // Populate diagram
            for (int i = 0; i < clusteringData.Count; i++)
            {
                AddItem(plotModel, clusteringData[i], i);
            }

            // Update diagram
            ClusterDiagram.Model = plotModel;
            ClusterDiagram.InvalidatePlot();
        }

        private void AddItem(PlotModel plotModel, List<double> values, int slot)
        {
            values.Sort();

            var sorted = values.ToArray();

            var median = sorted.Median();
            int r = values.Count % 2;
            var firstQuartile = sorted.LowerQuartile();
            var thirdQuartile = sorted.UpperQuartile();

            var interQuartileRange = thirdQuartile - firstQuartile;
            var step = interQuartileRange * 1.5;
            var upperWhisker = thirdQuartile + step;
            upperWhisker = values.Where(v => v <= upperWhisker).Max();
            var lowerWhisker = firstQuartile - step;
            lowerWhisker = values.Where(v => v >= lowerWhisker).Min();

            var outliers = values.Where(v => v > upperWhisker || v < lowerWhisker).ToList();

            var item = new BoxPlotItem(
                slot, 
                lowerWhisker, 
                firstQuartile, 
                median, 
                thirdQuartile, 
                upperWhisker)
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

        private PlotModel PrepareDiagram(string[] categories)
        {
            var foreground = OxyColors.SteelBlue;

            var plotModel = new PlotModel
            {
                PlotAreaBorderThickness = new OxyThickness(1, 0, 0, 1),
                PlotAreaBorderColor = foreground,
                TextColor = foreground,
                TitleColor = foreground,
                TitleFontWeight = 2,
                SubtitleColor = foreground
            };

            var series = new BoxPlotSeries
            {
                Stroke = foreground,
                Fill = OxyColors.DarkOrange
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
                ItemsSource = categories,
                TextColor = foreground,
                TicklineColor = OxyColors.Transparent,
                TitleColor = foreground
            });

            return plotModel;
        }
    }
}
