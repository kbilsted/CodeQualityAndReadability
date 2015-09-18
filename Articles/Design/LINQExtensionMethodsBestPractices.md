# The 6 best practices for writing LINQ extension methods
*Author: Kasper B. Graversen*
<br>[[Introduction]](<BaseUrl/>) [[All categories]](<BaseUrl/>AllTags.html) [[All articles]](<BaseUrl/>AllArticles.html)
<br>
<Categories Tags="LINQ, Extension_Methods">
</Categories>


*LINQ extension methods are often easy to describe, and quickly you can build something that seems to work. But there are pitfalls that you must be aware of in order for your code to work in the general case. This article describes the best practices you need to follow when writing LINQ extension methods.* 

Please show your support by sharing and voting: 
<SocialShareButtons>
</SocialShareButtons> 


<img src="img/pixabay.com_da_industry-569145_640.jpg" alt="from https://pixabay.com/da/industri-pumper-udstyr-teknologi-569145/">


Table of Content

 * [Introduction](#introduction)
   * [1. Use a separate namespace](#use-a-separate-namespace)
   * [2. Use descriptive names](#use-descriptive-names)
   * [3. Clean up after iterating](#clean-up-after-iterating)
   * [4. Don't re-iterate the input](#dont-re-iterate-the-input)
     * [4.1 Expensive lazy input](#expensive-lazy-input)
     * [4.2 Non-reiteratable input](#non-reiteratable-input)
     * [4.3 Solutions](#solutions)
   * [5. Don't assume input fits RAM](#dont-assume-input-fits-ram)
   * [6. Avoid side-effects if possible](#avoid-side-effects-if-possible)
   * [7. Summary](#summary)
   
   
   
   
   

# Introduction

Extension methods and LINQ are great inventions and make it really nice writing C# code as it enables you to write more declarative and more maintainable code. We have documented this [here](http://firstclassthoughts.co.uk/Articles/Readability/RestrictExpressibilityWhenIterating.html) and [here](http://firstclassthoughts.co.uk/Articles/Readability/FromImperativeTtoDeclarativeCodeUsingLINQ.html). Unfortunately, there are  pitfalls one can easily fall into when dealing with LINQ and the `IEnumerable<T>` type in general. 

We will not go into explaining the semantics of [`yield`](https://msdn.microsoft.com/en-us/library/9k7k7cf0.aspx), nor [how to create extension methods](https://msdn.microsoft.com/en-us//library/bb383977.aspx). There are plenty of resources on the internet for that.

The list of advice is ordered in a "top-down" fashion.


## 1. Use a separate namespace

Inside your IDE, it would be very annoying if every time you press `.` on an instance variable, every extension method under the sun is suggested to you. Fortunately, extension methods only become available when imported by the `using` clause. To get the standard LINQ, you must declare `using System.Linq`. 

It is a good idea to define your extension methods in a separate namespace, such that any usage becomes explicitly "opt-in" through an import clause. Of course, herein lies the danger that your collogues will be oblivious to the existence of said extensions. Especially when the code grows. 

To eliminate that danger, use the *same-ish* namespace for the extensions throughout the modules of an application. It would be silly to insist on using exactly the same namespace, instead we suggest keeping only the last part of the namespace static. For example `com.eggcorp.shared.linqextensions`. Here the `linqextensions` is the denominator which is unchanged across the code complex. 

Others prefer using the top-level name instead, so it'll almost always be imported. E.g. `com.eggcorp`. 

The important thing is to have a strategy that the whole team understands and honors.




## 2. Use descriptive names

It should come as no surprise that we want  good names for our methods. Alas, this is easier said than done. And it is notoriously difficult to give advice  on naming due to the subjectivity of names. Personally, I try to strike a balance between naming and naming style. The name must tersely capture what the method does while minding the naming style of the existing LINQ methods.

Hence I would typically not call my methods "Get.." or "Calculate..". Names that might otherwise be valid for normal methods.



## 3. Clean up after iterating 

LINQ extension methods iterate the source elements in order to produce a result. There are two typical implementation patterns for iteration. The `foreach` and the "enumerator approach". I'll show them both here with an implementation that does absolutely nothing.

First the `foreach`. It is a straightforward iteration of the input returning each element in sequence.

```
public static IEnumerable<T> NoOperation(this IEnumerable<T> source)
{
    if (source == null) 
        throw new ArgumentNullException("source");
  
    foreach(var element in source)
    {
        yield return element;
    }
}
```

We can achieve the same semantics with an enumeration approach. 

```
public static IEnumerable<T> NoOperation<T>(this IEnumerable<T> source)
{
    if (source == null) 
        throw new ArgumentNullException("source");

    using (var enumerator = source.GetEnumerator())
    {
        while (enumerator.MoveNext())
        {
            T element = enumerator.Current;
            yield return element;
        }
    }
}
```

The latter example is more low-level than the first. On the other hand, it provides more freedom while iterating. Examples are provided of this later in the article. Notice that he result of `GetEnumerator()` is of type `IDisposable` and thus we have to remember to clean up. That's why we use the `using`-block. 

I prefer using `foreach`. It is higher level, automatically takes care of disposing the underlying enumerator.





## 4. Don't re-iterate the input 
Due to LINQ expressions being deferred in execution, it is a general rule of thumb not to re-evaluate the expression more than once. For LINQ extension methods, this is even more important not to re-iterate. You have to be diligent, as it is very to unintentionally do the wrong thing. Imagine some business logic requiring elements to be present. In the general case, it is absolutely wrong expressing this as:

```
public static IEnumerable<T> SomeOperation<T>(this IEnumerable<T> source)
{
    if (source.Any())
    {
        ...
    }

    foreach (var element in source)
    {
        ...
    }
}
```

You may test it against an array, or some other input and it may *seem* fine. But it's not. Another example. Business logic require you to inspect both the current and the next element for filtering. With LINQ this is not straight forward as you operate on one element at a time. But you can transform your input to a stream of tuples holding the 'current' and the 'next' elements, by combining the sequence (using `Zip`) with itself juxtaposed by one (using `Skip`). For example

```
public static IEnumerable<T> SomeOperation(IEnumerable<T> source)
{
    var onlyABs = source
        .Zip(source.Skip(1), (current, next) => new { current, next })
        .Where(x => x.current == "a" && x.next == "b");
    ...
```

Similar to before, both `Zip` and `Skip` are causing the `source` to be evaluated and iterated over twice. And there are roughly infinitely many such examples of accidental re-iteration. We shall make no attempt going through every single one of them. 

There are two problems with multiple iterations over the source input 

* It is unnecessarily expensive.
* It may be impossible

We will deal with both problems one at a time


### 4.1 Expensive lazy input
When working on lists and arrays, there are no problems iterating over them again and again. But operating on `IEnumerable<T>` means that we don't really know what the input is. It could be a lazily evaluated LINQ expression. Evaluating it multiple times may turn into a performance bottleneck. Let's look at some silly input.

```
var newYork = cities.Where(x.City == "New York");
var newYorkerRichards = people.Where(x => x.Name("Richard") && x.City = newYork);
var billing = newYorkerRichards.SomeOperation();
```

Not only is this careless LINQ code with a run-time complexity of O(N^2) (quite needlessly), but by using any of the above `SomeOperation()` implementations, we incur this cost **twice**. Recall that `newYorkerRichards` is not a set of Richard-values. It is a lump of code that will evaluate upon iteration. And when we iterate twice, we compute the set of New York Richards twice. Code with complexity O(N^2) scales incredibly bad. Try for yourself with using some thousands of names and cities. 

The LINQ expression could also be a database query. I can well imagine bogged down servers due to multiple round-trips to the database. Over and over again executing the same SQL. **Shivers!**


### 4.2 Non-reiteratable input
It is easy to fall into the trap of thinking that the input can be iterated multiple times. We do this because most of the time, it is possible. The are no problems re-iterating the standard collection types list, array, dictionary and so forth. But in the general case, *not all streams are seekable*. We use seeking to go back to the start of the stream and start over. That is what the [`Seek()`](https://msdn.microsoft.com/en-us/library/system.io.stream.seek(v=vs.110).aspx) and [`CanSeek`](https://msdn.microsoft.com/en-us/library/system.io.stream.canseek(v=vs.110).aspx) abstractions are for. Microsoft has envisioned a number of situations where things have gone astray, and in the documentation for `Seek()` it is mentioned that it can throw the following exceptions `IOException`,
`NotSupportedException`, and `ObjectDisposedException`. For example, network streams (like GET and POST) are non-seekable, and certain file streams, e.g. those accessing tape hardware.

Recall, that we have no ability to restrict the kind of input when we're taking `IEnumerable<T>` as an argument. Best practice, therefore, is to avoid reiteration.



### 4.3 Solutions

We can solve this by ensuring we only iterate once. For the first example with logic to be called if elements are present, we can easily rewrite it into:

```
public static IEnumerable<T> SomeOperation<T>(this IEnumerable<T> source)
{
    bool isFirst = true;
    
    foreach (var element in source)
    {
        if (isFirst)
        {
            isFirst = false;

            ...
        }
        
        ...
    }
}
```

The code forces a comparison against `isFirst` on each iteration. The cost of this comparison may well be negligible due to the JIT compilation and CPU branch predictions. But to show the versatility of the enumeration approach, let's look at how the comparison can be eliminated.


```
public static IEnumerable<T> SomeOperation<T>(this IEnumerable<T> source)
{
    using (var enumerator = source.GetEnumerator())
    {
        if (enumerator.MoveNext())
        {
            ...
        
            do 
            {
                T element = enumerator.Current;
                ...
            } while (enumerator.MoveNext())
        }
    }
}
```




## 5. Don't assume input fits RAM
A faulty reasoning is to remedy the situation above by copying the result of the source into an array, which we know is re-iteratable. What an elegant idea! In the below code we introduce `elements` for this. 

```
public static IEnumerable<T> SomeOperation<T>(this IEnumerable<T> source)
{
    var elements = source.ToArray();
    var onlyABs = elements
        .Zip(elements.Skip(1), (current, next) => new { current, next })
        .Where(x => x.current == "a" && x.next == "b");
    ...
```

Unfortunately, this strategy carries with it problems of its own. Operating on `IEnumerable<T>` means that the input may be a lazily evaluated and potentially be very big. Possibly even infinite. You need not look hard to find examples. An academic example would be to pass in the set of all primes. A  business example is some business logic operating on all rows of a database table. A really big table. A few billion lines is more than enough to death grip your RAM if you try to hold it all at one time. And holding it all in RAM is exactly what we do when we issue the `ToArrray()` or similar construct.

Since we can't encode a database in an article, let's continue with the primes example. First we need to define an infinite set of all primes:

```
public IEnumerable<BigInteger> GetAllPrimes() 
{
    var candidate = new BigInteger(1);
    while (true) 
    {
        if (IsPrime(candidate))
            yield return candidate;

        candidate = BigInteger.Add(BigInteger.One, candidate);
    }
}
```

Calling `GetAllPrimes().SomeOperation()` quickly get you out of memory. 

Lazily evaluated infinite streams are not everyday food for the business programmer. But when writing general LINQ extension methods, we do not know what kind of input we are being served. Streams from databases or other combinations of large input can get you out of memory. You'd be surprised, how little RAM application servers are be armed with due to their high hosting costs. 





## 6. Avoid side-effects if possible

Due to the lazy nature of LINQ, your code may be executed at unexpected times. The code

``` 
var someCities = cities
        .SomeOperation()
        .Where(x => x.City.Contains("Y"));
```

Does **not** execute the code in `SomeOperation`! We can easily return our `someCities` variable and pass it onto a completely different method/context, in which it will be executed or may be passed on again. This can lead to difficult to trace bugs since the error is very far from the origin. And it can get especially nasty when the side-effect accesses entities using external resources such as a database connection that may well have been closed.

Naturally, there will be times where side-effects is the only viable solution. So use them. Just think twice before you jump that wagon. 

 
 


## 7. Summary

The main takeaway from this article is a checklist of things to go through each time you write a LINQ extension method.

  1. Use a dedicated and easy-to-find namespace for your LINQ extension methods.
  2. Invest energy into proper naming.
  3. Iteration requires disposing resources.
  4. Do not re-iterate the input stream, it may be *expensive*, and even *impossible* at times.
  5. Do not assume that the input stream *fits in RAM*.
  6. Due to the *deferred execution* of LINQ expressions, you best avoid side-effects.



Please show your support by sharing and voting: <SocialShareButtons></SocialShareButtons> 

<br><br>
<CommentText>
</CommentText>

<br><br>
