# When IOC containers become an anti-pattern
*Author: Kasper B. Graversen*
<ArticleHeaderUrls/><Categories Tags="Architecture, Micro_Service, Refactor_to_Micro_Services, Dependency_Injection, IOC, IOC_Container, Service_Locator">
</Categories>


*For many years IOC containers have been touted the tool that ensure testable and reusable software. Any large enterprise project with self respect uses one. Interestingly, when refactoring to micro services, IOC frameworks play less of a role - To the extend, that their use may be regarded an anti-pattern.*


Please show your support by sharing and voting: 
<SocialShareButtons>
</SocialShareButtons> 



<img src="img/pixabay_com_greater-roadrunner-854405_640.jpg">

   
Table of Content

   * [1. Historical background](#historical-background)
   * [2. IOC containers as an anti-pattern](#ioc-containers-as-an-anti-pattern)
   * [Conclusion](#conclusion)
    
   
## 1. Historical background
First at bit of history to set the scene. Around 2005-2006 people started writing about dependency injection in MSDN Magazine and Dr. Dobbs. With the popularization of dependency injection techniques, the ecosystem of IOC container frameworks proliferated, giving further momentum to the movement. Inevitably, IOC containers became modern. There was excitement. Finally, we had the technique, and tool support, for creating testable code. Heck, I was excited too. Enterprise software, unit testing, maybe even a bit of TDD - combined!

In any modern larger software project with self respect, you will find the use of an IOC framework. If you're not using IOC containers, you'll hear experienced programmers complain your ears off about it. Nothing wrong with that. Their benefits generally out-weigh the introduced complexity. At least so long as they are not used as service locators. Personally. I've done magic with IOC containers! Test-enabling a behemoth of an application framework that otherwise was not impenetrable to testing. But more on that another time.

Before we continue, it is important to the terminology straight. There are two closely related concepts that are easily mixed up on the Internet. These are;

* **Dependency injection** is a programming practice of passing into an object it’s collaborators, rather the object itself creating them. For example, "constructor injection" is the technique of taking all "static dependencies" as constructor arguments.
* An **IOC Container/Dependency injection frameworks** is a framework, that can create instances of object graphs and their dependencies. Typically through some explicit configuration or implicitly by scanning assemblies combined with conventions of naming or use of attributes. 

So you can do dependency injection perfectly fine without an IOC container if you so choose.


## 2. IOC containers as an anti-pattern
Part of my job at MVNO, is to tease apart functionality of a large monolith, and placing it in independent micro services. Independent of each other, we made an interesting observation. 

> The first thing we do after moving the code to the micro service, <br>
> almost as a knee-jerk reflex, <br>
> is to remove the dependency to the IOC framework. 

It's not that we don't like IOC frameworks, they're great! But in the context of micro services, IOC containers often do not have a role to play. Part of what defines a micro service, is that it is small. Hence the number of composition roots are limited. It may therefore be just as easy to instantiate the object graph by hand. In a micro service implementation, using an IOC framework may be regarded an anti-pattern - if you have a need for an IOC framework, it may be an indicator, that your service is too big.

Cutting the ties to the IOC container dependency yields the following advantages

* No registering of interfaces to types.
* No life-time management.
* One less dependency to manage when building and deploying.

Make no mistake. We do a whole lot of dependency injection - we just don't employ an IOC framework.


## Conclusion
In terms of testable code, it is the practice of using dependency injection that enables testability, not the IOC framework. 

In a micro service implementation, using an IOC framework may be regarded an anti-pattern - if you have a need for an IOC framework, it may be an indicator, that your service is too big.
 


Please show your support by sharing and voting: <SocialShareButtons>
</SocialShareButtons> 


<br><br>
<CommentText>
</CommentText>

<br><br>