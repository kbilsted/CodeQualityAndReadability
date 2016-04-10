# Strategies for making high performance thread safe code
*Author: Kasper B. Graversen*
<br>[[Introduction]](<BaseUrl/>) [[All categories]](<BaseUrl/>AllTags.html) [[All articles]](<BaseUrl/>AllArticles.html) [[Edit article <img src="http://firstclassthoughts.co.uk/img/edit.png"> ]](<GithubPageUrl/>)<br>

<Categories Tags=".Net,lock,Threads,Multi threaded programming, Semaphore, Interlocked">
</Categories>

**We explain three strategies for making thread safe code extremely fast. The strategies are called "Compare-and-exchange", "copy local scope" and "reducing the lock scope". The strategies are easy to apply in practice.**



Table of Content

   * [1. Introduction](#introduction)
   * [2. Revisiting the example problem](#revisiting-the-example-problem)
   * [3. Strategy 1: Compare-and-exchange](#strategy--compare-and-exchange)
   * [4. Strategy 2: Copy local scope](#strategy--copy-local-scope)
   * [5. Strategy 3: Reducing the lock scope](#strategy--reducing-the-lock-scope)
   * [6. Performance measurements](#performance-measurements)
   * [7. Conclusions](#conclusions)




## 1. Introduction
This is part 2 of a two-part series. In [part 1](DotNetLocksAreReallyFast.html) we looked at the overhead of the `lock` construct, which we found to be an overhead in the range of *3x* slower execution speed. It was interesting to see that the speed of `lock` itself was comparable to a busy-waiting spin-lock. That's pretty cool! It means that we need not worry about the lock overhead. It is important, however, to recall [Amdahl's law](https://en.wikipedia.org/wiki/Amdahl%27s_law) - which says that while `lock` is fast, threads waiting for a lock may incur serious impact on the general performance. This is explained with graphs and measurements in great numbers in [It is the contention that is expensive, not the cost of the lock](http://preshing.com/20111118/locks-arent-slow-lock-contention-is/).


In this installment, we will look at two strategies for reducing the overhead of thread safety. The main take-away - we will get very close to the speed of the thread-unsafe code. The strategies are 

	* Compare-and-exchange
	* Copy-local-scope
	* Reducing the lock-scope



## 2. Revisiting the example problem

The running example from [part 1](DotNetLocksAreReallyFast.html) was a `Sequence` class that emits a sequence of numbers, where it is guaranteed that the sequence does not contain duplicates and that there are no holes.

A **thread-unsafe** version is

```
public class Sequence_unsafe
{
    long count;
    readonly long start;
    const long Max = 9999999999L;

    public Sequence(long seed)
    {
        start = seed;
        this.count = seed;
    }

    public string Next()
    {
        count = count == Max
            ? start
            : count + 1;

        return string.Format("{0:D10}", count).Substring(0, 10);
    }
}
```


## 3. Strategy 1: Compare-and-exchange

One strategy to really squeeze the last bit of performance out of the code is to not protect state-mutating code in a safe region. Instead, rely on an API to thread-safely update a variable. of course, such strategy only works when you are updating *one* variable. Coincidentally, this is just what we are doing in our example. The be honest, it may not be that much of a coincidence - updating only a single variable inside a critical region is a fairly common use-case. 

In the previous installment we utilized the `Interlocked.Exchange` api to create a spin-lock, which turned out to be clumsy. Instead we turn to `Interlocked.CompareExchange()`.  `CompareExchange()` assigns the variable `a` if and only if two other values are equal. The signature in its full glory: `public static long CompareExchange(ref long location1, long value, long comparand)`. It looks a bit cryptic, but let's rewrite the code for `Next()` then it'll become much clearer.


```
public string Next()
{
    while (true)
    {
        var readCount = Interlocked.Read(ref count);
        var newCount = readCount == Max
            ? start
            : count + 1;

        if (Interlocked.CompareExchange(ref count, newCount, readCount) == readCount)
            return string.Format("{0:D10}", newCount).Substring(0, 10);
    }
}
```

Comparing this to the version in the version in the part-1 article we see that there is no semaphore anymore. We simply read the value, increment if no-one else have done so. If another thread has updated `count` between us reading it and wanting to update it, we simply take another iteration in the loop; Reading the variable and attempt incrementing it.



## 4. Strategy 2: Copy local scope

Another approach to speeding up the code is to minimize the time we spend inside our critical region (our lock). Let's repeat the old implementation first for easier comparison. It is fairly standard; Enclose the whole method in a `lock`.

```
public class Sequence_from_prequel_slow
{
    ....
    readonly Object lockObject = new Object();
    
    public string Next()
    {
        lock(lockObject)
        {
            count = count == Max
                ? start
                : count + 1;

            return string.Format("{0:D10}", count).Substring(0, 10);
        }
    }
}
```

The hint here to improving speed, is that 

  * `string.Format()` and `Substring()` are costly compared to the cost of `lock`. 
  * These operations does not mutate state defined outside our `Netxt` method.

Recall, that with each method invocation, state declared inside the method is instantiated. Hence code that only operate on method-local state is thread safe out of the box. So by making a local copy of the state that needs be type safely mutated, we can leave the rest of the code outside the lock. 

Really, this is simply a repeat of the theory explained by [Amdahl's law](https://en.wikipedia.org/wiki/Amdahl%27s_law). So let's apply theory to practice:

```
public string Next()
{
    long countLocalCopy;
    
    lock (lockObject)
    {
        count = count == Max
            ? start
            : count + 1;
            
        countLocalCopy = count;
    }

    return string.Format("{0:D10}", countLocalCopy).Substring(0, 10);
}
```




## 5. Strategy 3: Reducing the lock scope

Sometimes we cannot employ the "copy-local-scope", especially when the state is expensive to copy - for example when we are dealing with a collection of state. In these situations we can look for ways to reduce the scope of locks. A real-life example from the [Stateprinter project](https://github.com/kbilsted/StatePrinter/blob/10d524cc72026c3cacf5e7ab6dfbbbad1b8ead39/StatePrinter/FieldHarvesters/RunTimeCodeGenerator.cs) is the `RunTimeCodeGeneratorCache`. Briefly, the Stateprinter project set sail in trying to automate aspects of unit testing that had to do with expressing and updating the "assert"-parts of unit testing. The project is still maturing, but is definitely worth looking at.

Anyways.. The job of the cache is to see if there is a cache hit, and if not, generate a value and store it in the cache. A straight forward way for writing this is by creating a lock, inside which we do the lookup, and possibly code generation.

```
public Func<object, object> CreateGetter(MemberInfo memberInfo)
{
	lock (cache)
	{
		Func<object, object> getter;
		
		if (cache.TryGetValue(memberInfo, out getter))
			return getter;
		
		// expensive
		var generatedGetter = generator.CreateGetter(memberInfo);
		cache.Add(memberInfo, generatedGetter);
		
		return generatedGetter;
	}
}
```

The problem with this implementation is that we are holding a global lock on the cache with the `lock(cache)` statement. This means, that while we are calling the generator and run-time generating code (this is time consuming) we are *blocking* for cache lookups of values in the cache.

Since the code generation is thread safe in itself, we can split our lock-scope into two, and thus only lock the critical places of the code - the places where shared state is accessed. Thus our code becomes

```
public Func<object, object> CreateGetter(MemberInfo memberInfo)
{
	Func<object, object> getter;
	lock (cache)
	{
		if (cache.TryGetValue(memberInfo, out getter))
			return getter;
	}


	// expensive
	var generatedGetter = generator.CreateGetter(memberInfo);

	lock (cache)
	{
		if (cache.TryGetValue(memberInfo, out getter))
			return getter;

		cache.Add(memberInfo, generatedGetter);
	}

	return generatedGetter;
}
```

Since our running example is too short for us to employ this strategy, we have not timed and compared it with the other strategies.



## 6. Performance measurements

We do the same performance measurements as in the previous article. This means we are calling `Sequence` 800.000 * 13 times. The running times are staggeringly close to the performance of the thread unsafe code!

| Approach              | Time         
|-----------------------|----------------:
| unsafe                | 1.7 seconds
| Compare-and-exchange  | 2.2 seconds
| Copy-local-scope      | 2.7 seconds
 
 Compare this with the performance numbers from the [previous article](DotNetLocksAreReallyFast.html):

| Approach            | Time         
|---------------------|----------------:
|using lock           | 6.7 seconds
|using Interlocked    | 6.5 seconds

notice how much closer we are to the running-time of the thread-unsafe code! 



## 7. Conclusions

Using either of the two first techniques, we are getting very close to the running-time of the thread-unsafe code. The `Interlocked` is faster but also less general.


Please show your support by sharing and voting:
<SocialShareButtons>
</SocialShareButtons>


<br><br>
<CommentText>
</CommentText>

<br><br>
