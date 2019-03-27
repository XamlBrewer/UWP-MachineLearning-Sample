using Microsoft.ML;
using Mvvm;
using System.Collections.Generic;
using System.Threading.Tasks;
using XamlBrewer.Uwp.MachineLearningSample.Models;

namespace XamlBrewer.Uwp.MachineLearningSample.ViewModels
{
    internal class ClusteringPageViewModel : ViewModelBase
    {
        private ClusteringModel _model = new ClusteringModel();

        public ClusteringPageViewModel()
        { }

        public Task<IDataView> Load(string trainingDataPath)
        {
            return Task.Run(() =>
            {
                return _model.Load(trainingDataPath);
            });
        }

        public Task Build()
        {
            return Task.Run(() =>
            {
                _model.Build();
            });
        }

        public Task Train(IDataView trainingDataView)
        {
            return Task.Run(() =>
            {
                _model.Train(trainingDataView);
            });
        }

        public Task Save(string modelName)
        {
            return Task.Run(() =>
            {
                _model.Save(modelName);
            });
        }

        public async Task<IEnumerable<ClusteringPrediction>> Predict(IDataView dataView)
        {
            return await Task.Run(() =>
            {
                return _model.Predict(dataView);
            });
        }

        public Task<ClusteringPrediction> Predict(ClusteringData clusteringData)
        {
            return Task.Run(() =>
            {
                return _model.Predict(clusteringData);
            });
        }
    }
}
