using Microsoft.ML;
using Microsoft.ML.Transforms;
using System;
using System.Linq;

namespace XamlBrewer.Uwp.MachineLearningSample.Models
{
    public static class MLContextExtensions
    {
        /// <summary>
        /// Divides the numeric Label in two 'buckets' and transforms it to a Boolean.
        /// </summary>
        public static IEstimator<ITransformer> FloatToBoolLabelNormalizer(this MLContext mLContext)
        {
            var normalizer = mLContext.Transforms.Normalize(
                new NormalizingEstimator.BinningColumnOptions(outputColumnName: "Label", numBins: 2));

            return normalizer.Append(mLContext.Transforms.CustomMapping(new MapFloatToBool().GetMapping(), "MapFloatToBool"));
        }

        private class LabelInput
        {
            public float Label { get; set; }
        }

        private class LabelOutput
        {
            public bool Label { get; set; }

            public static LabelOutput True = new LabelOutput() { Label = true };
            public static LabelOutput False = new LabelOutput() { Label = false };
        }

        [CustomMappingFactoryAttribute("MapFloatToBool")]
        private class MapFloatToBool : CustomMappingFactory<LabelInput, LabelOutput>
        {
            public override Action<LabelInput, LabelOutput> GetMapping()
            {
                return (input, output) =>
                {
                    if (input.Label > 0)
                        output.Label = true;
                    else
                        output.Label = false;
                };
            }
        }
    }
}
