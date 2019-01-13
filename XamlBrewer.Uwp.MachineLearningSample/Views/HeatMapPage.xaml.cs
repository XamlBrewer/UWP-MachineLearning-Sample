using Mvvm.Services;
using OxyPlot;
using System.Collections.Generic;
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

        private async void Page_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            // Prepare diagram
            var plotModel = PrepareDiagram();

            // Read data
            // var trainingDataPath = await MlDotNet.FilePath(@"ms-appx:///Data/Mall_Customers.csv");
            // var data = await ViewModel.Load(trainingDataPath);

            // Populate diagram
            // ...

            // Update diagram
            Diagram.InvalidatePlot();
        }

        private void AddItem(PlotModel plotModel, List<double> values, int slot)
        {
            // ...
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

            // ...

            Diagram.Model = plotModel;

            return plotModel;
        }
    }
}
