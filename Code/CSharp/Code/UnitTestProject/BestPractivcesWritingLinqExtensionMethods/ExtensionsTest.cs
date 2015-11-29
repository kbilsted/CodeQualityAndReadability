using System;
using System.Collections.Generic;

using Code.BestPractivcesWritingLinqExtensionMethods;
using Code.ConvertingForToLinq;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Numerics;

namespace UnitTestProject.BestPractivcesWritingLinqExtensionMethods {

    [TestClass]
    public class ConvertForToLinqTest {

        [TestMethod]
        public void Use() {
            var strings = new[] { "a", "c", "x" };
            Assert.AreEqual("a, c, x", string.Join(", ", strings.NoOperation()));
        }

        [TestMethod]
        public void UseSyntax2() {
            var strings = new[] { "a", "c", "x" };
            var x = (from s in strings select s).NoOperation();
            Assert.AreEqual("a, c, x", string.Join(", ", x.NoOperation()));

            IQueryable<string> y = strings.Where(g => g == "r").AsQueryable();
        }


        public IEnumerable<BigInteger> GetAllPrimes() {
            var i = new BigInteger(1);
            while (true) {
                if (IsPrime(i))
                    yield return i;
                i = BigInteger.Add(i, BigInteger.One);
            }
        }

        static bool IsPrime(BigInteger bigInteger) {
            return true;
        }

        [TestMethod]
        public void hash() {
            decimal beløb1 = 11.5M;

            decimal beløb2 = beløb1 / 3M;

            beløb2 *= 3M;

            /*
            True
            40270000
            BFD9000F
            True
            False
            False
            True*/

            Console.WriteLine(beløb1 == beløb2); // (A)

            Console.WriteLine(beløb1.GetHashCode().ToString("X8")); // (B)

            Console.WriteLine(beløb2.GetHashCode().ToString("X8")); // (C)

            var list = new List<decimal> { beløb1, };

            Console.WriteLine(list.Contains(beløb2)); // (D)

            var mgd = new HashSet<decimal> { beløb1, };

            Console.WriteLine(mgd.Contains(beløb2)); // (E)

            Console.WriteLine(mgd.AsEnumerable().Contains(beløb2)); // (F)

            Console.WriteLine(mgd.Where(d => true).Contains(beløb2)); // (G)

        }
    }
}
 