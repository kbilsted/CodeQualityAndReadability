# From imperative to declarative code using LINQ extension methods
*Author: Kasper B. Graversen*
<ArticleHeaderUrls/><Categories Tags="Code_Readability, Coding_Guideline, Looping, Iteration, for, LINQ">
</Categories>

*In this article we show how break down and separate unrelated business logic of a program. By creating a LINQ extension method, the separated code is very easy to reuse across the application, further, it transform the code from being imperative to declarative.*

Please show your support by sharing and voting:
<SocialShareButtons>
</SocialShareButtons>


<img src="img/chameleon-885595_640.jpg">


Table of Content

   * [1. Introduction](#introduction)
   * [2. A use case](#a-use-case)
     * [2.1 Modelling intervals](#modelling-intervals)
     * [2.2 Modelling coverage of intervals](#modelling-coverage-of-intervals)
     * [2.3 Splitting an interval](#splitting-an-interval)
   * [3. Adding a configuration element](#adding-a-configuration-element)
     * [3.1 The business logic expressed imperatively using `for`](#the-business-logic-expressed-imperatively-using)
     * [3.2 The business logic expressed declaratively using LINQ](#the-business-logic-expressed-declaratively-using-linq)
   * [4. Abstraction leads to general applicability](#abstraction-leads-to-general-applicability)
   * [5. Discussion](#discussion)
     * [5.1 The problem with lesser experienced developers](#the-problem-with-lesser-experienced-developers)
     * [5.2 Separation of concerns](#separation-of-concerns)
     * [5.3 Over engineering?](#over-engineering?)
     * [5.4 Performance implications](#performance-implications)
     * [Summary](#summary)
   
   
## 1. Introduction
The other day I wrote [Restrict expressibility when iterating](RestrictExpressibilityWhenIterating.md) it serves as the theoretical foundation for this article. The main take-away from it is how readable code is achieved through using code constructs that are limited in expressibility. Thus, a LINQ expression is typically easier to reason about than say a `for` loop due to the limitations of what you can do. We continue this  trail of thought, by showing an example where we turn logic expressed with a `for` loop into using LINQ.

This implementation strategy has many advantages which are detailed later in the article, but to wet your appetite, let's summarize

* We separate concerns, the code becomes more SOLID and DRY.
* We get a generally reusable component.
* We can easily test the code in isolation.
* We turn usages of `for` into LINQ, making it composable with the rest of the LINQ universe.
* We turn imperative code into declarative code. This means we shift focus from *how* to *what*.

In essence, we are developing a separate `Replace()` method that naturally extend the LINQ universe.



## 2. A use case
Please excuse me for this whole section 2. I want to take outset in *real life code*, and thus the scene must be set for a concrete problem. This way, hopefully it becomes more relevant to you. I hope it is not at the expense of you being bored to tears and quitting out. 


### 2.1 Modelling intervals

For a constraint solver I am building I need a way to express a span of values, say from -2 up to but, not including, 30. We can depict this as:


```
--[----------------------|----
 -2                      30
```

We use `[` to represent inclusive and `|` to denote exclusive. we can achieve this by creating a `Span` class, like a Tuple holding two values, and instantiate it with:

```
new Span<int>(-2, 30);
```

In addition to holding the values,  we also implement an overlap check `IsOverlapOf()`.



### 2.2 Modelling coverage of intervals

Given a collection of these intervals, we want to track if they together span the whole range for the given data type. E.g. `int` has the range of -2147483648 to 2147483647. If we define the interval -2 to 30, we have two uncovered intervals, namely that of -2147483648 upto -2 and 30 upto 2147483647.

Graphically we can represent the uncovered interval as starting with

```
     [-------------------------------] 
int.MinValue                    int.MaxValue
```

and after configuring the interval (-2, 30) the uncovered interval becomes

```
     [----------|     [-------------------]
int.MinValue   -2     30             int.MaxValue
```

We implement an collection administering this information, by holding both the configurations and the uncovered intervals.

```
public class IntIntervalCollection
{
    List<Span<int>> Configurations = new List<Span<int>>();
    List<Span<int>> UncoveredIntervals;
}
```
 
 
### 2.3 Splitting an interval

On the `Span` class we can create a method, that given an interval, produces 0, 1, or 2 sub intervals where the given interval is excluded. What for? Well, to cater for the very situation depicted above. 

```
public class Span<T> where T : IComparable<T>
{
    public readonly T From, Upto;

    public List<Span<T>> RemoveInterval(Span<T> interval)
    {
        var result = new List<Span<T>>();
 
        if (Compare.LessOrEquals(From, interval.From))
            result.Add(new Span<T>(From, interval.From));

        if (Compare.Greater(Upto, interval.Upto))
            result.Add(new Span<T>(interval.Upto, Upto));

        return result;
    }
}
```  

Basically, the gist of it is that when the input starts after the current interval, we create a span covering up to the input start. Like-wise in the other end. Assume we have an interval of the full range of `int` and we call `RemoveInterval()` with an (-2, 30), we get two intervals: `(int.MinValue, -2)` and `(30, int.MaxValue)`. Assuming we understand the graphics above this is as expected.




 
 
## 3. Adding a configuration element

Now the scene has been set. The problem introduced along with all the pieces needed. Now we just need to assemble them. 

When adding an element to our `IntIntervalCollection` collection, we add it so the `Configurations` list, and subtract the interval from the `UncoveredIntervals` using the `RemoveInterval()`.

There are many ways to doing this. I'll show how I first approached this using low-level imperative `for` with indexing. When I had a working solution, I immediately refactored it into a declarative LINQ extension. After analysing the end-result, through writing this article, I feel quite content with spending that bit of energy on doing the refactoring.



### 3.1 The business logic expressed imperatively using `for`

The following code will

* Find the index of a match
* Replace the match with intervals excluding the new configuration

```
public class IntIntervalCollection
{
    public void AddConfiguration(Span<int> configuration)
    {
        for (int i = 0; i < UncoveredIntervals.Count; i++)
        {
            if (UncoveredIntervals[i].IsOverlapOf(configuration))
            {
                var tmp = UncoveredIntervals[i];
                var splitIntervals = tmp.RemoveInterval(configuration);

                UncoveredIntervals.RemoveAt(i);
                UncoveredIntervals.InsertRange(i, splitIntervals);
                
                break;
            }
        }
        
        Configurations.Add(configuration);
    }
}
```

In the outset this code is not too bad. Sure we are iterating over a list of elements and potentially could perform funky stuff on `i` while iterating. But the whole code fits on my screen. You'' notice that the style is very imperative. Step-by-step instructions are made to iterate over the uncovered steps, and if we have a match we replace and break out. We can extract the iterating part of the logic as shown below: 


```
public class IntIntervalCollection
{
    public void AddConfiguration(Span<int> configuration)
    {
        int? pos = FindOverlap(configuration);
        if (pos.HasValue)
        {
            var tmp = UncoveredIntervals[pos.Value];
            var splitIntervals = tmp.RemoveInterval(configuration);
 
            UncoveredIntervals.RemoveAt(pos.Value);
            UncoveredIntervals.InsertRange(pos.Value, splitIntervals);
        }
        
        Configurations.Add(configuration);
    }

    int? FindOverlap(Span<int> configuration)
    {
        for (int i = 0; i < UncoveredIntervals.Count; i++)
        {
            if (UncoveredIntervals[i].IsOverlapOf(configuration))
                return i;
        }
        
        return null;
    }
}
```

We are moving towards a more declarative coding style in the `AddConfiguration()` method. And w have separated the logic between searching and replacing. Notice how we are closer to the bullet point description served above. Definitely a step in the right direction. But we can do better!

Of course there are many ways to cut the cake in terms of which parts of the code we want to extract into which methods. This is just what came to mind first. Thus this is in no way the "authoritative" approach.



### 3.2 The business logic expressed declaratively using LINQ

As motivated in [Restrict expressibility when iterating](RestrictExpressibilityWhenIterating.md) using LINQ over `for` often leads to more readable code. The code shown in 3.1 is somewhat unclear if we look at the methods from a 5 mile perspective. There is a method for adding, and   a method for finding an overlap. Zooming in on the `AddConfiguration()`, we see some removing and inserting. Looking even closer, we discover that the removing and inserting is related in position. *In actuality, what we are dealing with here is a replacement*. 

So really, the code is self explanatory when expressed in terms of a "replacement". Maybe something like:

```
public class IntIntervalCollection
{
    public void AddConfiguration(Span<int> configuration)
    {
        UncoveredIntervals = UncoveredIntervals.Replace(matcher, replacer);
        
        Configurations.Add(configuration);
    }
}
```

LINQ does not provide a replace, but we can easily create one on our own. 

From the above pseudo code, we have 3 abstractions in play here. A `Replace()` LINQ method, a `matcher` which can pin point the element to be replaced, and a `replacer` which given an element to replace, produce a list of elements to replace with.

* `matcher` is easy, we can use the `IsOverlapOf()` method presented above.
* `replacer` is just as simple, it we use the `RemoveInterval()` method presented above.
* The `Replace()` method. It's implementation must iterate through the collection, and calling the `matcher` on each element. If an element matches, we call `replacer`. 

The below code matches this description quite nicely. The code uses a few tricks. The argument is prefixed with `this` making it an extension method. It returns and takes as first argument a `IEnumeratble<T>`. We are using `yield` when returning elements of a sequence. The three together is what is required to make a LINQ extension method. We will not go in-depth with any of these constructs, as it is outside the scope of this article and is well addressed on the internet.

```
public static class LinqReplacer
{
    public static IEnumerable<T> Replace<T>(
        this IEnumerable<T> elements,
        Func<T, bool> match,
        Func<T, IEnumerable<T>> replacer)
    {
        bool hasReplaced = false;
        
        foreach (var element in elements)
        {
            if (!hasReplaced && match(element))
            {
                hasReplaced = true;
                foreach (var replaced in replacer(element))
                {
                    yield return replaced;
                }
            }
            else
            {
                yield return element;
            }
        }
    }
}
```

Our pseudo code now as *real* C# code.

```
public class IntIntervalCollection
{
    public void AddConfiguration(Span<int> configuration)
    {
        UncoveredIntervals =
            UncoveredIntervals.Replace(
                x => x.IsOverlapOf(configuration),
                x => x.RemoveInterval(configuration))
                .ToList();

        Configurations.Add(configuration);
    }
}
```


## 4. Abstraction leads to general applicability

The `Replace()` method is a general abstraction over a common re-occurring problem. And since we do not constraint the type parameter `T`, we can apply it to all sorts of type. And problems. How about playing around with string replacement?

```
var input = new[] { "Hello", "World", "Bye", "Again" };
var replaced = input.Replace(x => x.StartsWith("A"), x => new[] { x.ToLower()});

var result = string.Join(", ", replaced);
Console.WriteLine(result);
```

which prints: `Hello, World, Bye, again`.

Now that we have a basic notion of a `Replace()` in a LINQ context, we can start thinking in terms of it. Giving inspiration to how we implement the `Replace()`. Perhaps we should rename `Replace()` to `ReplaceFirst()` and create a `Replace()` that does not bail out after the first match. Or how about a `Replace()` that is aware of the ordering of the input and thus can do optimizations based on this knowledge. It is an open territory for ideas to be seized. Testing is easy since we have abstracted away concrete business cases.

But perhaps more importantly, we can also think of using `Replace()` in the context of solving other business requirements. The string example was not an important example, it merely shows how wide a range of problems we can apply this to.




## 5. Discussion

I will argue that the `Replace()` abstraction leads to higher quality software, that is easier to read and reuse. Yet there is a thing or two to be said.


### 5.1 The problem with lesser experienced developers
Admittedly, it may be a bitter pill to swallow for a less experienced programmer, the first time he looks at the implementation of `Replace()`. It may be outside his domain of knowledge 

* Thinking in streams.
* Not knowing the semantics of `yield` or `this` as the first "method argument".

But aren't we blowing things out of proportion? A less experienced developer may not even recognize that we are not using the standard LINQ methods supplied from Microsoft. Secondly, the `yield` semantics have been around for a long time now. That along with LINQ has been well established to be a good idea: From Smalltalk in 1967, to C# (2007), heck even Java jumped wagon (2015)!. 

I have not written a LINQ extension method in over a year, yet I was able to spew out the `Replace()` in one go. Without needing to look up anything on the internet. It was bug free. Testing took longer to write than the code itself. So let's not exaggerate complexity here when you have first learned how LINQ operates and how to do extension methods.  

Besides getting the lesser experienced up to speed in terms of expressing code using the full potential of the programming language is a small price to pay. Let alone, all the future implementations the now less-lesser-experienced will be writing. Understanding the capabilities of the language in which you program can only lead to something good.

It is possible to create a `Replace()` method that is not integrating with LINQ, operating on `List<>` and not using the "extension method semantics" of C#. Surely, that would make the code look more familiar to the less experienced, but we will loose out on the composibility with the rest of LINQ which would be a shame.



### 5.2 Separation of concerns
What we have achieved here may *seem* a bit complicated to programmers not used to using LINQ or seeing LINQ extensions. And you can argue, that the number of lines certainly have increased. Line count, however, is very subjective. What looks longer may in fact be shorter! Every time we start re-using `Replace()` in Never the less, we have achieved something *"magical"*. 

* We have completely separated *unrelated* business logic, following so many design principles, such as DRY, SRP, etc. Really, the more you think about it, the collection of intervals has **nothing** to do with replacing on element with possibly 0-2 other elements. 
* We have created a *reusable* component, as we showed in 4., we can apply it to many other domains.
* It allows us to separately test and reuse the `Replace()` making the overall code easier to reason about since it has been broken down.
* The end-code has been changed from an imperative style to a declarative style.



### 5.3 Over engineering?
Is this a case of over engineering? A bored programmer spending an afternoon conjuring up magical artefacts rather than droning XML configuration files? I believe not! The refactoring was not time consuming. We have a reusable component. Our `IntIntervalCollection` (and all the other collection classes for dates, long, etc. I need implementing) benefits from the abstraction. Both in terms of code size and complexity.




### 5.4 Performance implications
I have not made any performance measurements. If you feel inclined, please do so and share your experiences. But even if it turns out that the `Replace()` using LINQ is 5 times slower than the "low level" in-array solution presented first, I wouldn't hesitate to using `Replace()`. When I have a somewhat or complete application I will performance measure the most common use cases and let that be my guide. 



### Summary
We have shown how to turn a `for` into LINQ, thereby reducing the expressibility of the code. Separating unrelated business code that can be independently tested and re-used. We turn imperative code into declarative code. This means we shift focus from *how* to *what*. 

Please show your support by sharing and voting:
<SocialShareButtons>
</SocialShareButtons>



<br><br>

<CommentText>
</CommentText>


<br>
<br>
<br>

