using Microsoft.ML.Core.Data;
using Microsoft.ML.Runtime.Api;
using Microsoft.ML.Runtime.Data;
using Microsoft.ML.Runtime.KMeans;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Storage;

namespace XamlBrewer.Uwp.MachineLearningSample.Models
{
    internal class ClusteringModel
    {
        private LocalEnvironment _mlContext = new LocalEnvironment(seed: null); // v0.6;

        public EstimatorChain<ClusteringPredictionTransformer<KMeansPredictor>> Pipeline { get; private set; }

        public ITransformer Model { get; private set; }

        public ClusteringModel()
        { }

        public IDataView Load(string trainingDataPath)
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
        }

        public void Build()
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
        }

        public void Train(IDataView trainingDataView)
        {
            Model = Pipeline.Fit(trainingDataView);
        }

        public void Save(string modelName)
        {
            var storageFolder = ApplicationData.Current.LocalFolder;
            using (var fs = new FileStream(
                    Path.Combine(storageFolder.Path, modelName),
                    FileMode.Create,
                    FileAccess.Write,
                    FileShare.Write))
            {
                Model.SaveTo(_mlContext, fs);
            }
        }

        public IEnumerable<ClusteringPrediction> Predict(IDataView dataView)
        {
            var result = Model.Transform(dataView).AsEnumerable<ClusteringPrediction>(_mlContext, false);
            return result;
        }

        public ClusteringPrediction Predict(ClusteringData clusteringData)
        {
            var predictionFunc = Model.MakePredictionFunction<ClusteringData, ClusteringPrediction>(_mlContext);
            return predictionFunc.Predict(clusteringData);
        }
    }
}
