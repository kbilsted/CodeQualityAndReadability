using System;
using System.Collections.Generic;
using System.Linq;

using Code.FinancialDesignPatterns;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject.FinancialDesignPatterns
{
    class Phonecall
    {
        public TimeSpan Duration;
    }


    [TestClass]
    public class UnitTest1
    {

        [TestMethod]
        public void NoSplitNeeded() {
            var calculator = new PartEntityPartRuleMatchPattern();
            var rules = new List<IRule<Phonecall, decimal>>() { new UnitPrSecond() };
            var result = calculator.Calculate(MakeCalls(1, 7), rules);
            Assert.AreEqual(0, result.UnconsumedEntities.Count);
            Assert.AreEqual(8M, result.Billed.SelectMany(x => x.Billed).Sum(x => x.BillAmount));
        }


        List<Phonecall> MakeCalls(params int[] calls) {
            return calls.Select(x => new Phonecall() { Duration = TimeSpan.FromSeconds(x) }).ToList();
        } 
    }

    class UnitPrSecond : IRule<Phonecall, decimal> {
        public CalcResult<Phonecall, decimal> Calculate(List<ConsumedInfo<Phonecall, decimal>> items) {
            var result = new CalcResult<Phonecall, decimal>();

            foreach (var itme in items) {
                var am = itme.Origin.Duration.Seconds - itme.Consumed;
                result.Billed.Add(new BillRecord<decimal>() { BillAmount = am, Consumed = am, Description = "a $ a second" });
            }

            return result;
        }
    }

    class Discount3For2 : IRule<Phonecall, decimal> {
        readonly IRule<Phonecall, decimal> priceCalculator;

        public Discount3For2(IRule<Phonecall, decimal> priceCalculator) {
            this.priceCalculator = priceCalculator;
        }

        public CalcResult<Phonecall, decimal> Calculate(List<ConsumedInfo<Phonecall, decimal>> items) {
            var result = new CalcResult<Phonecall, decimal>();
            var chuncks = items.Chunker(3);
            foreach (var chunck in chuncks) {
                var prices = chunck.Select(x => priceCalculator.Calculate(new List<ConsumedInfo<Phonecall, decimal>>() { x })).ToList();
                if (prices.Count == 3) {
                    var min = prices.Min();
                    bool hasGivenDiscount = false;
                    foreach (var price in prices) {
                        result.Billed.Add(new BillRecord<decimal>() {});
                    }
                }
            }

            return result;

        }
    }

    class TimeOrMoneyConsumption
    {
        public TimeSpan? Duration;
        public decimal? Cost;
    }

    public static class Extensions {
        public static IEnumerable<List<T>> Chunker<T>(this IEnumerable<T> source, int chunkSize) {

            List<T> bucket = null;
            foreach (var item in source)
            {
                if (bucket == null)
                    bucket = new List<T>();

                bucket.Add(item);

                if (bucket.Count == chunkSize) {
                    yield return bucket;
                    bucket = null;
                }
            }

            if (bucket != null)
                yield return bucket;
        } 
    }

}
