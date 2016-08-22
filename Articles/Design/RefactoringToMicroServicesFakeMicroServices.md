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

When reading the Internet, you will find advice in abundance. But mostly in the form of "Identify seams of you system and go separate", or "if your team is about to implement feature Y, consider implementing that as a service instead". In practice, however, things are not as simple. *You quickly find yourself in a catch-22. You want to write a new service independent of the monolith, yet the service must depend on a lot of functionality found within the monolith.*

There a several ways to deal with this situation

  1. You can split the monolith into services and only depends on loosely coupled services.
  2. You can duplicate reams of code from the monolith to the new service.
  3. We can create a number of fake service which the new service on.

Let's deal with each of the three possibilities. Clearly **(1)** is a bit of a recursive joke. We are in the midst of extracting the monolith into services, so the answer to our problems, can hardly be to just do more of that. 

While **(2)** is a viable option, it carries with it a lot of overhead. With code duplication, code changes require manually searching all other code repositories to ensure alignment of the new logic. At MVNO some scornfully sneered at the warning signs and embraced code duplication. But when code changed, we often forgot to re-implement the changes in all the duplicated places. While a part of the microservices paradigm is to trade DRY for flexibility, code duplication is now a path we thread more cautiously. 

**(3)** is the focus of this article. It is what has worked best for us.



## 2. Fake it 'till you make it

When you start off with building your first services, you quickly find yourself missing out on a lot of the monolithic infrastructure. To a large extend we have dealt with this issue in MVNO by means of **fake microservices**. What is a fake service? It's sort of the opposite of a microservice. 

> A *fake microservice* is a unit of software that highly depend on functionality of the monolith. 
> It cannot be independently upgraded and it shares data with the monolith and other fake services.

A fake microservice is like a wart on your girlfriends nose. Its ugly and its not intended to stay there for long. Still, while its there, it serves the practical purpose of scarring away other male courters. Regard fake services as stepping stones, helping you to reaching you goals faster. Separating out and building a service is far more time consuming that building a fake service.  A fake microservice may live within the code repository and the same execution environment as the monolith, or it may be separated out. 

What are the kinds of services you want to fake? At first, it is the infrastructure needed in your soon-to-be-written-services. Later, you may find that obstacles for taking apart the monolithic, can be solved by breaking a part of the logic into one or more fake services. 


  
## 3. Real-life scenarios

The success and speed with which you can transition from monolithic code -> to fake microservice -> to real service, highly depend on the code base and code extracted. I'll show case two pieces of important infrastructure whose success in this regard are very different.


### 3.1 Example 1. The customer log
Many enterprise systems have the notion of a log of business events. What has happened to the customer, and when. At MVNO we term such a log a "Customer log", and it holds all important information for the administrative staff to understand the customer. When did they pay their bill, late payment thresholds crossed, changes in the subscription, the status of important third party integration calls. And it also holds an entry for each time a staff member lookup the customer. If you don't have a customer log in your system, consider getting one. 

Needles to say, we can't do much in a service without the need of writing to the customer log. 

Now in our monolith the customer log is a database table, and the last thing we want to do is to have our micro serves write directly into tables of the monolith. It defies the whole purpose of the microservice architecture. 

Here is what we did:

  * Left the database untouched
  * Created a fake microservice that listens on a new event `LogCustomerLog` taking as payload the message, the customer id, the administrative user id, etc.
  * Done ...

You can well imagine the speed with which such a service could be established. It makes our services decoupled from the problem of "customer logging", and we can at any time upgrade our fake microservice *without having to do any changes to our microservices*. Of course, now we have two ways of writing to the customer log. Sending events and writing directly into the table. We address this in section 4.


### 3.2 Example 2. Sending emails
Many services needs to inform customers on important actions they've taken. A welcome letter, a change of subscription, a payment running late etc. Very similar to the customer logging, we can introduce a fake service and an event `SendEmail`. 

The way email and phone texts are generated is by means of a very generic templating system. Each message is defined in a separate part of the application. Messages may use tags which at the rendering time replaces e.g. `[firstname]` with the customer's name. A message may be associated defines as any of our message types in our system, e.g. "welcome letter" or "change of phone number". Each of message types support a set of tags.


### 3.3 Discussion

From a conceptual perspective, the two services are "identical": They receive an event and generate a "message" in the form of an email or an entry in a customer log. From a data perspective, however, they are very different. The customer log messages are entirely defined by the payload of the event, not so for emails. Recall, emails may contain tags. And a tag corresponds to data virtually from any corner of the monolith. 

Thus, while it is fairly straight forward to upgrade the fake customerlog service, upgrading the email service may either require a lot of data pumps to feed the service with all kinds of information, or alternatively, a number of services must exhibit the data needed for the email generation. Reading between the lines: The email service is serving the purpose of decoupling just fine, and we'd rather focus our energy elsewhere.



## 4. From fake to fabulous

Getting rid of a fake service is a two-step process. 

First we must change the monolith into using events rather than writing directly to the database. For this we leverage upon the [event queue design pattern](RefactoringToMicroServicesTheEventQueue.html) to ensure our event publishing is aligned with our database transactions. **Conceptually, this is an interesting step: We are reducing the coupling of the code without separating code into services**. 

Second, we must upgrade our fake service to a real service. We do this by cutting ties to our dependencies in the monolith - perhaps by moving the code to the service. This is possible now that the monolith is publishing events rather than calling said code. Also, we need to migrate data either to some other schema or another database entirely.

    
  
## 5. Conclusions

By using fake micro service, we can quickly make the platform understand events (or other means of requests). This makes it fast to expose infrastructure needed in the microservices we want to write. This has several advantages

  * We save time making all infrastructure code services, as this require changing the whole monolith, the database etc.
  * We do without code duplication.
  * We can steadily migrate the monolith code to using the fake services.
  * Finally, we can turn the fake service into a real service when all dependencies have been resolved.

Each of these steps are incremental, and low-risk.
 



For more articles in this series, see <Categories Tags="Refactor_to_Micro_Services">
</Categories>

Please show your support by sharing and voting: <SocialShareButtons>

</SocialShareButtons>

**August 2016 NOTICE: MVNO IS HIRING** *MVNO at Telenor is looking for talented developers.  MVNO is the infrastructure that keep mobile companies running. Our clients include CBB and OK Mobil.
Do you want to expand or further grow your skills within the realm of Micro services, IOC containers, Kafka, C#, F#, Javascript, Angular, ASP.NET MVC, Command-Query-Separation, DBA skills, Git, Github, Appveyor, Octopus, look no further. 
<br>MVNO offers a unique culture where a big part of the working life if about optimizing for happiness. Your happiness! We have 20% of our time allocated for innovation or fixing stuff that bothers us. Great work place with 37h work week, six weeks vacation and highly skilled friendly colleagues.
<br> If you are interested please contact mokc@telenor.dk for further details. The office is situated at Telenor Copenhagen.*


<br><br>
<CommentText>
</CommentText>

<br><br>
