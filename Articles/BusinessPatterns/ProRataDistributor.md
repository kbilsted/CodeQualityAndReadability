draft
# Building a Pro-Rata Distributor
*Author: Kasper B. Graversen*
<ArticleHeaderUrls/>
<Categories Tags="Business_Pattern, Numerical_Calculations">
</Categories>


*Applications handling money always call for extra caution. A pro-rata distributor along with a notion of one or several accounts are the right components to ensure proper handling of money. A Pro-rata distributor is a small reusable component that split an amount among one or more recipients. The distribution may be even or weighted. The need to distribute amounts (or other numerical figures) arise often when an application handles money. The pro-rata distributor ensures that no money is lost or appear by magic when splitting amounts. Additionally, it handles corner cases with respect to change in decimal digits between input and output. 

This article goes in-depth with how to design and implement a production ready pro-rata distributor and highlight some of the surprising design pitfalls that exist.*


Please show your support by sharing and voting:

<SocialShareButtons>
</SocialShareButtons>

<img src="img/httpspixabay.comdavand-anlæg-grøn-bøde-lag-abstrakt-821293.jpg" alt="from https://pixabay.com/da/vand-anl%C3%A6g-gr%C3%B8n-b%C3%B8de-lag-abstrakt-821293/" >


Table of Content

   * [1. Introduction](#introduction)
   * [2. Requirements for a pro-rata distributor](#requirements-for-a-pro-rata-distributor)
   * [3. A naïve (but wrong) implementation](#a-naïve-but-wrong-implementation)
   * [4. Designing the weights specification API](#designing-the-weights-specification-api)
     * [4.1 Percentage Weights specification](#percentage-weights-specification)
     * [4.2 Relative weights specification](#relative-weights-specification)
   * [5. Residual - the amount left after first-pass distribution](#residual---the-amount-left-after-first-pass-distribution)
   * [6. Deviation - the non-distributable amount](#deviation---the-non-distributable-amount)
   * [7. A correct and fast implementation](#a-correct-and-fast-implementation)
   * [8. Pathological cases showing uneven distributions](#pathological-cases-showing-uneven-distributions)
     * [8.1 Smearing out the residual.](#smearing-out-the-residual)
   * [9. Unit test driving the design astray](#unit-test-driving-the-design-astray)
   * [10. Negative weights](#negative-weights)
   * [Summary](#summary)
    

## 1. Introduction
A central piece of logic in many business systems, is the handling of amounts to be shared by a number of parties. The fundamental problem of splitting money is first that numerical calculations in computers use finite representations of numbers, secondly, that monetary units have a very few decimal places whereas computers operate with more decimal places. Additionally, there are edge cases, where special attention to how we distribute needs be taken, and finally, we must cater for situations where not all parties are to be receiving the same share of the sum. 

The running example of this article is the distribution of life insurance money to heirs. But the problem of distribution is a far more common pattern. For example, given a payment and an investment profile, split the payment accordingly, say by "high risk stocks 10%", "bonds 50%", "green energy 40%". 

As will soon become painfully evident, since computers represent decimal values using a finite bit-representation, division is not sufficiently sophisticated to provide the correct answers. 

Let's warm up with a simple distribution of $100 to 3 recipients

```
Console.WriteLine(100/3M);

>> 33.333333333333333333333333333
```

That's a finite representation of an infinite number, not a real 1/3 of 100! Needles to say, this will get us into trouble when we operate with money (and hence need a very limited number of decimal places for the result). For this example using division we get $33.33 to each party (is not possible to pay someone $33.333333333333333333333333333!), which when added together results in $99.99. *Wow! Will you look at that, we just lost $0.01*. That may accrue to billions of dollars lost - given enough amounts passing through our system. Splitting $100 three ways should yield ($33.33, $33.33, $33.34). While it is slightly inconvenient that one person is getting more than the rest, our distribution sums to the amount we are distributing - that's what accountants appreciate.

Sometimes you also see code like this 

```
var total = moneyBags.Sum();
distribution[0] = total / 3;
distribution[1] = total / 3;
distribution[2] = total - distribution[0] - distribution[1];
```

What kind of code is this? It is code *trying* to address the problems with distribution. Unfortunately, the approach has flaws, is not very general, it is not a reusable component and it doesn't address issues of rounding. If you are writing code like this, you are off to a good start, but the approach we are showing here is superior.



## 2. Requirements for a pro-rata distributor

When constructing an abstraction for splitting amounts, we might as well contemplate on the different use scenarios we may run into and pack it all into the implementation. This is the advantage of creating abstractions over simple copy-paste of code - We get to think more generally about the problem we are solving.

When we split between multiple parties, a need may arise to give more to one party than others. For example, Laura, the just-turned widow of Bob, who passed away from a terrible accident at the butchers during a mega-sale on sausage, will inherit 50% of the life insurance money. Laura's children gets to share the remainder three-ways. Formally speaking, our distribution may be *weighted*. Weights may be positive or zero. 

The *minimum unit of computation* is typically very low compared to ordinary numerical computation. The minimum unit of computation is dependent on the currency in play. For most currencies it is two decimal digits. For the USD currency, amounts such as $0.001 cannot be distributed. It is below the threshold of two decimal places. In such a case, no mater the size of the weight we end up with $0. Consequently, we need a notion of a *deviation* to ensure we don't loose money during distribution. The following equation must hold true: `money == sum of distribution + deviation`.  


To summarize the requirements for a pro-rata distributor

  1. The sum of the split amounts must equal the original amount.
  2. The distributor must support *weighted distribution*.
  3. The *deviation*, the amount impossible to assign to any recipient, must be returned.
  4. The number of *decimal digits* must be configurable and change from input to output.




## 3. A naïve (but wrong) implementation

Before going through all the requirements we just listed, let us get a feel for what the shape of a pro-rata distributor looks like. We can fairly easily create a naïve strategy to doing the distribution. Although it doesn't do its job correctly, we can learn from it.

```
public decimal[] NaiveDistribute(decimal amount, int[] weights, int decimalPlaces) 
{
    var sumWeights = weights.Sum();
    return weights
        .Select(weight => ((amount / sumWeights) * weight))
        .Select(anAmount => Math.Round(anAmount, decimalPlaces))
        .ToArray();
}
```

So the code takes an amount to be distributed along with some weights and a specification as to how many decimal places the resulting amounts should have. The weights are then summed, and for each weight, we divide the amount with the sum of weights to know how much money a weight unit represents. After finding each weighted amount, we round the amounts to the specified number of decimal places.

Dead simple. Unfortunately, also wrong. For the simple cases it seems to be working

```
var result = new ProRataDistributor().NaiveDistribute(100, new[] { 1, 1 }, 2);
Assert.AreEqual(new[] { 50, 50 }, result);
Assert.AreEqual(100, result.Sum());
```

but if we try a more devious situation, we loose money (1 cent).

```
var result = new ProRataDistributor().NaiveDistribute(100, new[] { 1, 1, 1 }, 2);
Assert.AreEqual(new[] { 33.33M, 33.33M, 33.33M }, result);
Assert.AreEqual(99.99M, result.Sum());
```
 
What are the pitfalls here? There are a few in fact.

* We use `Math.Round()` which may make money appear or disappear. 
* We are not tracking non-distributed amounts and thus when the weights are "unfortunate" in relation to the amount, we cannot rely on the equation `amount/sumWeights * weight`
* We have no notion of *deviation* which may arise when there are fewer decimal digits in the output than in the input.

Before we dig into these numerical problems, let's first look at designing the API.




## 4. Designing the weights specification API

There are two distinct ways to pass as argument, the weights for the distribution. Using percentages, and using relative weights. We shall touch upon both soon enough. But first, lets recap the misfortunate Bob, who left behind $1.000.000 to his heirs. His wife is to receive 50%, and each of the three kids share the remaining 50%. 



### 4.1 Percentage Weights specification

The gut reaction for most people is to use percentages as the means for weight specification. The argument typically are as follows.

The concept "percentage" is fairly common and is easy to apply. If you need half the money, you simply specify the weight 0.5, and equally, 10% is the weight 0.1. Secondly, percentages are susceptible to validation. A fair requirement to the API, is that the weights must sum to 1. If not, a misconfiguration has occurred, perhaps a recipient has been forgotten.

Paradoxically, in my experience, however, these advantages are actually disadvantages. Take the concept of percentage. Yes 50% = 0.50. But what are the percentages needed for the distribution of Bob's money? Wife = 50%, but the children? They each take 1/6th which *roughly* is 17% - or 0.17. But how did I get the 0.17? I had to resort to my calculator. Calculating the percentage was not hard, when using the API I first has to spend lines of code calculating the percentages. And an administrative person using the application's GUI will have to use a calculator in order to fill out the fields. Not particularly user friendly. 

Recall that I said 1/16th was roughly 0.17? Therein lies the second problem, because our fancy argument validation fails, since 0.5 + 0.17 + 0.17 + 0.17 = **1.01**! Not good. In fact, it turns out, there are a whole range of frequently used weights that cannot adequately be expressed using percentages. For example 1/3, 2/3, 1/6, and 1/7. 

A client once insisted on using percentage weights for portfolio management where the configuration Gui had to validate the sum as just described. Needles to say, they soon ran into problems like expressing 1/6th. The solution? Use the weights as input for a pre-burner pro-rata distributor that would distribute the value `1` using the input weights of the Gui. The output would then be the weights for the configuration for the real pro-rata distributor. This way the administrative clerks could calculate approximations to the weights, and the pro-rata distributor would then behind the scene auto correct the input values to something that summed to 1. And now our input validation did not complain any longer.

Just think about that for a moment.. Allow me to presume that time is ripe for discussing the alternative: The relative weights.



### 4.2 Relative weights specification

Relative weights as inputs are, once you understand them, really easy and nice to work with. A relative weight is simply a number whose value in itself has no meaning, the meaning is found by the relation to the sum of all weights. 

If two recipients need to split an amount fifty-fifty, you can use the weights `{1, 1}`, but `{2, 2}` or `{230346, 230346}` are equally good. The reason? The weights represent half the accumulated sum of the weights. Three people needing a 1/3 split? Easy! `{1, 1, 1}`. Need a 1/3,  2/3 split? Use the weights `{1, 2}`. Notice that it is *straight forward* to express any of the problematic equations I raved on about above.  

Now that we have an understanding for how relative weights works, lets try to redo the splitting of Bobs inheritance. Each of the children needs a third each. For that we use the weight `1`. Mom Laura needs the same as the children together = 1+1+1, thus the weights are: `{1, 1, 1, 3}`. No calculator needed. No extra behind the scene pre-burning and tweaking the input.

There is a downside, unfortunately. The sum of weights do not sum to anything in particular, so there really isn't anything to validate on.




## 5. Residual - the amount left after first-pass distribution

Take the example of splitting $100 three-ways. That is 3 x $33.33 = $99.99. Hence we have $0.01 left. This we call the residual. One of the main problems with the `NaiveDistribute()` is that is ignores the fact that weights may not precisely map to an amount. Who is to be receiving the residual depends. 

Mathematicians will say it is the amount stemming from the largest weight, that way, the relative deviation is reduced to its smallest. In our case the weights are of equal size hence we can place it on either of the three $33.33. 

When we deal with event based systems that may re-calculate their internal state by replaying events, we prefer to assign the residual to any of the amounts, e.g. the first, so long as we always place the residual on the same amount each time. This is because in such systems we emphasize that recalculation must to the extend possible yield the same result each time. Of course you are free to combine the two principles.

The *residual* is not to be confused with the *deviation* which we'll address next.




## 6. Deviation - the non-distributable amount

Here is a bit of a shocker. You can fail at properly distributing *one* amount to *one* recipient! How can this be? Why can code like `laura = amount;` ever fail?

Internally, financial systems operate with far more decimal digits than any currency. This is a hard requirement as otherwise many many rounding problems arise. Many systems operate with 8 decimal digits internally, while most currencies only has two decimal digits. 

Hence, it is not unlikely, that amounts that needs be split are odd, say $100.0047351. But if the currency only has two decimal digits, how on earth did we ever accrue the 0.0047351? This can be accrued due to interest, or fees. Or perhaps it is the accumulated cost of units that are cheaper that the lowest amount possible in the currency, think of in-app-purchases or phone calls rated at $0.001 a minute. 

Hence when we distribute one amount to one recipient, there may be a *deviation* that cannot be carried over to the currency. Let's be bold and up the stakes to distributing an amount to two recipients. Not that it really matters much, it just feels more realistic. 

Let's try to split $100.0044351 in two equal parts for two bank transfers? For starters we divide the amount into two even portions. Respecting the USD currency only has two decimal digits, We get two $50 payments and a *deviation* of $0.0044351. What to do with that? We can't really transfer that amount out of the system to the outside world. But we can't just ignore it either. If we do, we are moving money into the void which will make alarms bells ring in any accountant. A system that deals with money *should* should follow the guidelines of accounting. For example use a double book keeping system. This way, money cannot appear or disappear without registering it on explicit accounts (such as complementary, loss, etc). In such a system I'd probably have an account solely for keeping unplayable deviations. On the positive side, that account may grow pretty fat over time, making it a nice source of money for high-risk investment.

We implement the deviation as a decimal which is the difference between the original amount and the sum of the distributed amounts.




## 7. A correct and fast implementation

For the implementation we first need some scaffolding infrastructure. We need truncate semantics over rounding semantics to ensure we do not  arbitrarily round up or down.

```
decimal Truncate(decimal value, int decimalPlaces)
{
    var factor = (decimal) Math.Pow(10, decimalPlaces);
    return Math.Truncate(value * factor) / factor;
}
```

We assign the residual to the last positive weight, hence we need to know where that is

```
int FindLastIndexPossitiveWeight(int[] weights) 
{
    for (int i = weights.Length - 1; i >= 0; i--) 
    {
        if (weights[i] > 0) 
            return i;
    }
    throw new ArgumentException("Must have at least one positive weight", nameof(weights));
}
```

Now for the implementation. As we saw in the naïve implementation, we split the amount into the sum of the weights. Then assign the according to weight. Finally, we assign the residual and the deviation.


```
public decimal[] Distribute(decimal amount, int[] weights, int decimalPlaces, out decimal deviation)
{
    var sumWeights = weights.Sum();
    var indexLastPositive = FindLastIndexPossitiveWeight(weights);

    var result = new decimal[weights.Length];
    decimal total = 0;
	
    for (int i = 0; i < indexLastPositive; i++) 
    {
	    decimal distributedAmount = weights[i] * amount / sumWeights;
        decimal truncated = Truncate(distributedAmount, decimalPlaces);
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
```

To those unfamiliar with C#, the `out` parameter `deviation` effectively is an additional return value returned when calling `Distribute()`.


Testing the code with the 3-split of $100 works!

```
var res = new ProRataDistributor().Distribute(100, new[] { 1, 1, 1 }, 2);
Assert.AreEqual(new[] { 33.33M, 33.33M, 33.34M }, res);
Assert.AreEqual(100, res.Sum());
```



## 8. Pathological cases showing uneven distributions 

A pro-rata distributor is meant to distribute amounts *evenly* amongst its recipients. Given two recipients have the same weight, it'd be safe to assume that they are receiving the same amount. This, however, is not always possible. Enter the world of pathological edge cases.

Given equal weights 

```
var weights = new[] { 1, 1, 1 };
```

We can exercise the code with very small amounts

```
var res = Distribute(0.01, weights, 2);
Assert.AreEqual(new[] { 0, 0, 0.01 }, res);
```

Not really a surprise - some one has to receive the residual.


```
res = Distribute(0.02M, weights, 2);
Assert.AreEqual(new[] { 0, 0, 0.02 }, res);
```
Oh dear, wouldn't we have expected a split something like: `{0, 0.1, 0.1}` ?


```
res = Distribute(0.03M, weights, 2);
Assert.AreEqual(new[] { 0.01, 0.01, 0.01 }, res);
```

Finally, we are distributing as expected. Let's continue.


```
res = Distribute(0.04M, weights, 2);
Assert.AreEqual(new[] { 0.01, 0.01, 0.02 }, res);

res = Distribute(0.05M, weights, 2);
Assert.AreEqual(new[] { 0.01, 0.01, 0.03 }, res);
```

We observe an uneven distribution of the amount for the cases 0.01 and 0.02. When we get to 0.03 an even distribution occur, and at 0.04, 0.05 the unevenness continues. Arguably for the cases 0.01 and 0.04 we have no chance of doing a better distribution, but a client with attention to detail may require us to do better in the cases of 0.02 and 0.05. To some business domains, making the "correct" distribution is really important. But for the most cases such pathological cases are little more than amusing. Doing the distribution more correctly is associated with higher CPU cycle cost during distributions.




### 8.1 Smearing out the residual.

We can get wilder inaccuracies if we scale up the number of recipients. If you are in a domain where you have to distribute small amounts amongst many recipients. Let's take an example showing larger inaccuracies. 


```
weights = new[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
res = Distribute(0.15, weights, 2);
Assert.AreEqual(new[] { 0.01, 0.01, 0.01, 0.01, 0.01, 0.01, 0.01, 0.01, 0.01, 0.06 }, res);
```

What is going on here? It is the `residual` that we assign to the last positive weight. By introducing some complexity, we can "smear" the residual across the recipients, creating more evenly distributions. 

To solve this, we may think we need to smear the residual in accordance to weight size, smearing more to the larger weights. This is not correct. Instead, we need to take outset in each split amount, figuring out its deviation.

I'll only cover the algorithm in a descriptive form. The implementation is left to the reader as an exercise. The algorithm works as follows.

* From the residual, calculate the minimal computational unit. 
* For each distributed amount calculate the deviation between assigned amount and what should have been assigned given infinite number of decimals.
* Order the distributed amounts by their weight specification (descending) 
  * Ignoring zero-weight amounts (0 weights should never receive anything). 
  * Ignoring zero-deviance amounts (we do not want to assign more than specified by weight).
* Continuously add a minimal computational unit to an across distributed amounts one at a time until the residual is below this minimum threshold. 
* Any leftovers is then assigned to the `deviation`. 

Needles to say that although this provides a more academically correct result, it is computationally far more expensive. 




## 9. Unit test driving the design astray  

After having used pro-rata distributors as descrived above, I've come to the conclusion the Api as we have designed it is an abomination.
But why, you may ask. We have used unit tests to verify that the interface to the pro-rata distributor is sound. The tests were easy to read and write. All is good.

But sometimes, tests may be deceiving and drive our design in the wrong direction. This happens when you focus too much on making the tests succinct. Then beauty of the test become the focus, rather than stepping back and simulating how the Api is to be used in code on a grander scale.

The problem at hand is the use of arrays for specifying the weights.  By requiring a array as input, building the input and processing the output becomes clumsy. First an array has to be build with the recipients, then call the pro-rata distributor and finally, a loop for iterating the answers correlating them with the just-constructed array. 

If setting up the input array is split across multiple methods one, the reader of the code must revisit all these places in order to understand the ordering of the output. If only a few recipients exists, say two or three, I've seen programmers create the array of weights constructed in an in-line fashion `weights = new []{  Bank.Share, Wife.Share, ...}`. The result-array is then accessed by means of absolute positions, ie. one has to know that `result[0]` is the bank, `result[1]` is the wife and so on. Encoding knowledge by positioning makes you reluctant to splitting up your code in smaller units since the lack of readability of positions drives you to want the input and output arrays as close to each other as possible. Further processing the result array is often clumsy. Say you use a loop to iterate over the result, you need to branch for each kind of data that is returned (e.g. bank, wife, etc.) and you need to know how many of each you need to deal with.

A better approach is to specify the weights as pairs of `(key, weight)`, likewise the distributed amounts are returned as `(key, amount)`. It makes unit testing a bit more cumbersome, but business code using the distributor becomes more malleable. But rather than using list of tuples, we instead take as argument, and return a *sorted dictionary*. Dictionaries naturally have a key and a value part. Sorting the dictionaries *ensure* that applications using event sourcing works correctly on calculation, and later, recalculation. If we do not sort the input and the answers, we may get a different ordering when first calculating from memory, and later, when we fetch data from the data base (since data may be retrieved in a different order, e.g. due to an SQL `ORDER BY` clause (or lack thereof). If we do not sort, cases where the residual is assigned to a subset of the recipients, we may get a different recipients on calculation on a re-calculation - requiring the system to quite needlessly store both old and new versions of the result. In turn this may instill discomfort in trusting the underlying event system. I know many hours wasted witch hunting the cause of new recalculation results. Was it a bug or what? 

Thus our Api becomes

```
public SortedDictionary<TKey, decimal> Distribute<TKey>(
    decimal amount, 
    SortedDictionary<TKey, int> weights, 
    int decimalPlaces, 
    out decimal deviation)
```




## 10. Negative weights

So far we have only discussed positive weights. In practice, there may be situations where weights for a distribution is both positive and negative. For example, a number of parties are to split an insurance sum, but the recipients have already received estimated pre-payments. Some estimates may be higher than actual, and thus arises the need to negative weights, to move money from one recipient to another.

Adding negative weights to the distributor is certainly doable, but it is with the introduction of further complexity. For example, the calculation of the sum of weights now no longer is valid. It has to calculate the sum of all the absolute values of all the weights. And likewise, for the calculation of the *residual*, and *deviations* now need to take the sign of the weight into account.

Like the spreading of the residual describe in section 8.1, we leave this up to the reader.





## Summary
We have presented a generally reusable framework to do split matching of facts and consumers. The code is freely available on www.github...




Please show your support by sharing and voting:
<SocialShareButtons>
</SocialShareButtons>



<br><br>
<CommentText>
</CommentText>

<br><br>
