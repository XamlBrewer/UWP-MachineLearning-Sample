using Microsoft.ML.Runtime.Api;
using Microsoft.ML.Runtime.Data;
using Mvvm;
using Mvvm.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using XamlBrewer.Uwp.MachineLearningSample.Models;

namespace XamlBrewer.Uwp.MachineLearningSample.ViewModels
{
    internal class HeatMapPageViewModel : ViewModelBase
    {
        private LocalEnvironment _mlContext = new LocalEnvironment(seed: null); // v0.6;

        public Task<IEnumerable<CorrelationData>> LoadCorrelationData()
        {
            return Task.Run(async () =>
            {
                var trainingDataPath = await MlDotNet.FilePath(@"ms-appx:///Data/titanic.csv");
                var reader = new TextLoader(_mlContext,
                                            new TextLoader.Arguments()
                                            {
                                                Separator = ",",
                                                HasHeader = true,
                                                Column = new[]
                                                    {
                                                    new TextLoader.Column("Survived", DataKind.R4, 1),
                                                    new TextLoader.Column("PClass", DataKind.R4, 2),
                                                    new TextLoader.Column("Age", DataKind.R4, 5),
                                                    new TextLoader.Column("SibSp", DataKind.R4, 6),
                                                    new TextLoader.Column("Parch", DataKind.R4, 7),
                                                    new TextLoader.Column("Fare", DataKind.R4, 9)
                                                    }
                                            });

                var file = _mlContext.OpenInputFile(trainingDataPath);
                var src = new FileHandleSource(file);
                return reader.Read(src).AsEnumerable<CorrelationData>(_mlContext, false);
            });
        }
    }
}

