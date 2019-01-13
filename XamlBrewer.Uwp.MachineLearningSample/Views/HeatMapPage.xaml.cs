using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
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

        private void Page_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            // Prepare diagram
            var plotModel = PrepareDiagram();

            // Read data
            // var trainingDataPath = await MlDotNet.FilePath(@"ms-appx:///Data/Mall_Customers.csv");
            // var data = await ViewModel.Load(trainingDataPath);

            // Populate diagram
            var rand = new Random();
            var data = new double[7, 7];
            for (int x = 0; x < 7; ++x)
            {
                for (int y = 0; y < x; ++y)
                {
                    // Pearson range from -1 to 1.
                    var value = (double)rand.Next(200) / 100 - 1;
                    data[6 - x, y] = value;
                    data[6 - y, x] = value;
                }

                data[x, 6 - x] = 1;
            }

            (plotModel.Series[0] as HeatMapSeries).Data = data;

            // Update diagram
            Diagram.InvalidatePlot();
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

            plotModel.Axes.Add(new CategoryAxis
            {
                Position = AxisPosition.Bottom,
                Key = "HorizontalAxis",
                ItemsSource = new[]
                {
                    "Pride",
                    "Greed",
                    "Lust",
                    "Envy",
                    "Gluttony",
                    "Wrath",
                    "Sloth"
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
                    "Sloth",
                    "Wrath",
                    "Gluttony",
                    "Envy",
                    "Lust",
                    "Greed",
                    "Pride"
                },
                TextColor = foreground,
                TicklineColor = foreground,
                TitleColor = foreground
            });

            plotModel.Axes.Add(new LinearColorAxis
            {
                // Pearson color scheme from blue over white to red.
                Palette = OxyPalettes.BlueWhiteRed31
            });

            var heatMapSeries = new HeatMapSeries
            {
                X0 = 0,
                X1 = 6,
                Y0 = 0,
                Y1 = 6,
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
