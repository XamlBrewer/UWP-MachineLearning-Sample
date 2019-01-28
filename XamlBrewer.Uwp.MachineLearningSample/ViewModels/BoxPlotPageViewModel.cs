using Microsoft.ML.Runtime.Api;
using Microsoft.ML.Runtime.Data;
using Mvvm;
using Mvvm.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using XamlBrewer.Uwp.MachineLearningSample.Models;

namespace XamlBrewer.Uwp.MachineLearningSample.ViewModels
{
    internal class BoxPlotPageViewModel : ViewModelBase
    {
        private LocalEnvironment _mlContext = new LocalEnvironment(seed: null); // v0.6;

        public Task<IEnumerable<RegressionPercentage>> LoadRegressionData()
        {
            return Task.Run(async() =>
            {
                var trainingDataPath = await MlDotNet.FilePath(@"ms-appx:///Data/2017-18_NBA_salary.csv");
                var reader = new TextLoader(_mlContext,
                                            new TextLoader.Arguments()
                                            {
                                                Separator = ",",
                                                HasHeader = true,
                                                Column = new[]
                                                    {
                                                    new TextLoader.Column("Ts", DataKind.R4, 9),
                                                    new TextLoader.Column("Orb", DataKind.R4, 12),
                                                    new TextLoader.Column("Drb", DataKind.R4, 13),
                                                    new TextLoader.Column("Trb", DataKind.R4, 14),
                                                    new TextLoader.Column("Ast", DataKind.R4, 15),
                                                    new TextLoader.Column("Stl", DataKind.R4, 16),
                                                    new TextLoader.Column("Blk", DataKind.R4, 17),
                                                    new TextLoader.Column("Tov", DataKind.R4, 18),
                                                    new TextLoader.Column("Usg", DataKind.R4, 19),
                                                    new TextLoader.Column("Age", DataKind.R4, 4)
                                                    }
                                            });

                var file = _mlContext.OpenInputFile(trainingDataPath);
                var src = new FileHandleSource(file);
                return reader.Read(src).AsEnumerable<RegressionPercentage>(_mlContext, false);
            });
        }

        public Task<IEnumerable<ClusteringRawData>> LoadClusteringData()
        {
            return Task.Run(async() =>
            {
                var trainingDataPath = await MlDotNet.FilePath(@"ms-appx:///Data/Mall_Customers.csv");
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
                return reader.Read(src).AsEnumerable<ClusteringRawData>(_mlContext, false);
            });
        }
    }
}

