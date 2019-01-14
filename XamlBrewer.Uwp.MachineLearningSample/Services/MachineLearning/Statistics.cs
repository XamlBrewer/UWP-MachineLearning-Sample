using System;
using System.Collections.Generic;

namespace XamlBrewer.Uwp.MachineLearningSample
{
    internal static class Statistics
    {
        /// <summary>
        /// Estimates the median value from the sorted data array (ascending).
        /// Approximately median-unbiased regardless of the sample distribution (R8).
        /// </summary>
        /// <param name="data">Sample array, must be sorted ascendingly.</param>
        /// <remarks>Original Source: https://github.com/mathnet/mathnet-numerics/blob/master/src/Numerics/Statistics/SortedArrayStatistics.cs </remarks>
        public static double Median(this double[] data)
        {
            if (data.Length == 0)
            {
                return double.NaN;
            }

            var k = data.Length / 2;

            return data.Length % 2 == 1
                ? data[k]
                : (data[k - 1] + data[k]) / 2.0;
        }

        /// <summary>
        /// Estimates the first quartile value from the sorted data array (ascending).
        /// Approximately median-unbiased regardless of the sample distribution (R8).
        /// </summary>
        /// <param name="data">Sample array, must be sorted ascendingly.</param>
        /// <remarks>Original Source: https://github.com/mathnet/mathnet-numerics/blob/master/src/Numerics/Statistics/SortedArrayStatistics.cs </remarks>
        public static double LowerQuartile(this double[] data)
        {
            return Quantile(data, 0.25d);
        }

        /// <summary>
        /// Estimates the third quartile value from the sorted data array (ascending).
        /// Approximately median-unbiased regardless of the sample distribution (R8).
        /// </summary>
        /// <param name="data">Sample array, must be sorted ascendingly.</param>
        /// <remarks>Original Source: https://github.com/mathnet/mathnet-numerics/blob/master/src/Numerics/Statistics/SortedArrayStatistics.cs </remarks>
        public static double UpperQuartile(this double[] data)
        {
            return Quantile(data, 0.75d);
        }

        /// <summary>
        /// Estimates the tau-th quantile from the sorted data array (ascending).
        /// The tau-th quantile is the data value where the cumulative distribution
        /// function crosses tau.
        /// Approximately median-unbiased regardless of the sample distribution (R8).
        /// </summary>
        /// <param name="data">Sample array, must be sorted ascendingly.</param>
        /// <param name="tau">Quantile selector, between 0.0 and 1.0 (inclusive).</param>
        /// <remarks>
        /// R-8, SciPy-(1/3,1/3):
        /// Linear interpolation of the approximate medians for order statistics.
        /// When tau &lt; (2/3) / (N + 1/3), use x1. When tau &gt;= (N - 1/3) / (N + 1/3), use xN.
        /// </remarks>
        /// <remarks>Original Source: https://github.com/mathnet/mathnet-numerics/blob/master/src/Numerics/Statistics/SortedArrayStatistics.cs </remarks>
        public static double Quantile(double[] data, double tau)
        {
            if (tau < 0d || tau > 1d || data.Length == 0)
            {
                return double.NaN;
            }

            if (tau == 0d || data.Length == 1)
            {
                return data[0];
            }

            if (tau == 1d)
            {
                return data[data.Length - 1];
            }

            double h = (data.Length + 1 / 3d) * tau + 1 / 3d;
            var hf = (int)h;
            return hf < 1 ? data[0]
                : hf >= data.Length ? data[data.Length - 1]
                    : data[hf - 1] + (h - hf) * (data[hf] - data[hf - 1]);
        }

        /// <summary>
        /// Computes the Pearson Product-Moment Correlation coefficient.
        /// </summary>
        /// <param name="dataA">Sample data A.</param>
        /// <param name="dataB">Sample data B.</param>
        /// <returns>The Pearson product-moment correlation coefficient.</returns>
        /// <remarks>Original Source: https://github.com/mathnet/mathnet-numerics/blob/master/src/Numerics/Statistics/Correlation.cs </remarks>
        public static double Pearson(IEnumerable<double> dataA, IEnumerable<double> dataB)
        {
            var n = 0;
            var r = 0.0;

            var meanA = 0d;
            var meanB = 0d;
            var varA = 0d;
            var varB = 0d;

            using (IEnumerator<double> ieA = dataA.GetEnumerator())
            using (IEnumerator<double> ieB = dataB.GetEnumerator())
            {
                while (ieA.MoveNext())
                {
                    if (!ieB.MoveNext())
                    {
                        throw new ArgumentOutOfRangeException(nameof(dataB), "Array too short.");
                    }

                    var currentA = ieA.Current;
                    var currentB = ieB.Current;

                    var deltaA = currentA - meanA;
                    var scaleDeltaA = deltaA / ++n;

                    var deltaB = currentB - meanB;
                    var scaleDeltaB = deltaB / n;

                    meanA += scaleDeltaA;
                    meanB += scaleDeltaB;

                    varA += scaleDeltaA * deltaA * (n - 1);
                    varB += scaleDeltaB * deltaB * (n - 1);
                    r += (deltaA * deltaB * (n - 1)) / n;
                }

                if (ieB.MoveNext())
                {
                    throw new ArgumentOutOfRangeException(nameof(dataA), "Array too short.");
                }
            }

            return r / Math.Sqrt(varA * varB);
        }
    }
}
