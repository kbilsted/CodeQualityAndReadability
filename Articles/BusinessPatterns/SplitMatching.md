draft
# Split matching
*Author: Kasper B. Graversen*
<ArticleHeaderUrls/>
<Categories Tags="Design, Business_Pattern, Design_Pattern ">
</Categories>


*We introduce a design pattern/general algorithm for performing partial matching between "events" and "rules" by means of splitting potentially both if necessary. It is an extended form of the "chain of responsibility" design pattern in that a "rule" may choose to only partly consume its input. A generally reusable solution is provided that can freely be used.*


Please show your support by sharing and voting:

<SocialShareButtons>
</SocialShareButtons>

<img src="img/httpspixabay.comdavand-anlæg-grøn-bøde-lag-abstrakt-821293.jpg" alt="from https://pixabay.com/da/vand-anl%C3%A6g-gr%C3%B8n-b%C3%B8de-lag-abstrakt-821293/" >


Table of Content

   * [1. Introduction](#introduction)
   * [2. Real life examples](#real-life-examples)
     * [2.1 Example: Payments and bills - limited rules](#example-payments-and-bills---limited-rules)
     * [2.2 Example: 3-for-2 discounts](#example-3-for-2-discounts)
     * [2.3 Example: Pricing phone calls](#example-pricing-phone-calls)
     * [Capturing the residual - limited rule](#capturing-the-residual---limited-rule)
   * [3. Depicting rules for matching](#depicting-rules-for-matching)
     * [3.2 Matching limitations](#matching-limitations)
   * [4. Overall usage strategy](#overall-usage-strategy)
   * [5. Focusing on the consumer](#focusing-on-the-consumer)
     * [5.1 Background information](#background-information)
     * [5.2 Implementing a bill-consumer](#implementing-a-bill-consumer)
   * [6. Implementation details of the SplitMatcher](#implementation-details-of-the-splitmatcher)
   * [x. Run-time complexity](#x-run-time-complexity)
   * [Summary](#summary)
   ...

   
## 1. Introduction

Business logic often involves matching entities. For example payments with invoices. More often than not, the matching process is not a simple one-to-one. 

Consider a fitness gym where Jolly Joe frequents but for some reason fails to pay his February membership fee of $9.99. Then in March he makes a single payment of $19.98 effectively paying for both months. It is quite obvious that we need to split his payment in order to match it with his dept. Or what if instead, Joe perhaps due to a misunderstanding paid $10.99 in February - effectively paying more than he should have. Now we balance out the invoice with the payment and take due action with the remainder.

While it may be tempting to merge all payments into a decimal number (and likewise for the depth entries) so that a simple subtraction can determine the action, it has been my experience, that it is advantageous to record each payment independently along with information on what it paid for. This become increasingly important when there are legislative requirements involved.

Most of the matching logic I have been involved with can be boiled down to a few common cases. In this article we will try to distill some of the matching rules commonly encountered when implementing business logic, and provide a reusable implementation. First though, there is a bit of terminology we need to get out of the way. When we use the word **"event"** we refer to something that occur like a payment, a usage of a service or the ordering of an order. When we use the word **"rule"** we denote some business rule that needs fulfilling. Forgive me as these definitions are intentionally vague and abstract.



## 2. Real life examples

Before we get to the actual code, let's tour through a few different scenarios showing the versatility in the kind of matching we wish to create a reusable solution for. All the examples have in common, that we need to split either the events (input) or support that rules are only partially matched. Hence we call this **split matching**. 




### 2.1 Example: Payments and bills - limited rules

An insurance company offering both mandatory (by law) and voluntary insurances. For example a car insurance and a dental insurance. Hairy Harry is a customer in said company and who happens to have both insurances (along with other insurances). Harry can choose to pay his monthly bill in a number of ways, for example by means of several smaller payments, or as one larger payment. For reasons unknown, he may choose to only pay a subtotal of what he owns - or even cancel payments (e.g. cancelling a cheque he has sent by mail).

Sometimes a payment is not just a payment. Several legal entities may take part in a payment, in which case we may need to restrict the products each legal entity may pay for. This is not as crazy as it sounds. In Denmark it is the norm that the employer pays monthly installments to the employees life insurance. The payment consists of payments from both employer and employee, and different rules apply in terms of investment profiles, tax exemption etc.

Bringing back the more formal terminology, we say that payment are *event* while each bill is a *rule* that captures a limited amount of payment. The rules for this example are all "limited" in the sense that there exist a threshold for how much of a payment a rule may consume. 



### 2.2 Example: 3-for-2 discounts
A discount model that encourages extra spending is the "Buy 3 items for the price of 2". By giving away the cheapest item, you get the consumer to buy an extra item.

To implement this, typically, items in the shopping basket eligible for the discount are ordered e.g. by price to ensure that the same discount is given irregardless of the order they were put into the basket. Here the shopping basket items serve as *events*. We model the discount by making the rule group items together and producing order lines. For each group an additional negative order line is produced for the cheapest item.



### 2.3 Example: Pricing phone calls

Imagine we need to calculate the total monthly cost for a phone subscription. Assume a phone call costs $0.50 pr. minute in weekdays, and $0.25 in weekends. To complicate matters, we can imagine the business frequently changing the pricing scheme. Perhaps introducing a discount making the first $10 worth of weekday calls each month free of charge.

We may model each call as an *event*, and describe *two rules*: One for calculating weekday calls and one for weekend calls. The rules are "unlimited", as they can consume as many events they like. 



### Capturing the residual - limited rule
When matching events with limited rules, the situation may arise where there is leftover resources in the events after the rule matching. Naturally, capturing any leftover is as important as capturing which rules were completed. We refer to any "leftover" as the **residual**. 

But rather than asking the match result for any residual, it may be cleaner to model the residual as any other rule. If such a rule is unlimited and the last rule to match, then any residual is automatically captured. This is really nice since no special attention needs be given in the split matching implementation.




## 3. Depicting rules for matching  
  
We have gone through a few examples of what a split matcher can be used for. Let's try to convey the same using pictures. The vertical size of the boxes is an attempt to define "size". So a small box may represent $10 while a larger box may represent say $20.

**Event and rules match one-to-one**

```
Event                     Rule
  |                         | 
 +-+                       +-+ 
 | |---------------------->| |
 +-+                       +-+
  |                         | 
 +-+                       +-+ 
 | |---------------------->| |
 +-+                       +-+
```

**A rule may not be fully matched/completed**

```
Event                     Rule
  |                         | 
 +-+                       +-+ 
 | |---------------------->| |
 +-+                       | | 
  |                        +-+  
```


**An Event may not be fully consumed**

```
Event                     Rule
  |                         | 
 +-+                       +-+ 
 | |---------------------->| |
 | |                       +-+
 +-+                        |
```



**Several Event required to match/complete a rule**

```
Event                     Rule
  |                         | 
 +-+                       +-+ 
 | |---------------------->| |
 +-+          +----------->| | 
  |           |            +-+  
 +-+          |             |   
 | |----------+            +-+  
 +-+                       | |
  |                        +-+             
```


**An Event may match/complete several rules**

```
Event                     Rule
  |                         | 
 +-+                       +-+ 
 | |---------------------> | |
 | |-------------+         +-+
 +-+             |          |
  |              |         +-+  
  |              +-------->| |
  |                        +-+   
```


**A Rule may match several non-consecutive Event**

```
Event                     Rule
  |                         | 
 +-+                       +-+ 
 | |---------------------->| |
 +-+          +----------->| |
  |           |            +-+    
 +-+          |             |   
 | |--------- | ----+      +-+  
 +-+          |     +----->| | 
  |           |            +-+     
 +-+          |             |   
 | |----------+             |
 +-+                      
```


**A Rule may be consume unlimited Event**

```
Event                     Rule
  |                         | 
 +-+---------------------->+-+ 
 | |       +-------------->| |
 +-+       |  +----------->+-+
  |        |  |             |      
 +-+       |  |            +-+   
 | |-------+  |            | |   
 +-+          |            +-+ 
  |           |             |    
 +-+          |             |   
 | |----------+           
 +-+                      
```




### 3.2 Matching limitations

It is important to stress, that we will severely limit the matching algorithm. We do not regard the split matcher to solve "optimization problems" like say the [knapsack problem](https://en.wikipedia.org/wiki/Knapsack_problem). I.e. we do not attempt all possible combinations of input and rules. Instead, we resolve by either allowing multiple event (or rules) to constitute a match. Algorithmically this is a simpler process. But just as importantly, a simpler matching algorithm is one that both customers and the business can understand and explain. I cannot stress this enough. When business rules become complicated, customers are at a loss, the business will have to spend costly customer service trying to explain the bill, and the customer service itself will have to go through costly training for themselves to understand the implications of each choice in order to be able to face customers.

Our matching algorithm takes place as follows.

* The algorithm is fed *ordered* lists of "Events" and "rules"
* For each rule, starting at the first rule
  * For each non-fully-consumed event (starting at the first event)
    * The rule is presented with the event and chooses to consume fully or partly or ignore. 
    * During this process the rule may announce to be fully consumed/matched, then no further events are served to the rule. 
* The end result is 
  * A list of match information which describe event-to-rule matching and to which degree the event is fully or partly consumed. 
  * A list of completed and uncompleted rules.


If you know your design pattern, you will find that this is similar to the *chain of command* design pattern - except we allow partial matching and we emphasize on separating the logic of selecting facts from the implementation of rules. This is more filtered together in the chain of command pattern.

  

## 4. Overall usage strategy

There are roughly two main approaches to using the split matcher. You can separate the matching process from a later process determining the consequences of each match. Or, you may choose to do *both* the matching and the side-effects of each match. 

Imagine again the pricing of phone calls. We can have either of the two flows

```list calls -> matching -> list matches -> pricing engine -> list prices```

or 

```list calls -> matching + pricing -> list prices```


It is a matter of temperament and how SOLID you want your code which strategy you choose. Possibly, as always complexity of the matching and further processing is key here. If both are straight forward, it may be natural to merge them into one step.




## 5. Focusing on the consumer

### 5.1 Background information
We have now gone through the conceptual ideas of matching, *events* and *rules*. It is time to dig into the implementation details. 

When a rule is served an *fact* (a conglomerate of *N* things) the *fact* can either be fully consumed (all *N*) or partially consumed (0 upto *N*). This consumption must be communicated back to the sender such that the particular *fact* is not served to any other consumer. Serving it to any other consumer would be pointless, as there is nothing more to consume. 

In addition, the consumer itself may have limits to its consumption abilities. Therefore the (part) consumption of a *fact* may render the consumer  completed.

It should be clear by now, that for each element of *input* we have two pieces of information to return.

The completion of a rule is a binary value that fit a `bool`. However, I prefer using an enum for the representation. This data structure much better express out intent, and it allows to be extended in the future. Go [here](http://firstclassthoughts.co.uk/Tags/Domain_Types.html) for more thoughts on domain types.

```
public enum ConsumerStatus { Active, Complete }
```

The consumption of some input is in fact a triple consisting of 

* The input
* The amount/volume consumed
* The rule that consumed 

Hence for a given input we'll potentially have a list of rules that has consumed parts of it. Rather than forcing all *input* to inherit such book keeping information, we wrap each *input* element in a `Matchable` class holding the information.

We are now finally ready to expose the interface all consumers must implement. It merely consist of a match method, returning the consumer status and taking as argument the `Matchable`.

```
public interface IConsumer<TFact, TConsumption>
{
    ConsumerStatus Match(Matchable<TFact, TConsumption> input);
}
```

Notice the type parameters `TFact` is simply the type of the fact. Fact are not required to implement anything, and thus do not share an interface.  `TConsumption` is the data type used for measuring the consumption. If we were to deal with phone calls this could be a `TimeSpan` or maybe a `decimal` or a  `Money` class if we were dealing with payments and bills.



### 5.2 Implementing a bill-consumer
Consumers all follow the same pattern. 


For unlimited consumers

* Check *input* against constraints, eg. a phone call made during the weekend
* Extract the unconsumed amount from *input*. This is important since preceding consumers could consume part of the input.
* Produce a match by consuming the input
* Return consumer status as Active

For limited consumers, the logic is a bit more complex

* Setup book keeping information to track consumption so far to enforce the limited nature of the consumer
* Check *input* against constraints, eg. a phone call made during the weekend
* Extract the unconsumed amount from *input*. This is important since preceding consumers could consume part of the input.
* Create a match of the minimum between the consumers limited consumption and the unconsumed input.
* If the consumer status as Complete or Active depending on the consumer being completed

Since the limited consumer is the most involved, perhaps it is the most interesting to dig into.

First the class and the constructor setting up the cost. The type parameters indicate that both the *input* and the consumption book keeping is done using `decimal`.

```
class MonthlyExpense : IConsumer<decimal, decimal> 
{
    readonly decimal monthlyCost;
    decimal CoveredByConsumer;

    public MonthlyExpense(decimal monthlyCost) {
        if (monthlyCost < 0)
            throw new ArgumentException("Negative cost");

        this.monthlyCost = monthlyCost;
    }
}
```

Now for the actual matching algorithm, it pretty much a codification of the above bullet list.

```
public ConsumerStatus Match(Matchable<decimal, decimal> payment)
{
    var leftOfPayment = payment.Input - payment.AccumulatedConsumption;

    var toPay = Math.Min(leftOfPayment, monthlyCost - CoveredByConsumer);
    CoveredByConsumer += toPay;

    var isPaymentComplete = toPay == leftOfPayment;
    payment.AddMatch(this, toPay, isPaymentComplete);

    return CoveredByConsumer == monthlyCost ? ConsumerStatus.Complete : ConsumerStatus.Active;
}
```


We can try exercising the code so far in a unit test. As you can see the matching data structure is requiring a little getting used to. Perhaps due to the simplicity of the consumer implementation itself.

```
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
    CollectionAssert.AreEqual(consumers, res.Matches[0].Consumers.Select(x => x.Consumer));

    // The first consumer is "m1" and has consumed $200
    Assert.AreEqual(m1, res.Matches[0].Consumers.First().Consumer);
    Assert.AreEqual(200, res.Matches[0].Consumers.First().Consumption);
    
    // The second input only paid monthly expense 2
    Assert.AreEqual(m2, res.Matches[1].Consumers.Single().Consumer);
}
```

We have pretty much exhausted the information we can squeeze out of `MatchResult` - the class that holds information about the 


```
public class MatchResult<TFact, TConsumption> 
{
    public readonly Dictionary<ConsumerStatus, List<IConsumer<TFact, TConsumption>>> Consumers; 
    
    public List<Matchable<TFact, TConsumption>> Matches;
}
```



## 6. Implementation details of the SplitMatcher

The following detail are not meant as required understanding in order to use the frame work. You can easily do without them. But if you really want to see what goes on under the hood, now is the time to take a thorough look.

We first define a delegate `ConsumptionAdder`. This is needed as for each consumer only partly consuming a fact, we need to accumulate the consumption. Since we do not know anything about the type of the `TConsumption`, we have to ask the caller to define how consumption is added together. In the above unit test, it was a simple decimal addition. But chances are, that the consumption be a complex monstrosity counting all sorts of dimensions of time, money etc. The framework does not prevent this if you require such complexity.



```
public class SplitMatcher 
{
    public delegate TFact ConsumptionAdder<TFact>(TFact consumedSoFar, TFact newlyConsumed);
}
```

Now for the heart of the algorithm. We iterate all consumers one at a time, serving it all non-fully consumed facts. If there are no more facts left, we only set the status of the consumer to `Active`. If a consumer is completed, we set it to `Complete`. 

Before we serve facts to the consumers, we first wrap them in a scaffolding structure that we mentioned above. This structure is responsible for tracking which consumers have consumed what amount of each fact.

```
public MatchResult<TFact, TConsumption> Match2<TFact, TConsumption>(
    IEnumerable<TFact> fact,
    IEnumerable<IConsumer<TFact, TConsumption>> consumers,
    ConsumptionAdder<TConsumption> consumptionAdder)
{
    var scaffolding = fact.Select(x => new Matchable<TFact, TConsumption>(x, consumptionAdder)).ToList();
    var result = new MatchResult<TFact,TConsumption>();
    var toProcess = scaffolding; 

    foreach (var consumer in consumers) 
    {
        if (!toProcess.Any()) 
        {
            result.Consumers[ConsumerStatus.Active].Add(consumer); 
            continue;
        }

        var consumerCompleted = toProcess
            .Select(element => consumer.Match(element))
            .Any(status => status == ConsumerStatus.Complete);

        if (consumerCompleted)
            result.Consumers[ConsumerStatus.Complete].Add(consumer);

        toProcess = toProcess.Where(x => !x.IsFullyConsumed).ToList();
    }

    result.Matches = scaffolding;

    return result;
}
```


## x. Run-time complexity

The runtime complexity of the algorithm is not easy to account for. The complexity is a function of *fact* and *consumers*. If all facts are handled by the first consumer, we have O(N), if all are handled by the last consumer, we have O(F*C) roughly O(N²), where *F* are the number of facts and *C* the number of consumers. *N* denote some abstract "input".


## Summary
We have presented a generally reusable framework to do split matching of facts and consumers. The code is freely available from www.github...

*Samuel Goldwyn  - "I'm willing to admit that I may not always be right, but I am never wrong."*




Please show your support by sharing and voting:
<SocialShareButtons>
</SocialShareButtons>



<br><br>
<CommentText>
</CommentText>

<br><br>
