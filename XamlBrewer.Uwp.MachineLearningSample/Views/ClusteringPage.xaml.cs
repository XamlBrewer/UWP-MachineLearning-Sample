using Microsoft.ML.Core.Data;
using Microsoft.ML.Runtime.Api;
using Microsoft.ML.Runtime.Data;
using Microsoft.ML.Runtime.KMeans;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.Storage;
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
                    OxyColors.Red,
                    OxyColors.Yellow,
                    OxyColors.White,
                    OxyColors.Green,
                    OxyColors.Blue,
                    OxyColors.HotPink
                };

        public ClusteringPage()
        {
            this.InitializeComponent();
            Loaded += ClusteringPage_Loaded;
        }

        private async void ClusteringPage_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            // https://github.com/dotnet/machinelearning-samples/commit/24ccd37c1f3671b3a3ea709d59449a52550b6aef#diff-398ac923421ec3fef2555299173117ba
            // https://blogs.msdn.microsoft.com/dotnet/2018/10/08/announcing-ml-net-0-6-machine-learning-net/
            // https://www.c-sharpcorner.com/article/getting-started-with-machine-learning-net-for-clustering-model/
            // http://docs.oxyplot.org/en/latest/getting-started/hello-uwp.html

            SettingUpBox.IsChecked = false;
            LoadingBox.IsChecked = false;
            TrainingBox.IsChecked = false;
            CalculatingBox.IsChecked = false;
            PlottingBox.IsChecked = false;

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
            //var src = new FileHandleSource(file);
            //var trainingDataView = reader.Read(src);
            LoadingBox.IsChecked = true;
            StorageFile modelFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri(@"ms-appx:///Assets/Mall_Customers.csv"));
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            StorageFile sf2 = await modelFile.CopyAsync(storageFolder, modelFile.Name, NameCollisionOption.ReplaceExisting);
            var file = _mlContext.OpenInputFile(sf2.Path);
            var src = new FileHandleSource(file);
            var trainingDataView = reader.Read(src);

            // Create and train the model      
            TrainingBox.IsChecked = true;
            _model = pipeline.Fit(trainingDataView);

            // Could save the model here.

            // Empty diagram
            var foreground = OxyColors.LightSteelBlue;
            var plotModel = new PlotModel();
            var linearAxisX = new LinearAxis { Position = AxisPosition.Bottom };
            linearAxisX.Title = "Spending Score";
            linearAxisX.TextColor = foreground;
            linearAxisX.TicklineColor = foreground;
            linearAxisX.TitleColor = foreground;
            plotModel.PlotAreaBorderThickness = new OxyThickness(1, 0, 0, 1);
            plotModel.PlotAreaBorderColor = foreground;
            plotModel.Axes.Add(linearAxisX);
            var linearAxisY = new LinearAxis();
            linearAxisY.Maximum = 140;
            linearAxisY.Title = "Annual Income";
            linearAxisY.TextColor = foreground;
            linearAxisY.TicklineColor = foreground;
            linearAxisY.TitleColor = foreground;
            plotModel.Axes.Add(linearAxisY);
            Diagram.Model = plotModel;
            linearAxisY.Reset();

            CalculatingBox.IsChecked = true;
            var result = _model.Transform(trainingDataView);

            PlottingBox.IsChecked = true;
            var outp = result.AsEnumerable<ClusterPrediction>(_mlContext, false);
            var enu = outp.GetEnumerator();
            while (enu.MoveNext())
            {
                var prediction = enu.Current;
                Debug.WriteLine("{0} {1} {2}", prediction.AnnualIncome, prediction.SpendingScore, (int)prediction.PredictedCluster);
                var annotation = new PointAnnotation { Shape = MarkerType.Circle, X = prediction.SpendingScore, Y = prediction.AnnualIncome, Fill = _colors[(int)prediction.PredictedCluster] };
                plotModel.Annotations.Add(annotation);
            }

            Diagram.InvalidatePlot();
        }

        private void Calculate_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            int.TryParse(AnnualIncomeInput.Text, out int annualIncome);
            int.TryParse(SpendingScoreInput.Text, out int spendingScore);
            var predictionFunc = _model.MakePredictionFunction<ClusterInput, ClusterPrediction>(_mlContext);
            var output = predictionFunc.Predict(new ClusterInput { AnnualIncome = annualIncome, SpendingScore = spendingScore });
            var annotation = new PointAnnotation { Shape = MarkerType.Diamond, X = output.SpendingScore, Y = output.AnnualIncome, Fill = _colors[(int)output.PredictedCluster], TextColor = OxyColors.LightSteelBlue, Text = "Here" };
            Diagram.Model.Annotations.Add(annotation);
            Diagram.InvalidatePlot();
        }
    }
}
