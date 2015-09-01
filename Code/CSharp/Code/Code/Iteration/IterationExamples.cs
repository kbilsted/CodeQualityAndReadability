using System;
using System.Linq;

namespace Code.Iteration
{
    public class IterationExamples
    {
        public string Gotos(int[] numbers)
        {
            string res = "";
            int i = 0;

            again:
            res += numbers[i];
            i++;
            if (i == numbers.Length)
                goto stop;
            goto  again;

            stop:
            return res;
        }

        //

        public string UnboundWhile(int[] numbers)
        {
            string res = "";
            int i = 0;
            while (true)
            {
                res += numbers[i];
                i++;
                if (i == numbers.Length)
                   break;
            }

            return res;
        }

        //

        public string BoundedWhile(int[] numbers)
        {
            string res = "";
            int i = -1;
            while (i < numbers.Length - 1)
            {
                i++;

                res += numbers[i];
            }

            return res;
        }

        //

        public string For(int[] numbers)
        {
            string res = "";
            for (int i = 0; i < numbers.Length; i++)
            {
                res += numbers[i];
            }

            return res;
        }

        //

        public string ForWithExtractMethod(int[] numbers)
        {
            string res = "";
            for (int i = 0; i < numbers.Length; i++)
            {
                res = Concat(numbers[i], res);
            }

            return res;
        }

        string Concat(int number, string res)
        {
            return res + number;
        }

        //

        public string Enumerator(int[] numbers)
        {
            string res = "";
            var enumerator = numbers.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var elem = enumerator.Current;
                res += elem;
            }

            return res;
        }


        //

        public string Foreach(int[] numbers)
        {
            string res = "";
            foreach (var number in numbers)
                res += number;

            return res;
        }

        //

        public string Linq(int[] numbers)
        {
            return numbers.Aggregate("", (res, number) => res + number);
        }
    }

    public class ComplexIteration
    {
        public string IterationSkippingSome(int[] numbers)
        {
            string res = "";

            for (int i = 0; i < numbers.Length - 1; i++)
            {
                var current = numbers[i];
                if (current % 5 == 0)
                    continue;

                var next = numbers[i + 1];
                if (next % 2 == 0)
                {
                    i++;
                }
                else
                {
                    res += current + next;
                }
            }
            return res;
        }

        //

        public string IterationSkippingSomeExtracted(int[] numbers)
        {
            string res = "";

            for (uint i = 0; i < numbers.Length - 1; i++)
            {
                uint delta = RestrictedPairwiseSum(numbers[i], numbers[i+1], ref res);
                i += delta;
            }

            return res;
        }

        uint RestrictedPairwiseSum(int current, int next, ref string res)
        {
            if (current % 5 == 0)
                return 0;

            if (next % 2 == 0)
                return 1;

            res += current + next;
            return 0;
        }

        //

        enum SkippingBehaviour
        {
            NoSkip, SkipNext
        }

        public string IterationSkippingSomeExtractedAndSkipLogic(int[] numbers)
        {
            string res = "";

            for (int i = 0; i < numbers.Length - 1; i++)
            {
                var skipping = RestrictedPairwiseSum2(numbers[i], numbers[i + 1], ref res);
                i += GetDelta(skipping);
            }

            return res;
        }

        int GetDelta(SkippingBehaviour skipping)
        {
            switch (skipping)
            {
                case SkippingBehaviour.NoSkip:
                    return 0;
                case SkippingBehaviour.SkipNext:
                    return 1;
            }
            return 0;
        }

        SkippingBehaviour RestrictedPairwiseSum2(int current, int next, ref string res)
        {
            if (current % 5 == 0)
                return SkippingBehaviour.NoSkip;

            if (next % 2 == 0)
                return SkippingBehaviour.SkipNext;

            res += current + next;
            return SkippingBehaviour.NoSkip;
        }
    }
}
