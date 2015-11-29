using System;
using System.Linq;

using Code.FinancialDesignPatterns;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject.FinancialDesignPatterns
{
    [TestClass]
    public class ProRataDistributorNaiveTest
    {

        [TestMethod]
        public void Print_Error_division_failure()
        {
            Console.WriteLine(100 / 3M);
        }

        [TestMethod]
        public void Error_should_sum_100()
        {
            var res = new ProRataDistributor().NaiveDistribute(100, new[] { 1, 1, 1 }, 2);
            CollectionAssert.AreEqual(new[] { 33.33M, 33.33M, 33.33M }, res);
            Assert.AreEqual(99.99M, res.Sum());
        }

        [TestMethod]
        public void Correct_sum_100()
        {
            var res = new ProRataDistributor().NaiveDistribute(100, new[] { 1, 1 }, 2);
            CollectionAssert.AreEqual(new[] { 50, 50 }, res);
            Assert.AreEqual(100, res.Sum());
        }
    }
}
