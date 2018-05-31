# The event queue design pattern
*Author: Kasper B. Graversen*
<ArticleHeaderUrls/><Categories Tags="Micro_Service, Monolith,
Refactoring, Architecture, Refactor_to_Micro_Services, Design_Pattern">
</Categories>

*The event queue is an important design pattern to help prying apart a monolith application. It delays the sending of events to a point in the execution flow where it is safe to publish them.*


Please show your support by sharing and voting:
<SocialShareButtons>
</SocialShareButtons>


<img src="img/https_pixabay.comen_blackberries-bramble-berries-bush-1539540blackberries-1539540_640.jpg">
<br>Image from [https://pixabay.com/en/blackberries-bramble-berries-bush-1539540/](https://pixabay.com/en/blackberries-bramble-berries-bush-1539540/)


Table of Content

   * [1. Context](#context)
   * [2. Problem: When to publish events](#problem-when-to-publish-events)
   * [3. Solution: The event queue](#solution-the-event-queue)
     * [3.1 Implementation](#implementation)
     * [3.2 Alternative implementation](#alternative-implementation)
   * [4. Discussion](#discussion)
     * [4.1 The event queue won't solve everything](#the-event-queue-wont-solve-everything)
     * [4.2 Don't put business logic in the queue](#dont-put-business-logic-in-the-queue)
     * [4.3 Avoid vendor lock-in](#avoid-vendor-lock-in)
     * [4.4 Transactional event publishing](#transactional-event-publishing)
     * [4.5 Roll-out strategies](#roll-out-strategies)
   * [5. Summary](#summary)


## 1. Context
The many benefits to be reaped by using a *Micro service architecture* naturally make a lot of people are eager to jump on the wagon, and get micro servicin'. But how do you transcend from a monolithic architecture to a micro service oriented architecture?

Before we can start extracting code into a service (or write new code as a service) we need a mechanism through which we can publish events from within our monolith. One pattern we have used at our company in our transitioning is the event queue pattern.




## 2. Problem: When to publish events

During the execution of business logic, we need to substitute method calls with publishing events (in order to initiate other business processes). However, *If an error occur later in the same execution flow, we do not want to publish the events*. Much like using a database with the ability to roll back a transaction upon error.

So miles deep into the execution, how can we determine whether or not to publish an event then? How do we know whether the rest of the code in the flow will succeed? It is close to impossible in the general case. So let's find alternatives.


## 3. Solution: The event queue

The concept is simple. Throughout the execution of our business code, we maintain a queue of events to be published. Upon the end of the execution flow, when we persist state, we publish all events in the queue.

If during the execution of the business logic an error occur, all we have to do is throw away the queue instance and we are done. There is no need for a roll-back of the events. Finally, we must ensure at each composition root (entry of the system), we instantiate a fresh instance of the queue.

Essentially, this is the [unit of work](http://martinfowler.com/eaaCatalog/unitOfWork.html) pattern presented in "patterns of enterprise application architecture"  (Fowler;2003). We just call it an even queue instead.  


### 3.1 Implementation

The implementation of the queue is fairly simple. It is a list with a wrapper exposing an `Add()` and a `PublishEvents()`. The later is used at the end of the execution flow. Notice the queue is "insert only", you can't query its content. Why that is important is elaborated in section 4.2

Now for the implementation:

```
class EventQueue
{
    private readonly List<Event> Events = new List<Event>();
    private IEventSender sender;

    public EventQueue(IEventSender sender)
    {
        this.sender = sender;
    }

    public void Add(Event e)
    {
        events.Add(e);
    }

    public PublishEvents()
    {
        sender.SendEvents(events):
    }
}
```

Notice that there is no interface `IEventQueue`. I'm not convinced we need one. There is no need for mocking out the implementation, you can freely use the real implementation in your tests. Instead during testing you need to mock out the event sender.

If you are using an ORM, you may be able to hook into it and automatically call `PublishEvents()` on successful transactions. We do that with NHibernate at our company, using the `RegisterSynchronization(AfterTransactionCompletes)` hook.


### 3.2 Alternative implementation

Using the event queue means that you need to pass it around to every method call or constructor. If you think such a roll-out is going to be too laborious, you can consider using an alternative implementation. Using a static implementation all the code can reach it without an object is passed around. This implementation holds a queue either pr. thread/session (eg. Http, ORM or whatever you can grasp onto).

For this article I store queues pr. thread. It is far from the best "session" to hook into, but I want the example code to be as technology-agnostic possible. I suggest you use this code for inspirational purposes only. Some of the problems with this implementation is that a piece of code can be executed by a number of threads - e.g. when using parallel-LINQ or `async`-`await`. You must step carefully, for no warnings are given if (perhaps unknowingly) use multiple threads.

```
static class StaticEventQueue
{
    private static ConcurrentDictionary<Thread, List<Event>> queues =
        new ConcurrentDictionary<Thread, List<Event>>();

    public static IEventSender sender = new MegaCorpEventsSender();

    public static void Add(Event e)
    {
        queues.AddOrUpdate(
            Thread.CurrentThread,
            new List<Event> { e },
            (key, l) => { l.Add(e); return old;}
        );
    }

    public static void PublishEvents()
    {
        List<Event> events;
        queues.TryRemove(Thread.CurrentThread, out events);

        sender.SendEvents(events);
    }

    public static void AbortEvents()
    {
        List<Event> events;
        queues.TryRemove(Thread.CurrentThread, out events);
    }
}
```

Notice that this implementation needs an explicit abort method for the case that an error has occurred. Needles to say, it is dangerous to forget to call this. If you are hooking into an ORM, you may be called whenever a transaction ends, and given information on whether or not the transaction succeeded.

Alternatively, as discussed in 4.4 you can store events in the db using the same transaction, such that when it rolls back, so does your event queue items.



## 4. Discussion

I think there are a number of issues worth discussing.

### 4.1 The event queue won't solve everything
The event queue enables your monolithic code to publish events at safe points in the execution flow. And that's a good starting point. However, it is important to understand the implications of substituting a method call with sending an event.

First, a method call is a synchronous action. Sending an event which is then processed by some receiver(s) is an asynchronous action. Hence there won't be a "return value" to use in the rest of the execution flow. This will of course limit its use since your entire code base most likely has been written around a "synchronous" design. Hence many places you need to rewrite in order to utilize the queue.

Secondly, and in a similar vein, substituting method calls with event sending effectively means breaking a transaction into many transactions: A transaction where you are sending the event, and a transaction for each of the receivers of the event. A lot of code assume it is executed within the same transaction (think data consistency) cannot be teased apart solely with the use of event queue. It requires restructuring such as changing the ordering of instructions. More on this in a later article.

In essence, you will need to restructure your code whether you use this pattern or not.


### 4.2 Don't put business logic in the queue
The event queue is essentially a global state in the application. Behold the poor souls who travel the path of global state, for it is truly the path of your demise. We can mitigate the problems by promising to be hands of doing anything but inserting into the queue. Treat it like your boss' big chested wife at the cocktail party  who got a little too drunk and a little too flirtatious - don't look, don't touch.

There **will** come a time where you find yourself wanting to implement logic in the event queue. For example logic that removes event *A* when event *B* is added, or add event *C* automatically when event *A* is added. It is equally bad practice to query the content of the queue and react to that. Try to solve such problems by other means.

Here is why its dangerous. The second you start extending the queue or query its content, you are putting business logic where it doesn't belong. One important lesson learned by the SOA community, is to keep business rules outside the enterprise service bus. In other words, keep the middle ware stupid!


### 4.3 Avoid vendor lock-in
An alternative to using the event queue is to use the Api of your event-infrastructure supplier. Be aware, this a serious buy-in on that particular platform. Using the event queue shields you a bit from vendor lock-in - also sometimes referred to as *an anti-corruption layer* or a *channel adapter*.


### 4.4 Transactional event publishing
If your event-infrastructure does not support publishing events within a DB transaction, you may consider writing the content of the event-queue into a db-table instead. This way, its "business as usual" and your events will follow the same path all your other data changes follow.

Then have a separate worker (thread/service) whose only job is to read from the events DB table, publish the events and delete the rows from DB.


### 4.5 Roll-out strategies
When rolling out this pattern, prepare yourself for touching a large part of the code base. Literally all constructors needs to take the queue as an argument. Alternatively, just roll out the queue enough places to cover the business logic needing to publish events.

You also need to ensure that every composition root (entry point of your application) gets to instantiate a fresh copy and and the end of execution flows, you call `PublishEvents()`.

I suggest passing around the queue only and get that merged onto the master branch before using it. That way, the majority of changes are simple risk-free. Still for larger projects, the roll out may cause some initial pain.


## 5. Summary

  * Delay publishing event until the end of the code flow
  * The event queue pattern help the transition
  * Inevitably going micro services require non-trivial changes to your code

For more articles in this series, see <Categories Tags="Refactor_to_Micro_Services">
</Categories>

Please show your support by sharing and voting: <SocialShareButtons>

</SocialShareButtons>

<br><br>
<CommentText>
</CommentText>

<br><br>