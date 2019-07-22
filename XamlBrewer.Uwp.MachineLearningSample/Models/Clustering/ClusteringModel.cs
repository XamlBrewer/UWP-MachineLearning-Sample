using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Storage;

namespace XamlBrewer.Uwp.MachineLearningSample.Models
{
    internal class ClusteringModel
    {
        private MLContext _mlContext = new MLContext(seed: null);

        public EstimatorChain<ClusteringPredictionTransformer<KMeansModelParameters>> Pipeline { get; private set; }

        public ITransformer Model { get; private set; }

        public ClusteringModel()
        { }

        public IDataView Load(string trainingDataPath)
        {
            var readerOptions = new TextLoader.Options()
            {
                Separators = new[] { ',' },
                HasHeader = true,
                Columns = new[]
                    {
                        // We're not interested in all columns:
                        // new TextLoader.Column("CustomerId", DataKind.Int32, 0), 
                        // new TextLoader.Column("Gender", DataKind.String, 1),
                        // new TextLoader.Column("Age", DataKind.Int32, 2),
                        new TextLoader.Column("AnnualIncome", DataKind.Single, 3),
                        new TextLoader.Column("SpendingScore", DataKind.Single, 4),
                    }
            };

            var dataView = _mlContext.Data.LoadFromTextFile(trainingDataPath, readerOptions);
            var sneakPeek = dataView.Preview(maxRows: 20);

            return dataView;
        }

        public void Build()
        {
            Pipeline = _mlContext.Transforms.Concatenate("Features", new[] { "AnnualIncome", "SpendingScore" })
                       .Append(_mlContext.Clustering.Trainers.KMeans(
                            featureColumnName: "Features",
                            numberOfClusters: 5));

            // Advanced:
            //Pipeline = _mlContext.Transforms.Concatenate("Features", new[] { "AnnualIncome", "SpendingScore" })
            //           .Append(_mlContext.Clustering.Trainers.KMeans(
            //                options: new KMeansTrainer.Options
            //                {
            //                    FeatureColumnName = "Features",
            //                    NumberOfClusters = 5,
            //                    InitializationAlgorithm = KMeansTrainer.InitializationAlgorithm.Random,
            //                    OptimizationTolerance = 1
            //                }));
        }

        public void Train(IDataView trainingDataView)
        {
            Model = Pipeline.Fit(trainingDataView);
        }

        public void Save(string modelName)
        {
            var storageFolder = ApplicationData.Current.LocalFolder;
            string modelPath = Path.Combine(storageFolder.Path, modelName);

            // _mlContext.Model is the Model Operations Catalog.
            _mlContext.Model.Save(Model, inputSchema: null, filePath: modelPath);

            // Read back:
            //var file = await storageFolder.GetFileAsync(modelName);
            //var filePath = file.Path;

            //Model = _mlContext.Model.Load(
            //    filePath: filePath,
            //    inputSchema: out _);
        }

        public IEnumerable<ClusteringPrediction> Predict(IDataView dataView)
        {
            var result = _mlContext.Data.CreateEnumerable<ClusteringPrediction>(
                data: Model.Transform(dataView),
                reuseRowObject: false);

            return result;
        }

        public ClusteringPrediction Predict(ClusteringData clusteringData)
        {
            var predictionEngine = _mlContext.Model.CreatePredictionEngine<ClusteringData, ClusteringPrediction>(Model);
            var result = predictionEngine.Predict(clusteringData);

            return result;
        }
    }
}
