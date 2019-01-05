using Microsoft.ML.Core.Data;
using Microsoft.ML.Runtime.Api;
using Microsoft.ML.Runtime.Data;
using Microsoft.ML.Runtime.KMeans;
using Mvvm.Services;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.UI.Xaml.Controls;
using XamlBrewer.Uwp.MachineLearningSample.Models;

namespace XamlBrewer.Uwp.MachineLearningSample
{
    public sealed partial class ClusteringPage : Page
    {
        private ITransformer _model;
        private LocalEnvironment _mlContext;
        private List<OxyColor> _colors = new List<OxyColor>
                {
                    OxyColors.Black,
                    OxyColors.LightCoral,
                    OxyColors.Khaki,
                    OxyColors.SlateBlue,
                    OxyColors.DarkCyan,
                    OxyColors.LightSkyBlue,
                    OxyColors.HotPink
                };

        public ClusteringPage()
        {
            this.InitializeComponent();
            Loaded += Page_Loaded;
        }

        private async void Page_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            DatasetBox.IsChecked = false;
            SettingUpBox.IsChecked = false;
            TrainingBox.IsChecked = false;
            CalculatingBox.IsChecked = false;
            PlottingBox.IsChecked = false;

            //Preparing the files.
            DatasetBox.IsChecked = true;
            var trainingDataPath = await MlDotNet.FilePath(@"ms-appx:///Data/Mall_Customers.csv");

            //Create the MLContext
            SettingUpBox.IsChecked = true;
            _mlContext = new LocalEnvironment(seed: null); // v0.6
            var reader = new TextLoader(_mlContext,
                                        new TextLoader.Arguments()
                                        {
                                            Separator = ",",
                                            HasHeader = true,
                                            Column = new[]
                                                {
                                                    new TextLoader.Column("CustomerId", DataKind.I4, 0),
                                                    new TextLoader.Column("Gender", DataKind.Text, 1),
                                                    new TextLoader.Column("Age", DataKind.I4, 2),
                                                    new TextLoader.Column("AnnualIncome", DataKind.R4, 3),
                                                    new TextLoader.Column("SpendingScore", DataKind.R4, 4),
                                                }
                                        });

            // Transform your data and add a learner
            var pipeline = new ConcatEstimator(_mlContext, "Features", "AnnualIncome", "SpendingScore")
                   .Append(new KMeansPlusPlusTrainer(_mlContext, "Features", clustersCount: 5));

            //Load training data
            //var file = mlContext.OpenInputFile(@"ms-appx:///Assets/Mall_Customers.csv");
            var file = _mlContext.OpenInputFile(trainingDataPath);
            var src = new FileHandleSource(file);
            var trainingDataView = reader.Read(src);

            // Create and train the model      
            TrainingBox.IsChecked = true;
            _model = pipeline.Fit(trainingDataView);

            // Could save the model here.
            // ...

            // Produce the results.
            CalculatingBox.IsChecked = true;
            var result = _model.Transform(trainingDataView);

            // Draw the results.
            PlottingBox.IsChecked = true;
            var foreground = OxyColors.LightSteelBlue;
            var plotModel = new PlotModel
            {
                PlotAreaBorderThickness = new OxyThickness(1, 0, 0, 1),
                PlotAreaBorderColor = foreground
            };

            var linearAxisX = new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Title = "Spending Score",
                TextColor = foreground,
                TicklineColor = foreground,
                TitleColor = foreground
            };

            plotModel.Axes.Add(linearAxisX);
            var linearAxisY = new LinearAxis
            {
                Maximum = 140,
                Title = "Annual Income",
                TextColor = foreground,
                TicklineColor = foreground,
                TitleColor = foreground
            };
            plotModel.Axes.Add(linearAxisY);
            Diagram.Model = plotModel;
            linearAxisY.Reset();
            var outp = result.AsEnumerable<ClusteringPrediction>(_mlContext, false);
            var enu = outp.GetEnumerator();
            while (enu.MoveNext())
            {
                var prediction = enu.Current;
                var annotation = new PointAnnotation
                {
                    Shape = MarkerType.Circle,
                    X = prediction.SpendingScore,
                    Y = prediction.AnnualIncome,
                    Fill = _colors[(int)prediction.PredictedCluster]
                };
                plotModel.Annotations.Add(annotation);
            }

            Diagram.InvalidatePlot();
        }

        private void Calculate_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            int.TryParse(AnnualIncomeInput.Text, out int annualIncome);
            int.TryParse(SpendingScoreInput.Text, out int spendingScore);
            var predictionFunc = _model.MakePredictionFunction<ClusteringData, ClusteringPrediction>(_mlContext);
            var output = predictionFunc.Predict(new ClusteringData { AnnualIncome = annualIncome, SpendingScore = spendingScore });
            var annotation = new PointAnnotation { Shape = MarkerType.Diamond, X = output.SpendingScore, Y = output.AnnualIncome, Fill = _colors[(int)output.PredictedCluster], TextColor = OxyColors.LightSteelBlue, Text = "Here" };
            Diagram.Model.Annotations.Add(annotation);
            Diagram.InvalidatePlot();
        }
    }
}
