using Microsoft.ML.Core.Data;
using Microsoft.ML.Runtime.Api;
using Microsoft.ML.Runtime.Data;
using Microsoft.ML.Runtime.KMeans;
using Mvvm;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using XamlBrewer.Uwp.MachineLearningSample.Models;

namespace XamlBrewer.Uwp.MachineLearningSample.ViewModels
{
    internal class ClusteringPageViewModel : ViewModelBase
    {
        private LocalEnvironment _mlContext = new LocalEnvironment(seed: null); // v0.6;

        public EstimatorChain<ClusteringPredictionTransformer<KMeansPredictor>> Pipeline { get; private set; }

        public ITransformer Model { get; private set; }

        public ClusteringPageViewModel()
        { }

        public Task<IDataView> Load(string trainingDataPath)
        {
            return Task.Run(() =>
            {
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

                var file = _mlContext.OpenInputFile(trainingDataPath);
                var src = new FileHandleSource(file);
                return reader.Read(src);
            });
        }

        public Task Build()
        {
            return Task.Run(() =>
            {
                Pipeline = new ConcatEstimator(_mlContext, "Features", "AnnualIncome", "SpendingScore")
                    .Append(new KMeansPlusPlusTrainer(
                        env: _mlContext,
                        featureColumn: "Features",
                        clustersCount: 5,
                        advancedSettings: (a) => 
                            {
                                // a.AccelMemBudgetMb = 1;
                                // a.MaxIterations = 1;
                                // a. ...
                            } 
                        ));
            });
        }



        public Task Train(IDataView trainingDataView)
        {
            return Task.Run(() =>
            {
                Model = Pipeline.Fit(trainingDataView);
            });
        }

        public Task Save(string modelName)
        {
            return Task.Run(() =>
            {
                var storageFolder = ApplicationData.Current.LocalFolder;
                using (var fs = new FileStream(Path.Combine(storageFolder.Path, modelName), FileMode.Create, FileAccess.Write, FileShare.Write))
                    Model.SaveTo(_mlContext, fs);
            });
        }

        public async Task<IEnumerable<ClusteringPrediction>> Predict(IDataView dataView)
        {
            return await Task.Run(() =>
            {
                var result = Model.Transform(dataView).AsEnumerable<ClusteringPrediction>(_mlContext, false);
                return result;
            });
        }

        public Task<ClusteringPrediction> Predict(ClusteringData clusteringData)
        {
            return Task.Run(() =>
            {
                var predictionFunc = Model.MakePredictionFunction<ClusteringData, ClusteringPrediction>(_mlContext);
                return predictionFunc.Predict(clusteringData);
            });
        }
    }
}
