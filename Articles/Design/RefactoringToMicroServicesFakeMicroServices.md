# The importance of fake microservices
*Author: Kasper B. Graversen*
<ArticleHeaderUrls/><Categories Tags="Micro_Service, Monolith, Refactoring, Architecture, Refactor_to_Micro_Services">
</Categories>

*We introduce the notion of a fake microservice. Fake microservices may be instrumental in your journey from a monolithic architecture to microservices. In this article we  reveal how we got a kick-start towards a microservice architecture by means of fake microservices.*



Please show your support by sharing and voting:
<SocialShareButtons>
</SocialShareButtons>


<img src="img/https_pixabay_com_tableware-1571068_640.jpg">
<br>Image from [https://pixabay.com/en/tableware-plate-porcelain-stack-1571068/](https://pixabay.com/en/tableware-plate-porcelain-stack-1571068/)


Table of Content

   * [1. Problem](#problem)
   * [2. Fake it 'till you make it](#fake-it-till-you-make-it)
   * [3. Real-life scenarios](#real-life-scenarios)
     * [3.1 Example 1. The customer log](#example-1-the-customer-log)
     * [3.2 Example 2. Sending emails](#example-2-sending-emails)
     * [3.3 Discussion](#discussion)
   * [4. From fake to fabulous](#from-fake-to-fabulous)
   * [5. Conclusions](#conclusions)


   

## 1. Problem

Having taken the decision to depart from a monolith architecture, one of the first questions to arise is "how to get there?" *Fake microservices may be the answer you are looking for*. In order to understand how to get from a monolith to microservices, we must understand what a microservice is. For this article the following simple definition applies.

> A microservice is unit of software that is independently upgradeable and replaceable.
> It autonomously deals with a subset of the business and is full control of its data. 

When reading on the Internet, you will find advice in abundance. Most of this advice is in the form of "identify seams of your system and separate there" or "if your team is about to implement feature Y, consider implementing that as a service instead". In practice, however, things are not always that simple. *You quickly find yourself in a catch-22. You want to write a new service independent of the monolith, yet the service must depend on a lot of functionality found within the monolith.*

There a several ways to deal with this situation:

  1. You can split the monolith into services and only depend on loosely coupled services.
  2. You can duplicate reams of code from the monolith to the new service so it is independent.
  3. You can create a number of fake services which the new service depends on.

Let's deal with each of the three possibilities. Clearly **(1)** is a bit of a recursive joke. We are in the midst of extracting the monolith into services, so the answer to our problems can hardly be to just do more of that. 

While **(2)** is a viable option, it carries with it a lot of overhead. With code duplication, code changes require manually searching all other code repositories to ensure alignment of the new logic. At work some scornfully sneered at the warning signs and embraced code duplication. But when code changed, we often forgot to re-implement the changes in all the duplicated places. While a part of the microservices paradigm is to trade DRY for flexibility, code duplication is now a path we thread more cautiously. 

**(3)** is the focus of this article. It is what has worked best for us.



## 2. Fake it 'till you make it

When you start off with building your first services, you quickly find yourself missing out on a lot of the monolithic infrastructure. To a large extend we have dealt with this issue in MVNO by means of **fake microservices**. What is a fake service? It's sort of the opposite of a microservice. 

> A *fake microservice* is a unit of software that highly depends on functionality of the monolith. 
> It cannot be independently upgraded and it shares data with the monolith and other fake services.

A fake microservice is like a wart on your girlfriend's nose. It's ugly and is not intended to stay there for long. Still, while there, it serves the practical purpose of scaring away other romantic interest. Regard fake services as stepping stones, helping you to reaching you goals faster. Separating out and building a service is far more time consuming that building a fake service.  A fake microservice may live within the code repository and the same execution environment as the monolith or it may be separated out. 

What are the kinds of services you want to fake? At first, it is the infrastructure needed in your soon-to-be-written-services. Later you may find that obstacles for taking apart the monolithic can be solved by breaking a part of the logic into one or more fake services. 


  
## 3. Real-life scenarios

The success and speed with which you can transition from monolithic code to fake microservices to real services highly depends on the code base and the code extracted from the monolith. I'll showcase two pieces of important infrastructure whose success in this regard are very different.


### 3.1 Example 1. The customer log
Many enterprise systems have the notion of a log of business events, essentially what has happened to the customer and when. At our company we term such a log a "Customer log", and it holds all of the important information for the administrative staff to understand the customer (when did they pay their bill, late payment thresholds crossed, changes in the subscription, the status of important third party integration calls, etc.). It also holds an entry for each time a staff member performed a lookup the customer. If you don't have a customer log in your system, consider getting one. 

Needless to say, we can't do much in a service without writing to the customer log. 

Now in our monolith the customer log is a database table and the last thing we want to do is to have our microservices write directly into tables of the monolith. It defeats the whole purpose of the microservice architecture. 

Here is what we did:

  * Left the database untouched
  * Created a fake microservice that listens on a new event `LogCustomerLog` taking as payload the message, the customer id, the administrative user id, etc.
  * The fake microservice saves the received data into customer log entries corresponding to each event instance.
  * Done ...

You can well imagine the speed with which such a service could be established. It makes our services decoupled from the problem of "customer logging", and we can at any time upgrade our fake microservice *without having to do any changes to our microservices*. Of course, now we have two ways of writing to the customer log. Sending events and writing directly into the table. We address this in section 4.


### 3.2 Example 2. Sending emails

Many services need to send emails such as a welcome letter, a change of subscription, notice that a payment running late, or indicators of other activity. Very similar to the customer logging, we can introduce a fake service and an event `SendEmail`. 

The way email and phone texts are generated is by means of a very generic templating system. Each message is defined in a separate part of the application. Messages may use tags which at the rendering time replaces e.g. `[firstname]` with the customer's name. A message may be associated defines as any of our message types in our system, e.g. "welcome letter" or "change of phone number". Each of message types support a set of tags.


### 3.3 Discussion

From a conceptual perspective, the two services are identical: They receive an event and generate a message in the form of an email or an entry in a customer log. From a data perspective, however, they are very different. The customer log messages are entirely defined by the payload of the event, not so for emails. Recall, emails may contain tags. And a tag corresponds to data virtually from any corner of the monolith. 

Thus, while it is fairly straight forward to upgrade the fake customer log service, upgrading the email service may either require a lot of data pumps to feed the service with all kinds of information, or alternatively, a number of services must exhibit the data needed for the email generation. Reading between the lines: The email service is serving the purpose of decoupling just fine, and we'd rather focus our energy elsewhere.



## 4. From fake to fabulous

Getting rid of a fake service is a two-step process. 

First we must change the monolith so it uses events rather than writing directly to the database. For this we leverage upon the [event queue design pattern](RefactoringToMicroServicesTheEventQueue.html) to ensure our event publishing is aligned with our database transactions. **Conceptually, this is an interesting step: We are reducing the coupling of the code without separating code into services**. 

Second, we must upgrade our fake service to a real service. We do this by cutting ties to our dependencies in the monolith - perhaps by moving the code to the service. This is possible now that the monolith is publishing events rather than calling said code. Also, we need to migrate data either to some other schema or another database entirely.

    
  
## 5. Conclusions

By using fake microservices, we can quickly make the platform understand events (or other means of requests). This makes it easier and quicker to expose infrastructure needed in the microservices we want to write. This has several advantages:

  * We save time turning all of the infrastructure code into services, as this requires changing the whole monolith, the database etc.
  * We do it without code duplication.
  * We can steadily migrate the monolith code to using the fake services.
  * Finally, we can turn the fake service into a real service when all dependencies have been resolved.

Each of these steps are incremental, and low-risk.
 



For more articles in this series, see <Categories Tags="Refactor_to_Micro_Services">
</Categories>

Please show your support by sharing and voting: <SocialShareButtons>

</SocialShareButtons>


<br><br>
<CommentText>
</CommentText>

<br><br>
