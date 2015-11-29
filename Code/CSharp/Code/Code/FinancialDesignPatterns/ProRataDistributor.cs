using System;
using System.Linq;

namespace Code.FinancialDesignPatterns
{
    public class ProRataDistributor
    {
        public decimal[] NaiveDistribute(decimal amount, int[] weights, int decimalPlaces) {
            var sumWeights = weights.Sum();
            return weights
                .Select(weight => ((amount / sumWeights) * weight))
                .Select(anAmount => Math.Round(anAmount, decimalPlaces))
                .ToArray();
        }


        //


        public decimal[] DistributeAllButLast(decimal amount, int[] weights, int decimalPlaces) {
            decimal dummy;
            return DistributeAllButLast(amount, weights, decimalPlaces, out dummy);
        }

            public decimal[] DistributeAllButLast(decimal amount, int[] weights, int decimalPlaces, out decimal deviation)
            {
                var sumWeights = weights.Sum();
                int indexLastPositive = FindLastIndexPossitiveWeight(weights);

                var result = new decimal[weights.Length];
                decimal total = 0;

                for (int i = 0; i < indexLastPositive; i++) {
                    decimal truncated = Truncate(weights[i] * amount / sumWeights, decimalPlaces);
                    result[i] = truncated;
                    total += truncated;
                }

                // last positive weight
                var residual = amount - total;
                var truncatedResidual = Truncate(residual, decimalPlaces);
                result[indexLastPositive] = truncatedResidual;

                deviation = residual - truncatedResidual;

                return result;
            }

            int FindLastIndexPossitiveWeight(int[] weights) {
                for (int i = weights.Length - 1; i >= 0; i--) {
                    if (weights[i] > 0) {
                        return i;
                    }
                }
                throw new ArgumentException("Must have at least one positive weight", nameof(weights));
            }

            decimal Truncate(decimal value, int decimalPlaces)
            {
                if (decimalPlaces < 0)
                    throw new ArgumentException("Must be possitive.", nameof(decimalPlaces));

                var factor = (decimal)Math.Pow(10, decimalPlaces);
                return Math.Truncate(value * factor) / factor;
            }
    }
}
