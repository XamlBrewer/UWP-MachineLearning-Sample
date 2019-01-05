using Microsoft.ML.Legacy;
using Microsoft.ML.Legacy.Data;
using Microsoft.ML.Legacy.Models;
using XamlBrewer.Uwp.MachineLearningSample.Models;

namespace XamlBrewer.Uwp.MachineLearningSample.Helpers
{
    public class ModelEvaluator
    {
        /// <summary>
        /// Using passed testing data and model, it calculates model's accuracy.
        /// </summary>
        /// <returns>Accuracy of the model.</returns>
        public BinaryClassificationMetrics Evaluate(PredictionModel<BinaryClassificationData, BinaryClassificationPrediction> model, string testDataLocation)
        {
            var testData = new TextLoader(testDataLocation).CreateFrom<BinaryClassificationData>(useHeader: true, separator: ';');
            var metrics = new BinaryClassificationEvaluator().Evaluate(model, testData);
            return metrics;
        }
    }
}
