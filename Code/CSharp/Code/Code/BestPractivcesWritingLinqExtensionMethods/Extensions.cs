using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Code.BestPractivcesWritingLinqExtensionMethods {

    static public class Extensions {
        public static IEnumerable<T> NoOperation<T>(this IEnumerable<T> elements) {
            using (var enumerator = elements.GetEnumerator()) {
                while (enumerator.MoveNext()) {
                    T element = enumerator.Current;
                    yield return element;
                }
            }
        }
    }
}