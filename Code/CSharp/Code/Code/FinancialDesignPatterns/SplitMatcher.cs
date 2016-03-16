using System.Collections.Generic;
using System.Linq;

namespace Code.FinancialDesignPatterns {
    public class SplitMatcher {
        public List<Match<T, TC>> Match<T, TC>(
            IEnumerable<T> input,
            IEnumerable<IConsumer<T, TC>> consumers,
            ConsumptionAdder<TC> consumptionAdder) {
            var wrappers = input.Select(x => new Matchable<T, TC>(x, consumptionAdder)).ToList();
            return Match(wrappers, consumers);
        }

        List<Match<T, TC>> Match<T, TC>(
            List<Matchable<T, TC>> input,
            IEnumerable<IConsumer<T, TC>> consumers) {

            var processedList = input;

            foreach (var consumer in consumers) {
                if (!processedList.Any())
                    break;

                processedList = consumer.Match(input);
            }

            return null;
        }

        public MatchResult<TFact, TConsumption> Match2<TFact, TConsumption>(
            IEnumerable<TFact> input,
            IEnumerable<IConsumer<TFact, TConsumption>> consumers,
            ConsumptionAdder<TConsumption> consumptionAdder)
        {
            var scaffolding = input.Select(x => new Matchable<TFact, TConsumption>(x, consumptionAdder)).ToList();
            var result = new MatchResult<TFact,TConsumption>();
            var toProcess = scaffolding; 

            foreach (var consumer in consumers) {
                if (!toProcess.Any()) {
                    result.Consumers[ConsumerStatus.Active].Add(consumer); 
                    continue;
                }


                bool consumerCompleted = toProcess
                    .Select(element => consumer.Match(element))
                    .Any(status => status == ConsumerStatus.Complete);

                if (consumerCompleted)
                    result.Consumers[ConsumerStatus.Complete].Add(consumer);

                toProcess = toProcess.Where(x => !x.IsFullyConsumed).ToList();
            }

            // return all elements
            result.Matches = scaffolding;
            //result.ConsumerInfo = scaffolding. // TODO
            return result;
        }
    }


    public delegate TFact ConsumptionAdder<TFact>(TFact consumedSoFar, TFact newlyConsumed);

    public enum ConsumerStatus { Active, Complete } 

    public interface IConsumer<TFact, TConsumption>
    {
        List<Matchable<TFact, TConsumption>> Match(List<Matchable<TFact, TConsumption>> input);
        ConsumerStatus Match(Matchable<TFact, TConsumption> input);
    }

    public class Match<TFact, TC>
    {
        public IConsumer<TFact, TC> Consumer;
        public TC Consumption;

        public Match(IConsumer<TFact, TC> consumer, TC consumption)
        {
            Consumer = consumer;
            Consumption = consumption;
        }
    }

    public class ConsumerInfo<TFact, TConsumption> {
        IConsumer<TFact, TConsumption> Consumer;
        List<TFact> Fact = new List<TFact>(); 
    }

    public class MatchResult<TFact, TConsumption> {
        public readonly Dictionary<ConsumerStatus, List<IConsumer<TFact, TConsumption>>> Consumers =
            new Dictionary<ConsumerStatus, List<IConsumer<TFact, TConsumption>>>()
            {
                { ConsumerStatus.Complete, new List<IConsumer<TFact, TConsumption>>() },
                { ConsumerStatus.Active, new List<IConsumer<TFact, TConsumption>>() }
            };

        public List<Matchable<TFact, TConsumption>> Matches;
        public List<ConsumerInfo<TFact, TConsumption>> ConsumerInfo = new List<ConsumerInfo<TFact, TConsumption>>();
    }

    /// <summary>
    /// Augmented structure around each input element tracking the partial consumption of the element
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TC"></typeparam>
    public class Matchable<T, TC>
    {
        List<Match<T, TC>> consumers = new List<Match<T, TC>>();
        ConsumptionAdder<TC> adder;

        public readonly T Input;
        public IReadOnlyList<Match<T, TC>> Consumers => consumers.AsReadOnly();
        public TC AccumulatedConsumption;

        public Matchable(T input, ConsumptionAdder<TC> adder)
        {
            Input = input;
            this.adder = adder;
        }

        /// <summary>
        /// If fully consumed, the entity will not be fed to another consumer
        /// </summary>
        public void AddMatch(IConsumer<T, TC> consumer, TC consumtion, bool isFullyConsumed = false) {
            IsFullyConsumed = isFullyConsumed;
            consumers.Add(new Match<T, TC>(consumer, consumtion));
            AccumulatedConsumption = adder(AccumulatedConsumption, consumtion);
        }

        /// <summary>
        /// If fully consumed, the entity will not be fed to another consumer
        /// </summary>
        public bool IsFullyConsumed { get; set; }
    }
}
