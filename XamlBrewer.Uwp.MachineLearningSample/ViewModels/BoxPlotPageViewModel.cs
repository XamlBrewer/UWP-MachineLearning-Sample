using Microsoft.ML;
using Microsoft.ML.Data;
using Mvvm;
using Mvvm.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace XamlBrewer.Uwp.MachineLearningSample.ViewModels
{
    internal class BoxPlotPageViewModel : ViewModelBase
    {
        private MLContext _mlContext = new MLContext(seed: null);

        public Task<List<List<double>>> LoadRegressionData()
        {
            return Task.Run(async () =>
            {
                var trainingDataPath = await MlDotNet.FilePath(@"ms-appx:///Data/2017-18_NBA_salary.csv");
                var reader = _mlContext.Data.CreateTextLoader(
                                            new TextLoader.Options()
                                            {
                                                Separators = new[] { ',' },
                                                HasHeader = true,
                                                Columns = new[]
                                                    {
                                                    new TextLoader.Column("Ts", DataKind.Single, 9),
                                                    new TextLoader.Column("Orb", DataKind.Single, 12),
                                                    new TextLoader.Column("Drb", DataKind.Single, 13),
                                                    new TextLoader.Column("Trb", DataKind.Single, 14),
                                                    new TextLoader.Column("Ast", DataKind.Single, 15),
                                                    new TextLoader.Column("Stl", DataKind.Single, 16),
                                                    new TextLoader.Column("Blk", DataKind.Single, 17),
                                                    new TextLoader.Column("Tov", DataKind.Single, 18),
                                                    new TextLoader.Column("Usg", DataKind.Single, 19),
                                                    new TextLoader.Column("Age", DataKind.Single, 4)
                                                    }
                                            });

                var dataView = reader.Load(trainingDataPath);
                var result = new List<List<double>>();
                for (int i = 0; i < dataView.Schema.Count; i++)
                {
                    var column = dataView.Schema[i];
                    result.Add(dataView.GetColumn<float>(_mlContext, column.Name).Select(f => (double)f).ToList());
                }

                return result;
            });
        }

        public Task<List<List<double>>> LoadClusteringData()
        {
            return Task.Run(async () =>
            {
                var trainingDataPath = await MlDotNet.FilePath(@"ms-appx:///Data/Mall_Customers.csv");
                var reader = _mlContext.Data.CreateTextLoader(
                                            new TextLoader.Options()
                                            {
                                                Separators = new[] { ',' },
                                                HasHeader = true,
                                                Columns = new[]
                                                    {
                                    new TextLoader.Column("Age", DataKind.Single, 2),
                                    new TextLoader.Column("AnnualIncome", DataKind.Single, 3),
                                    new TextLoader.Column("SpendingScore", DataKind.Single, 4),
                                                    }
                                            });

                var dataView = reader.Load(trainingDataPath);
                var result = new List<List<double>>();
                for (int i = 0; i < dataView.Schema.Count; i++)
                {
                    var column = dataView.Schema[i];
                    result.Add(dataView.GetColumn<float>(_mlContext, column.Name).Select(f => (double)f).ToList());
                }

                return result;
            });
        }
    }
}

