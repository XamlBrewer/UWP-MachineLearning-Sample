using Microsoft.ML.Data;
using Microsoft.ML.Runtime.Data;
using Mvvm;
using Mvvm.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace XamlBrewer.Uwp.MachineLearningSample.ViewModels
{
    internal class BoxPlotPageViewModel : ViewModelBase
    {
        private LocalEnvironment _mlContext = new LocalEnvironment(seed: null); // v0.6;

        public Task<List<List<double>>> LoadRegressionData()
        {
            return Task.Run(async () =>
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
                var dataView = reader.Read(src);
                var result = new List<List<double>>();
                for (int i = 0; i < dataView.Schema.ColumnCount; i++)
                {
                    var columnName = dataView.Schema.GetColumnName(i);
                    result.Add(dataView.GetColumn<float>(_mlContext, columnName).Select(f => (double)f).ToList());
                }

                return result;
            });
        }

        public Task<List<List<double>>> LoadClusteringData()
        {
            return Task.Run(async () =>
            {
                var trainingDataPath = await MlDotNet.FilePath(@"ms-appx:///Data/Mall_Customers.csv");
                var reader = new TextLoader(_mlContext,
                                            new TextLoader.Arguments()
                                            {
                                                Separator = ",",
                                                HasHeader = true,
                                                Column = new[]
                                                    {
                                                    new TextLoader.Column("Age", DataKind.R4, 2),
                                                    new TextLoader.Column("AnnualIncome", DataKind.R4, 3),
                                                    new TextLoader.Column("SpendingScore", DataKind.R4, 4),
                                                    }
                                            });

                var file = _mlContext.OpenInputFile(trainingDataPath);
                var src = new FileHandleSource(file);
                var dataView = reader.Read(src);
                var result = new List<List<double>>();
                for (int i = 0; i < dataView.Schema.ColumnCount; i++)
                {
                    var columnName = dataView.Schema.GetColumnName(i);
                    result.Add(dataView.GetColumn<float>(_mlContext, columnName).Select(f => (double)f).ToList());
                }

                return result;
            });
        }
    }
}

