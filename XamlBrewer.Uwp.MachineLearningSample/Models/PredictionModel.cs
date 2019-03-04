using Microsoft.ML;

namespace XamlBrewer.Uwp.MachineLearningSample.Models
{
    internal class PredictionModel<TSrc, TDst>
        where TSrc : class
        where TDst : class, new()
    {
        public PredictionModel(MLContext mlContext, ITransformer transformer)
        {
            Transformer = transformer;
            Engine = Transformer.CreatePredictionEngine<TSrc, TDst>(mlContext);
        }

        public ITransformer Transformer { get; }
        public PredictionEngine<TSrc, TDst> Engine { get; }
    }
}
