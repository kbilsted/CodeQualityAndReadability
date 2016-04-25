# Optimal organization of database queries - a class per access
*Author: Kasper B. Graversen*
<ArticleHeaderUrls/>
<Categories Tags="Programming_with_databases,Database,SQL,OOP,Dependency_Injection,Design_Pattern,SOLID">
</Categories>

*From when we were toddlers, we have been warned of the terrible horror name 'global state' in our code. Global state is dangerous they said - like playing with fire, acid and gun powder at the same time. But turning to the database, global state is accepted unconditionally. A necessity rather than a danger. But shared state in the database is just as dangerous. This is why database access must be held in a tight leech. It must stand out in the code.*

Please show your support by sharing and voting:

<SocialShareButtons>
</SocialShareButtons>

<img src="img/bridge-truss-1031571_640.jpg" alt="from https://pixabay.com/en/brooklyn-bridge-suspension-bridge-1031571/"><br>
(*from https://pixabay.com/en/brooklyn-bridge-suspension-bridge-1031571/*)

Table of Content

   * [1. Introduction](#introduction)
   * [2. Implementation](#implementation)
   * [3. Consequences of usage](#consequences-of-usage)
     * [3.1 Explicate global state access](#explicate-global-state-access)
     * [3.2 Helps keeping the architecture clean](#helps-keeping-the-architecture-clean)
     * [3.3 Enhances readability](#enhances-readability)
   * [4. Code readability experiment](#code-readability-experiment)
     * [4.1 Readability experiment summary](#readability-experiment-summary)
   * [5. But won't I get swamped in classes??](#but-wont-i-get-swamped-in-classes??)
   * [6. What about the "repository pattern"?](#what-about-the-repository-pattern?)
   * [7. Summary](#summary)


## 1. Introduction

If you told me half a year ago, that I would come to love organizing each database access of an application as a separate class each containing a single method, I would have smacked you with a rubber fish! That is the way of MVNO - hitting people with rubber fish - and separating separating out  database access into separate classes.

A popular way of organizing code is to follow the Single Responsibility Principle ([SRP](https://blog.8thlight.com/uncle-bob/2014/05/08/SingleReponsibilityPrinciple.html)). The principle dictates, that a class should do only one thing. But do it well. The consequence of this organization is code that easier to understand and compose, and unit test.



## 2. Implementation
Glancing over some of the classes I've created recently at work for an integration project with a 3rd party system we find the following classes:  `SelectReadyItemsOfType`, `SelectCountAllReadyItems`, `UpdateItemRetry`, `UpdateItemSuccess`, `UpdateItemFail`. Each of these classes contain only a single method `Do` whose only responsibility is to communicate with the database. 

Basically all the implementation follows the implementation as given below.

```
public interface ISelectReadyItemsOfType
{
    Item[] Do(ITransaction tx, string itemType);
}

public class SelectReadyItemsOfType : ISelectReadyItemsOfType
{
    public Item[] Do(ITransaction tx, string itemType)
    {
        var sql = @"
            SELECT TOP 50 * 
            FROM Queue
            WHERE Type = @itemType
            AND Status = 'ready'";
            
        return tx.Connection.Execute(sql, new { itemType ) );
    }
}
```
As you can see the code is very functional in the sense that we are using a very generic name for the method of the class, and the class itself describe a verb (an action) rather than a noun (a thing). This is contrary to most OOP advice on naming classes and methods. And if you are a die-hard OOP-programmer, this style takes a little getting used to. I hope the rest of the article can convince you that there are sound reasons for this design.

The key design features of the code are:

  * The type describe the access we are performing against the database rather than the method name.
  * Each implementation is accompanied with an *interface* such that the database can be mocked in tests.
  * I'm using a micro ORM that take away the pain of writing plain ADO.Net, whilst *still write SQL*. 
  * Transaction management is *not* part of the responsibility of the code - this makes code more composable.

Perhaps the last bullet point is the most controversial, and thus will be the only point that we discuss in detail. 

Do we really need a transaction for reads? Yes. There are many reasons for explicit transaction handling also for database reads. 

  1. It is typically *faster* to create and commit a transaction for your read than not using an explicit transaction. Not specifying a transaction *still* entails using a transaction in the database. 
  2. The caller may choose to perform both reads and writes in the same transaction (there are cases where this is a good idea). 
  3. The caller chooses the isolation level of the query.
  4. By making transactions a part of your code, you can mock it and assert that a commit has or has not been issued.

  

## 3. Consequences of usage
Generally, this implementation pattern bring many advantages to the table. Lets have a look.


### 3.1 Explicate global state access
The most important advantage, perhaps, is that it makes access to the global state of your application (the database). Since the 1970's it has been advised to be cautious with global state in the code. Avoid the global state if possible, perhaps confining it to a subset of components, or pass it around where needed to make access explicit - and restricted. 

A similar movement can be seen with the whole micro services theme, which is motivated by breaking down the biggest global state of them all - the database. For an inspirational read take a look at [Application architectures with persistent storage](../design/ApplicationArchitecturesWithPersistentStorage.html). Even in a small implementation as a micro service, I advice to separate the database access.


### 3.2 Helps keeping the architecture clean
Business code and data access code *need* to be separated. You really need good reasons NOT to separate the two disparate domains. You won't find any literature telling you to keep the two together! And I'm not going to either.


### 3.3 Enhances readability
It is particularly beneficial when combining this implementation pattern with *dependency injection* and organizing object instantiation in *composition roots*. Recall, that [you can do dependency injection without IOC containers](../design/WhenIOCContainersBecomesAnAntiPattern.html). With this code style in place, I've noticed that I can read much of the code just by glossing. The class name combined with the dependencies taken in the constructor can reveal so much that you don't need to look through the code of the class. Let's experiment with this shall we. Keep an open mind. I'm going to show you a **non trivial excerpt from some real-world code we have running in production.** 


## 4. Code readability experiment
The following definitions are taken from real life production code. Let's see if we can make any sense of it. The code is handling the integration with an external 3rd party system. We will explain the classes in a top-down fashion. Essentially, the database access is the important aspect of any code. The motto for this code is *If it hasn't happened in the database, it did not happen!*. Thus we state **database changes in bold**.


```
class QueueItemProcessor 
{
    ILoggingGatewayClient mobileGateway;
    IResponseHandler responseHandler;
    IFatalErrorHandler sendToTaskSystem;
    IFatalErrorHandler splitRetry;
}
```

The queueitemprocessor is the implementation for handling items produced in out system that is to be transmitted to the external system. One item is handled at a time (the class name is singular). There is a "queue" somewhere in that name, because there are multiple simultaneous communication channels - and items are queued up in these. We can see that we are doing the communication since we depend on `ILoggingGatewayClient`. 

The external system may respond with either an "ok", "error" or "transient error" (eg. a timeout). Depending on the kind of message and the kind of error we encounter we have different strategies. For some errors we may choose to split the message (`IFatalErrorHandler splitRetry`). Otherwise we may flat out give up and send the failing message another system for manual handling (`IFatalErrorHandler sendToTaskSystem`). 

The fatal handlers are passed along as arguments when calling the `ResponseHandler` that will figure out the outcome of the communication and choose what to do. This is also why we did not have any dependencies on any update-to-success code here.

Then

```
class ResponseHandler 
{
    IEventProducer eventProducer;
    IUpdateQueueItemToComplete queueItemCompleted;
    IUpdateQueueItemRetry updateQueueItemRetry;
    IWriteOnlyCustomerLogClient writeOnlyCustomerLogClient;
    IFatalErrorHandler errorAction; 
}
```

The response handler can **`IUpdateQueueItemToComplete`** in case of an "OK", it can **`IUpdateQueueItemRetry`** in case of "transient error" and it can use a fatal handling `errorAction` in case of the error scenario. 


Then

```
class SendToTaskSystemAndUpdateRow : IFatalErrorHandler 
{
    IUpdateQueueItemToFailed updateQueueItemToFailed;
    IEventProducer eventProducer;
}
```

Sending an item to the task system entails sending an event using the micro service infrastructure (the `IEventProducer`), and updating the current item using **IUpdateQueueItemToFailed**.

Then

```
class SplitRetryAndUpdateRow : IFatalErrorHandler 
{
    IUpdateQueueItemToFailed updateQueueItemToFailed;
    IEventProducer eventProducer;
}
```

Splitting an item entails sending an event using the micro service infrastructure to the some one who asked us to transmit the message. Then they, in turn, will be splitting the message and creating new queue items. We also update the current item using **IUpdateQueueItemToFailed**.



### 4.1 Readability experiment summary
We are really speed-reading here! We just covered around 500 lines of code. I hope I have shown you that for obtaining a broad understand of the code, what exactly the code is doing is **unimportant**. By focusing on the global state we can do a high-level *"what goes in, what goes out"* analysis, and base our understanding on this. Essentially this analysis boils down to identifying the global state changes (the database and the event infra structure). 

The reason why we can with such confidence only gloss over the code is due to the separation of database access. And simply by following the type dependencies we know who is doing what - or who is delegating what to other entities.

If we had grouped several database accesses into the same class, we would need to go into much more detail of the code since we had to figure out what methods were invoked where.

Notice also, that we have not dug into the implementation of the database access code either. The names themselves are enough to reveal the intention as long as we know the implementation structure is as simple as we presented in section 2.

I hope this whirlwind tour of the code base was understandable. I hope that you feel confident when I say that breaking down code in smaller bits actually have an effect on the code that you may not otherwise anticipate when you only deal with 10-line hello world-like examples. 



## 5. But won't I get swamped in classes??
Perhaps the strongest force against this coding style is how uneasy it makes some people. And to be honest, this programming style is not particularly object-oriented. Yes, you *will* see growth in class numbers. And that may take time to getting used to. But remember, feelings concerning code is often rooted in habit. Once you get into the habit of a code style with more and smaller classes, you'll soon realize the benefits already described in section 4.



## 6. What about the "repository pattern"?
A well-known *object-oriented* alternative to this style of coding is the `repository` pattern. The repository pattern is characterized by abstracting away data access, typically modelling the database as an object collection much like one you would have in memory. Specifically different to is that *several* related database access methods  are grouped into one repository (a class). [http://deviq.com/repository-pattern/](http://deviq.com/repository-pattern/) explains in finer details some of the different implementation strategies of this pattern.

The repository pattern is beneficial in that it shields the application from the data storage. This make the code easier to test and easier to read. Unfortunately you will not reap the full benefits in terms of readability as we have just shown in section 4.



## 7. Summary
  * First and foremost, database access *must* be separated from business logic code. This is a well-known lesson most seasoned developers have learned by now. 
  * Next, *the database is one big nasty collection of global state* - hence access to it must be as explicit as possible to keep complexity under control.
  * We advise to completely separate one database queries from each other using types. 
  * We have shown the readability benefits of this implementation in a non-trivial real-life example 
  * We have compared our implementation to other common implementation patterns.


Please show your support by sharing and voting:
<SocialShareButtons>
</SocialShareButtons>


<br><br>
<CommentText>
</CommentText>

<br><br>
