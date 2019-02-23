using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using Windows.UI.Xaml.Controls;
using XamlBrewer.Uwp.MachineLearningSample.ViewModels;

namespace XamlBrewer.Uwp.MachineLearningSample
{
    public sealed partial class HeatMapPage : Page
    {
        public HeatMapPage()
        {
            this.InitializeComponent();
            this.DataContext = new HeatMapPageViewModel();

            Loaded += Page_Loaded;
        }

        private HeatMapPageViewModel ViewModel => DataContext as HeatMapPageViewModel;

        // Reference plot at
        // https://tryolabs.com/blog/2017/03/16/pandas-seaborn-a-guide-to-handle-visualize-data-elegantly/
        // Minor difference, since we did not compensate missing Age values.

        private async void Page_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            // Prepare diagram
            var plotModel = PrepareDiagram();

            // Read data
            var matrix = await ViewModel.LoadCorrelationData();

            // Populate diagram
            var data = new double[6, 6];
            for (int x = 0; x < 6; ++x)
            {
                for (int y = 0; y < 5 - x; ++y)
                {
                    var seriesA = matrix[x];
                    var seriesB = matrix[5 - y];

                    var value = Statistics.Pearson(seriesA, seriesB);

                    data[x, y] = value;
                    data[5 - y, 5 - x] = value;
                }

                data[x, 5 - x] = 1;
            }

            (plotModel.Series[0] as HeatMapSeries).Data = data;

            // Update diagram
            Diagram.InvalidatePlot();
        }

        private PlotModel PrepareDiagram()
        {
            var foreground = OxyColors.SteelBlue;

            var plotModel = new PlotModel
            {
                PlotAreaBorderThickness = new OxyThickness(1, 0, 0, 1),
                PlotAreaBorderColor = foreground,
                TextColor = foreground,
                TitleColor = foreground,
                SubtitleColor = foreground
            };

            plotModel.Axes.Add(new CategoryAxis
            {
                Position = AxisPosition.Bottom,
                Key = "HorizontalAxis",
                ItemsSource = new[]
                {
                    "Survived",
                    "Class",
                    "Age",
                    "Sib / Sp",
                    "Par / Chi",
                    "Fare"
                },
                TextColor = foreground,
                TicklineColor = foreground,
                TitleColor = foreground
            });

            plotModel.Axes.Add(new CategoryAxis
            {
                Position = AxisPosition.Left,
                Key = "VerticalAxis",
                ItemsSource = new[]
                {
                    "Fare",
                    "Parents / Children",
                    "Siblings / Spouses",
                    "Age",
                    "Class",
                    "Survived"
                },
                TextColor = foreground,
                TicklineColor = foreground,
                TitleColor = foreground
            });

            plotModel.Axes.Add(new LinearColorAxis
            {
                // Pearson color scheme from blue over white to red.
                Palette = OxyPalettes.BlueWhiteRed31,
                Position = AxisPosition.Top,
                Minimum = -1,
                Maximum = 1,
                TicklineColor = OxyColors.Transparent
            });

            var heatMapSeries = new HeatMapSeries
            {
                X0 = 0,
                X1 = 5,
                Y0 = 0,
                Y1 = 5,
                XAxisKey = "HorizontalAxis",
                YAxisKey = "VerticalAxis",
                RenderMethod = HeatMapRenderMethod.Rectangles,
                LabelFontSize = 0.12,
                LabelFormatString = ".00"
            };

            plotModel.Series.Add(heatMapSeries);

            Diagram.Model = plotModel;

            return plotModel;
        }
    }
}
