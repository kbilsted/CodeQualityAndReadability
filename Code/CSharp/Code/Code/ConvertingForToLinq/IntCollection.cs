using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Code.ConvertingForToLinq
{
    public class IntIntervalCollection
    {
        List<Span<int>> Configurations = new List<Span<int>>();
        List<Span<int>> UncoveredIntervals;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public IntIntervalCollection()
        {
            var allInterval = new Span<int>(int.MinValue, int.MaxValue);
            var max = new Span<int>(int.MaxValue, int.MaxValue);
            UncoveredIntervals = new List<Span<int>>() { allInterval, max, };
        }

        public void AddConfiguration(Span<int> configuration)
        {
            int? pos = FindOverlap(configuration);
            if (pos.HasValue)
            {
                var tmp = UncoveredIntervals[pos.Value];
                UncoveredIntervals.RemoveAt(pos.Value);

                var splitIntervals = tmp.RemoveInterval(configuration);
                UncoveredIntervals.InsertRange(pos.Value, splitIntervals);
            }
            Configurations.Add(configuration);
        }

        public void AddConfiguration2(Span<int> configuration)
        {
            UncoveredIntervals =
                UncoveredIntervals.Replace(
                    x => x.IsOverlapOf(configuration),
                    x => x.RemoveInterval(configuration))
                    .ToList();

            Configurations.Add(configuration);
        }

        int? FindOverlap(Span<int> configuration)
        {
            for (int i = 0; i < UncoveredIntervals.Count; i++)
            {
                if (UncoveredIntervals[i].IsOverlapOf(configuration))
                    return i;
            }
            return null;
        }

        public string Explain()
        {
            return string.Join(",", UncoveredIntervals);
        }

        public bool IsFullyCovered()
        {
            return !UncoveredIntervals.Any();
        }
    }
}
