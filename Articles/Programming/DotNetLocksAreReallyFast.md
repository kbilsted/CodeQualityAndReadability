# .Net `lock` is really fast!
*Author: Kasper B. Graversen*
<br>[[Introduction]](<BaseUrl/>) [[All categories]](<BaseUrl/>AllTags.html) [[All articles]](<BaseUrl/>AllArticles.html) [[Edit article <img src="http://firstclassthoughts.co.uk/img/edit.png"> ]](<GithubPageUrl/>)<br>

<Categories Tags=".Net,lock,Threads,Multi threaded programming, Semaphore, Interlocked">
</Categories>

**The lock() construct in .Net is surprisingly fast. We will cover just how fast it is and show an easy approach to testing a simple scenario.**


Please show your support by sharing and voting:

<SocialShareButtons>
</SocialShareButtons>



Table of Content

   * [1. The unsafe approach](#the-unsafe-approach)
   * [2. Thread safe version using `lock()`](#thread-safe-version-using)
   * [3. Building a low-level lock using `Interlocked`](#building-a-low-level-lock-using)
   * [4. Performance measurements - wow lock is fast!](#performance-measurements---wow-lock-is-fast)
   * [5. Testing multi-threaded code](#testing-multi-threaded-code)
   * [6. Conclusion](#conclusion)
 

At my company I was doing multi-threaded programming for an integrations project with a third party application. At some point we wondered what the speed of the `lock()` construct was. Was it a slow operation to be used with care? Rather than relying on Internet hear-say I measured the situation for my use case. 

The third party system required for each call a sequence number to be supplied with calls. Since we do communication through multiple threads, we need a `Sequence` abstraction that is shared among the threads. Let's give this a first stab.

Notice, this is a two-part article, part two [is found here](StrategiesForFastThreadSafeCode.html)
## 1. The unsafe approach

```
public class Sequence 
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
    
This implementation clearly is **not thread safe**, two threads calling `Next()` can get the same same sequence number. 


## 2. Thread safe version using `lock()`
To ensure thread safety, we draw upon the oldest trick in the book, protect the critical regions. We use C#'s `lock()`. Notice how the scope of the lock covers the whole method. If we only lock the code that changes `count` we may return duplicate values. For an example as simple as this, there are opportunities for reducing the running time. But more on that later.


```
public class Sequence 
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

Also notice that we are using an explicit object `lockObject` for the lock. This is a better practice than using `this` as the lock object. Locking on `this` *may* be dangerous. Nothing prevents other parts of the system to also use the `Sequence` instance for their locking - in turn potentially creating non-trivial deadlocks. By using a private object we shield ourselves from such  a predicament.



## 3. Building a low-level lock using `Interlocked`

So locks are fairly advanced in concept. They involve some registration-notification mechanism by which to waking up waiting threads upon the release of a lock. All such registration and notification surely *must incur a cost*. Now that we have a version without locking and one with locking, it is pretty easy to measure the difference. That will tell us the overhead, and there is one, but the difference may be difficult to fully appreciate. The reason being that the more we stuff into the locked region the slower the `lock` construct will seem to be. Thus I'm interested in measuring differently.

Instead we implement a low-level locking mechanism ourselves. We busy wait for the ressource rather than all those fancy-pancy notifications. By using the `Interlocked` Api, we can implement a semaphore. The semaphore will only have the values 0 and 1 and act as the lock. The semaphore is our guard. Each thread will attempt to change the semaphore from 0 to 1. The lucky thread who succeed has effectively entered the lock, while remaining threads will forever attempt at assigning. To exit the lock we change the semaphore from 1 to 0. Of course we use the constants `Free` and `Locked` for the numerical values.

```
public class Sequence 
{
    ....
    const int Free = 0, Locked = 1;
    int semaphore = Free;
    
    public string Next()
    {
        while (Interlocked.Exchange(ref semaphore, Locked) == Locked)
        { }

        count = count == Max
            ? start
            : count + 1;

        var result = string.Format("{0:D10}", count).Substring(0, 10);

        if (Interlocked.Exchange(ref semaphore, Free) == Free)
            throw new Exception("Programmer error!");

        return result;
    }
}
```

The `Interlocked` Api reads a little strange at first sight. `Interlocked.Exchange(ref semaphore, Locked)` means setting the variable `semaphore` to the value `Locked` and return the previous value of `semaphore`. So as long as we set `semaphore` to `Locked` when it is already locked, we know that some other thread has acquired the lock. Similarly, when we free the lock (setting `semaphore` to `Free`) we expect that the value of `semaphore` is not already free. If it is, we throw an exception since something has gone completely wrong.


 
## 4. Performance measurements - wow lock is fast! 
I'm not really interested in a complete micro performance benchmark, only look at the overall trends. Thus we perform only a single test case of 800.000 numbers to be drawn pr. worker, and 13 workers in total at play. We resume the testing details in the next section.

The measurements are as follows on a quad core 3.5 Ghz machine

| Approach            | Time         
|---------------------|----------------:
| Thread unsafe        | 1.7 seconds    
| using `lock`         | 6.7 seconds    
| using `Interlocked`  | 6.5 seconds    


Conclusion. When workers perform very little work, **`lock` is as fast as a low-level busy wait!**

Just how they're achieving this speed I have not dug into. My gut tells me, that they choose to busy wait like I do, and then after a while, shift over to  a more "heavy weight" registration-notification mechanism.

We can *attemp*t at calculating the overhead of the `lock`. The overhead is around 5 seconds, compared to the thread unsafe version. This means we are doing 800.000 * 13 / 5 = *2 million* locks a second! Recall due to the work inside the `lock` the actual overhead of the lock is far less!

*In a soon to come article, I'll show how we can improve the performance to be close to that of the thread unsafe version.*



## 5. Testing multi-threaded code

It is fairly easy test our implementations to see that they are in fact thread-safe. 

First we need to define a worker. A worker represents each thread in our system. In our implementation all it does is to draw a sequence number and convert it to an int. We do this to simulate a bit of work, and because `int` has a significantly smaller memory footprint than `String`. I assume that the implementation is somewhat close to the workload of my production code.


```
public class Worker
{
    public List<int> NumbersDrawn = new List<int>();

    public void Execute(Sequence sequence, int iterations)
    {
        for (int i = 0; i < iterations; i++)
            NumbersDrawn.Add(int.Parse(sequence.Next()));
    }
}
```

We reuse this worker throughout all our testing. We'll create 13 workers and for each of them let them work in their separate thread.



```
public class SequenceTest
{
    int Max = 800000;

    [Test]
    public void TestNoDuplicatesInSequence()
    {
        var sequence = new Sequence();

        var workers = new[]
        {
            new Worker(),new Worker(),new Worker(),new Worker(),new Worker(),new Worker(),
			new Worker(),new Worker(),new Worker(),new Worker(),new Worker(),new Worker(),new Worker(),
        };
        
        var tasks = workers
            .Select(x => Task.Factory.StartNew(() => x.Execute(sequence, Max), TaskCreationOptions.LongRunning))
            .ToArray();
        Task.WaitAll(tasks);

        var distinctSerialsDrawn = workers
            .SelectMany(x => x.NumbersDrawn)
            .Distinct()
            .Count();

        Assert.Equals(N * workers.Length, distinctSerialsDrawn);
    }
}
```
    
The trick to testing this is to ensure that by merging all numbers from all workers, there must not be any duplicate entries. If each worker must draw 800.000 number and we have 13 workers, we know the list of all messages should be 800.000 * 13. If we have duplicates the `Distinct()` call removes these and the total messages is below the expected number.

    
## 6. Conclusion

For workers doing very little work, the `lock()` construct is amazingly fast. Very close to a low-level busy-wait strategy.

[In the next installment](StrategiesForFastThreadSafeCode.html), I'll show using other features of the `Interlocked` Api, that achieves speed close to the thread-unsafe implementation!



Please show your support by sharing and voting:
<SocialShareButtons>
</SocialShareButtons>



<br><br>
<CommentText>
</CommentText>

<br><br>

