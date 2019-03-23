using Microsoft.ML.Data;
using Mvvm;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XamlBrewer.Uwp.MachineLearningSample.Models;

namespace XamlBrewer.Uwp.MachineLearningSample.ViewModels
{
    internal class RecommendationPageViewModel : ViewModelBase
    {
        private RecommendationModel _model = new RecommendationModel();

        public List<string> TravelerTypes { get; private set; }

        public List<string> Hotels { get; private set; }

        public Task<IEnumerable<RecommendationData>> Load(string trainingDataPath)
        {
            return Task.Run(() =>
            {
                var data = _model.Load(trainingDataPath);
                TravelerTypes = data.Select(r => r.TravelerType).Distinct().ToList();
                TravelerTypes.Sort();
                Hotels = data.Select(r => r.Hotel).Distinct().ToList();
                Hotels.Sort();

                return data;
            });
        }

        public Task Build()
        {
            return Task.Run(() =>
            {
                _model.Build();
            });
        }

        public Task<RegressionMetrics> Evaluate(string testDataPath)
        {
            return Task.Run(() =>
            {
                return _model.Evaluate(testDataPath);
            });
        }

        public Task Save(string modelName)
        {
            return Task.Run(() =>
            {
                _model.Save(modelName);
            });
        }

        public Task<RecommendationPrediction> Predict(RecommendationData recommendationData)
        {
            return Task.Run(() =>
            {
                return _model.Predict(recommendationData);
            });
        }

        public Task<IEnumerable<RecommendationPrediction>> Predict(IEnumerable<RecommendationData> recommendationData)
        {
            return Task.Run(() =>
            {
                return _model.Predict(recommendationData);
            });
        }
    }
}
