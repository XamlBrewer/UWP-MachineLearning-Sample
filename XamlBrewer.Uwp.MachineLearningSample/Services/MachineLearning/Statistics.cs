using System.Collections.Generic;

namespace XamlBrewer.Uwp.MachineLearningSample
{
    internal static class Statistics
    {
        public static double Median(this IEnumerable<double> values, bool isSorted = false)
        {
            var sortedValues = new List<double>(values);

            if (!isSorted)
            {
                sortedValues.Sort();
            }

            var count = sortedValues.Count;
            if (count % 2 == 1)
            {
                return sortedValues[(count - 1) / 2];
            }

            return 0.5 * sortedValues[count / 2] + 0.5 * sortedValues[(count / 2) - 1];
        }
    }
}
