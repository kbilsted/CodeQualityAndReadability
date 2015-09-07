using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Code.ConvertingForToLinq
{
    public class Span<T> where T : IComparable<T>
    {
        public readonly T From, Upto;

        public Span(T from, T upto)
        {
            if (Compare.Greater(from, upto))
                throw new ArgumentException("From is bigger than To");
            From = from;
            Upto = upto;
        }

        public bool IsOverlapOf(Span<T> other)
        {
            var isOutsideBounds = other.From.CompareTo(Upto) >= 0 || From.CompareTo(other.Upto) >= 0;
            return !isOutsideBounds;
        }

        public List<Span<T>> RemoveInterval(Span<T> interval)
        {
            if (!IsOverlapOf(interval))
                throw new ArgumentException("Cannot remove interval as there is no overlap");

            var result = new List<Span<T>>();
            if (Compare.LessOrEquals(From, interval.From))
                result.Add(new Span<T>(From, interval.From));

            if (Compare.Greater(Upto, interval.Upto))
                result.Add(new Span<T>(interval.Upto, Upto));

            return result;
        }
    }

    public static class Compare
    {
        public static bool LessOrEquals<T>(T a, T b) where T : IComparable<T>
        {
            return a.CompareTo(b) <= 0;
        }

        public static bool GreaterOrEquals<T>(T a, T b) where T : IComparable<T>
        {
            return a.CompareTo(b) >= 0;
        }

        public static bool Greater<T>(T a, T b) where T : IComparable<T>
        {
            return a.CompareTo(b) > 0;
        }
    }

}
