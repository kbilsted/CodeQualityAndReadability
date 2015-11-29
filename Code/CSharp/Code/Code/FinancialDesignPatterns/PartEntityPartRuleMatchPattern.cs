using System;
using System.Collections.Generic;
using System.Linq;

namespace Code.FinancialDesignPatterns {








    public class PartEntityPartRuleMatchPattern {
        public EngineResult<TEntry, TConsumption> Calculate<TEntry, TConsumption>(
            List<TEntry> items,
            List<IRule<TEntry, TConsumption>> rules) {

            var unconsumed = items.Select(x => new ConsumedInfo<TEntry, TConsumption>(x)).ToList();

            var result = new EngineResult<TEntry, TConsumption>();

            foreach (var rule in rules) {
                if (!unconsumed.Any())
                    break;

                var partResult = rule.Calculate(unconsumed);

                result.Billed.Add(new RuleBilling<TEntry, TConsumption>() { AppliedRule = rule, Billed = partResult.Billed });
                unconsumed = partResult.UnconsumedBillableEntities;
            }

            result.UnconsumedEntities = unconsumed;


            return result;
        }
    }

    public class ConsumedInfo<TEntry, TConsumption> {
        public TEntry Origin;

        public List<RuleBilling<TEntry, TConsumption>> Billed = new List<RuleBilling<TEntry, TConsumption>>();

        public TConsumption Consumed;

        public ConsumedInfo(TEntry origin) {
            Origin = origin;
        }
    }

    public class EngineResult<TEntry, C> {
        public readonly List<RuleBilling<TEntry, C>> Billed = new List<RuleBilling<TEntry, C>>();

        public List<ConsumedInfo<TEntry, C>> UnconsumedEntities;
    }

    public interface IRule<TEntry, C> {
        CalcResult<TEntry, C> Calculate(List<ConsumedInfo<TEntry, C>> items);
    }


    public class CalcResult<TEntry, C> {
        public List<ConsumedInfo<TEntry, C>> UnconsumedBillableEntities = new List<ConsumedInfo<TEntry, C>>();

        public List<BillRecord<C>> Billed = new List<BillRecord<C>>();
    }

    public class RuleBilling<T, C> {
        public IRule<T, C> AppliedRule;

        public List<BillRecord<C>> Billed;
    }

    public class BillRecord<C> {
        public C Consumed;

        public string Description;

        public decimal BillAmount;
    }

}
