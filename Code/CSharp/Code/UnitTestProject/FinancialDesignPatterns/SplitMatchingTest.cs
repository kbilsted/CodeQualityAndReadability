using System;
using System.Collections.Generic;
using System.Linq;

using Code.FinancialDesignPatterns;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject.FinancialDesignPatterns
{

    [TestClass]
    public class SeveralPaymentsForSeveralExpenses {

        class MonthlyExpense : IConsumer<decimal, decimal> {
            readonly decimal monthlyCost;
            decimal Covered;

            public MonthlyExpense(decimal monthlyCost) {
                if (monthlyCost < 0)
                    throw new ArgumentException("Negative cost");

                this.monthlyCost = monthlyCost;
            }

            public List<Matchable<decimal, decimal>> Match(List<Matchable<decimal, decimal>> payments) {
                decimal uncoveredExpense = monthlyCost;
                var needsProcessing = new List<Matchable<decimal,decimal>>();

                foreach (var payment in payments) {
                    // stop condition
                    if (uncoveredExpense == 0) {
                        needsProcessing.Add(payment);
                        continue;
                    }

                    var leftOfPayment = payment.Input - payment.AccumulatedConsumption;
                    var toPay = Math.Min(leftOfPayment, uncoveredExpense);
                    uncoveredExpense -= toPay;
                    payment.AddMatch(this, toPay);
                    if(toPay < leftOfPayment)
                        needsProcessing.Add(payment);
                }

                return needsProcessing;
            }

            public ConsumerStatus Match(Matchable<decimal, decimal> payment) {
                //var leftOfPayment = payment.Input - payment.AccumulatedConsumption;
                //var toPay = Math.Min(leftOfPayment, monthlyCost - Covered);
                //Covered += toPay;

                //var isPaymentComplete = toPay == leftOfPayment;
                //payment.AddMatch(this, toPay, isPaymentComplete);

                //if (Covered == monthlyCost)
                //    return ConsumerStatus.Complete;
                return ConsumerStatus.Active;
            }
        }

        [TestMethod]
        public void Match() {
            var sut = new SplitMatcher();
            var m1 = new MonthlyExpense(200);
            var m2 = new MonthlyExpense(200);

            var res = sut.Match(new[] { 300M, 300M }, new[] { m1, m2 }, (x,y)=> x+y);
        }
    }





    [TestClass]
    public class SeveralPaymentsForSeveralExpenses2
    {

        class MonthlyExpense : IConsumer<decimal, decimal>
        {
            readonly decimal monthlyCost;
            decimal CoveredByConsumer;

            public MonthlyExpense(decimal monthlyCost)
            {
                if (monthlyCost < 0)
                    throw new ArgumentException("Negative cost");

                this.monthlyCost = monthlyCost;
            }

            public List<Matchable<decimal, decimal>> Match(List<Matchable<decimal, decimal>> payments)
            {
                //decimal uncoveredExpense = monthlyCost;
                //var needsProcessing = new List<Matchable<decimal, decimal>>();

                //foreach (var payment in payments)
                //{
                //    // stop condition
                //    if (uncoveredExpense == 0)
                //    {
                //        needsProcessing.Add(payment);
                //        continue;
                //    }

                //    var leftOfPayment = payment.Input - payment.AccumulatedConsumption;
                //    var toPay = Math.Min(leftOfPayment, uncoveredExpense);
                //    uncoveredExpense -= toPay;
                //    payment.AddMatch(this, toPay);
                //    if (toPay < leftOfPayment)
                //        needsProcessing.Add(payment);
                //}

                //return needsProcessing;
                return null;
            }

            public ConsumerStatus Match(Matchable<decimal, decimal> payment)
            {
                var leftOfPayment = payment.Input - payment.AccumulatedConsumption;
                var toPay = Math.Min(leftOfPayment, monthlyCost - CoveredByConsumer);
                CoveredByConsumer += toPay;

                var isPaymentComplete = toPay == leftOfPayment;
                payment.AddMatch(this, toPay, isPaymentComplete);
                
                return CoveredByConsumer == monthlyCost ? ConsumerStatus.Complete : ConsumerStatus.Active;
            }
        }

        [TestMethod]
        public void Match33() {
            var input = new[] { 300M, 300M };
            var y = input.Where(x => x > 3);
            f(y);
            var cc = y.Where(x => x > 3);
            f(cc);
            IList<string> s = new List<string>();
            s.Add("dd");
            s.
        }

        void f<T>(IEnumerable<T> t) {
            foreach (var df in t) {
                Console.WriteLine(df);
            }
            g(t);
        }

        void g<T>(IEnumerable<T> t)
        {
            foreach (var df in t)
            {
                Console.WriteLine(df);
            }
        }




        [TestMethod]
        public void Match()
        {
            var sut = new SplitMatcher();

            var input = new[] { 300M, 300M };

            var m1 = new MonthlyExpense(200);
            var m2 = new MonthlyExpense(200);
            var consumers = new[] { m1, m2 };

            MatchResult<decimal, decimal> res = sut.Match2(input, consumers, (x, y) => x + y);

            // both consumers are complete
            Assert.AreEqual(2, res.Consumers[ConsumerStatus.Complete].Count);
            Assert.AreEqual(0, res.Consumers[ConsumerStatus.Active].Count);

            // The first input of $300 paid both monthly expense 1 and 2
            var firstMatchConsumers = res.Matches[0].Consumers;
            CollectionAssert.AreEqual(consumers, firstMatchConsumers.Select(x => x.Consumer).ToArray());

            // The first consumer is "m1" and has consumed $200
            Assert.AreEqual(m1, firstMatchConsumers.First().Consumer);
            Assert.AreEqual(200, firstMatchConsumers.First().Consumption);

            // The second input only paid monthly expense 2
            Assert.AreEqual(m2, res.Matches[1].Consumers.Single().Consumer);
        }
    }



}
