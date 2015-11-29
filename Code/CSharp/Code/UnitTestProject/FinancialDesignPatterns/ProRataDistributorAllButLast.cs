using System.Linq;

using Code.FinancialDesignPatterns;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject.FinancialDesignPatterns {
    [TestClass]
    public class ProRataDistributorAllButLast {
        [TestMethod]
        public void Error_should_sum_100() {
            var res = new ProRataDistributor().DistributeAllButLast(100, new[] { 1, 1, 1 }, 2);
            CollectionAssert.AreEqual(new[] { 33.33M, 33.33M, 33.34M }, res);
            Assert.AreEqual(100, res.Sum());
        }

        [TestMethod]
        public void Correct_sum_100() {
            var res = new ProRataDistributor().DistributeAllButLast(100, new[] { 1, 1 }, 2);
            CollectionAssert.AreEqual(new[] { 50M, 50M }, res);
            Assert.AreEqual(100, res.Sum());
        }


        [TestMethod]
        public void PathelogicalSmallAmounts() {
            var weights = new[] { 1, 1, 1 };

            var res = new ProRataDistributor().DistributeAllButLast(0.01M, weights, 2);
            CollectionAssert.AreEqual(new[] { 0, 0, 0.01M }, res);

            res = new ProRataDistributor().DistributeAllButLast(0.02M, weights, 2);
            CollectionAssert.AreEqual(new[] { 0, 0, 0.02M }, res);

            res = new ProRataDistributor().DistributeAllButLast(0.03M, weights, 2);
            CollectionAssert.AreEqual(new[] { 0.01M, 0.01M, 0.01M }, res);

            res = new ProRataDistributor().DistributeAllButLast(0.04M, weights, 2);
            CollectionAssert.AreEqual(new[] { 0.01M, 0.01M, 0.02M }, res);

            res = new ProRataDistributor().DistributeAllButLast(0.05M, weights, 2);
            CollectionAssert.AreEqual(new[] { 0.01M, 0.01M, 0.03M }, res);

            res = new ProRataDistributor().DistributeAllButLast(0.06M, weights, 2);
            CollectionAssert.AreEqual(new[] { 0.02M, 0.02M, 0.02M }, res);

            weights = new[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
            res = new ProRataDistributor().DistributeAllButLast(0.15M, weights, 2);
            CollectionAssert.AreEqual(new[] { 0.01M, 0.01M, 0.01M, 0.01M, 0.01M, 0.01M, 0.01M, 0.01M, 0.01M, 0.06M }, res);
        }

        [TestMethod]
        public void PathelogicalResidualHandling_alwaysAssignedToLastNonzeroWeight() {
            {
                var weights = new[] { 2, 1, 1 };
                var res = new ProRataDistributor().DistributeAllButLast(0.00000005M, weights, 8);
                CollectionAssert.AreEqual(new[] { 0.00000002M, 0.00000001M, 0.00000002M }, res);

                weights = new[] { 2, 1, 2 };
                res = new ProRataDistributor().DistributeAllButLast(0.00000006M, weights, 8);
                CollectionAssert.AreEqual(new[] { 0.00000002M, 0.00000001M, 0.00000003M }, res);
            }
        }

        [TestMethod]
        public void RespectZeroWeigh() {
            var weights = new[] { 1, 1, 0 };

            var res = new ProRataDistributor().DistributeAllButLast(204.10749999M, weights, 8);
            CollectionAssert.AreEqual(new[] { 102.05374999M, 102.05375M, 0 }, res);

            res = new ProRataDistributor().DistributeAllButLast(204.10750001M, weights, 8);
            CollectionAssert.AreEqual(new[] { 102.05375M, 102.05375001M, 0 }, res);
        }
    }
}