# Code Quality & Readability
By Kasper B. Graversen  
[[All articles]](AllArticles.html)  [[All tags]](AllTags.html)   
<AllTags />

*A site providing information on achieving super readable and maintainable code.*



Newest articles

* [From imperative to declarative code using LINQ extension methods](Articles/Readability/FromImperativeTtoDeclarativeCodeUsingLINQ.html) <img src="img/new.gif">
* [Restrict expressibility when iterating](Articles/Readability/RestrictExpressibilityWhenIterating.html) <img src="img/new.gif">
* [Productivity features that may ruin code readability](Articles/Readability/ProductivityFeaturesThatMayRuinCodeReadability.html)
* [Optimal indent size for code readability](Articles/Readability/OptimalIndentSizeForCodeReadability.html)

  
  
## Introduction

> BEWARE FOR INSIGNIFICANCE AND NOTHINGNESS CAN BE MOST POWERFULL FORCES.

*You can tell a lot about the clientele of an establishment by the garbage it leaves behind. Look at this! Putrid sprawling code promiscuously sharing state. Singletons and statics generously sprinkled on the code base. Classes spanning thousands and thousands of lines of code. Ever changing formatting of the code. Time pressure and tight budgets choking every attempt at setting up a build server or automating deployment. This is an intergalactic cesspool. A natural habitat for developers who thrive on stress, overtime and ever delayed deliveries. For the misfortunate, the depraved and degenerated. Those so technically inept, they have no where else to go. Developers in a state between living and dead -- Zombies!*

<img src="img/v1.std3.ru_50_d1_1436465041-50d172534a9c22422ceeb9bbfdf21041.jpeg" width="60%" alt="Logo from https://v1.std3.ru/50/d1/1436465041-50d172534a9c22422ceeb9bbfdf21041.jpeg" align="right">
*Oh look at the monstrosity they have created. It sweats, snorts and groan as it moves forward at a glacial pace. Contorted in shape, like tumours budding from every angle. Oh, I want to feel every part of you. Torment my soul by intimately learning your every detail.*

*AAAhh! An abruptly awakening from another nightmare. Awake, but only enough to realize, that it is time to rise and go to work. To let the hellish nightmare continue in broad daylight.*

Enough introduction to the flames of damnation that awaits those who have to maintain bad code. I presume that you are reading this out of an interest in creating code worth reading and maintaining. By yourself and others. Out of an interest in making the world a better place. At least the virtual world of code we developers spend a great deal of out lives in.

On to the formalities. *Code readability* is the human judgement of how easy code is to understand. The readability of a
program directly impact its maintainability cost, and is thus a key factor in overall software quality. The price of the maintenance cost, depending on your source, will range between 40-80% of the total life cycle cost. But long before the maintenance phases, readability impact your daily work. Code is often visited during development to understand how to properly use it, when scouting for appropriate hooks for new features or determining the design strategies for a particular module to follow upon implementing new or changing business requirements.

But it is not all about numbers. It is about creating a professional working environment, following best practises of the craft. About being happy developers who wants to attend work, who pride and joy from the code they produce and maintain.

**The goal of this site is to improve your programming skills by improving the code you produce**. We will touch upon many facets that constitute the process of programming. And by reflecting over the material presented you will be able to put yourself in the shoes of a future maintenance developer (maybe yourself) and through such analysis make thought through decisions. Let us term such a feeling *empathy*.

Empathy in itself, however, doesn't get us very far. You need to grow passion for your work, for the code you produce and the code others produce (because you have to read it). Your conduct must reek "professionalism". Simply put, "you need to care". This may in periods of time entail extra work on your behalf. You need to experiment and continuously evaluate how code impacts readability.


<AllTags />
<br>
<br>
<br>
<CommentText>
</CommentText>
