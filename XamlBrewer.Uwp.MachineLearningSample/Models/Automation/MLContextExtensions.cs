using Microsoft.ML;
using Microsoft.ML.Transforms;
using System;
using System.Linq;

namespace XamlBrewer.Uwp.MachineLearningSample.Models
{
    public static partial class MLContextExtensions
    {
        /// <summary>
        /// Divides the numeric Valuation in two 'buckets' and transforms it to a Boolean.
        /// </summary>
        public static IEstimator<ITransformer> ValuationToBoolLabelNormalizer(this MLContext mLContext)
        {
            var normalizer = mLContext.Transforms.NormalizeBinning(
                outputColumnName: "Valuation", maximumBinCount: 2);

            return normalizer.Append(mLContext.Transforms.CustomMapping(new MapValuationToBool().GetMapping(), "MapValuationToBool"));
        }

        private class FloatToBoolInput
        {
            public float Valuation { get; set; }
        }

        private class FloatToBoolOutput
        {
            public bool Label { get; set; }

            public static FloatToBoolOutput True = new FloatToBoolOutput() { Label = true };
            public static FloatToBoolOutput False = new FloatToBoolOutput() { Label = false };
        }

        [CustomMappingFactoryAttribute("MapValuationToBool")]
        private class MapValuationToBool : CustomMappingFactory<FloatToBoolInput, FloatToBoolOutput>
        {
            public override Action<FloatToBoolInput, FloatToBoolOutput> GetMapping()
            {
                return (input, output) =>
                {
                    if (input.Valuation > 0)
                        output.Label = true;
                    else
                        output.Label = false;
                };
            }
        }
    }
}
