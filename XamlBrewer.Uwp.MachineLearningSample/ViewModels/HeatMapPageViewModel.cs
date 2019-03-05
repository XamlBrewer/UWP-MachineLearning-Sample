using Microsoft.ML;
using Microsoft.ML.Data;
using Mvvm;
using Mvvm.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace XamlBrewer.Uwp.MachineLearningSample.ViewModels
{
    internal class HeatMapPageViewModel : ViewModelBase
    {
        private MLContext _mlContext = new MLContext(seed: null); // v0.6;

        public Task<List<List<double>>> LoadCorrelationData()
        {
            return Task.Run(async () =>
            {
                var trainingDataPath = await MlDotNet.FilePath(@"ms-appx:///Data/titanic.csv");
                var readerOptions = new TextLoader.Options()
                {
                    Separators = new[] { ',' },
                    HasHeader = true,
                    AllowQuoting = true,
                    Columns = new[]
                        {
                        new TextLoader.Column("Survived", DataKind.Single, 1),
                        new TextLoader.Column("PClass", DataKind.Single, 2),
                        new TextLoader.Column("Age", DataKind.Single, 5),
                        new TextLoader.Column("SibSp", DataKind.Single, 6),
                        new TextLoader.Column("Parch", DataKind.Single, 7),
                        new TextLoader.Column("Fare", DataKind.Single, 9)
                        }
                };

                var dataView = _mlContext.Data.LoadFromTextFile(trainingDataPath, readerOptions);
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

