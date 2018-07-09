draft
# Refactoring from monolith to micro services
*Author: Kasper B. Graversen*
<ArticleHeaderUrls/><Categories Tags="Micro_Service, Monolith, Refactoring, Architecture, Refactor_to_Micro_Services">
</Categories>

*Micro services promote agile development, easy change and rapid build and deployment. In this article, we go through some of the steps required for refactoring a monolith to micro services.*


Please show your support by sharing and voting: 
<SocialShareButtons>
</SocialShareButtons> 


<img src="img/httpspixabay.comdasommerfugl-insekt-oejne-sensor-376876.jpg">


Table of Content

   * [1. Introduction](#introduction)
 * [2. Risks](#risks)
   * [2.1 Infra-structural requirements](#infra-structural-requirements)
   * [2.2 Consistency and guarantees](#consistency-and-guarantees)
   * [2.3 Organization change](#organization-change)
   
   

## 1. Introduction

The many benefits to be reaped by using a *Micro service architecture* naturally make a lot of people are eager to jump on the wagon, and get micro servicin'. But how do you transcend from a monolithic architecture to a micro service oriented architecture? Before we can answer that, perhaps we should first define the term. 

	The Micro-Service architectural style is an approach to developing an application by means of a suite of services, each running in its own process and communicating with lightweight mechanisms, synchroniously and asynchroniously. Services are grouped around business domains or capabilities and are independently deployable and may be written using different technology stacks (programming language, frameworks and persistance storage).

	
At MVNO we have been on a journey towards micro services. Here I will try to capture some of the experiences we've gained.

# 2. Risks

The first thing to understand when endavouring upon the journey of micro service, is to understand the risks associated with micro services. 

## 2.1 Infra-structural requirements
In order to do micro services you need some kind of infra structure. You need a media through which you can send events, for example. You need a way to lookup services, do fail-over when that particular service instance is not responding, and you need a way to build and provision services. Perhaps you even want dynamic spinning up and degradation of your service with the current load.

You can either start using a cloud provider, or you can assemble a pipeline yourself. At MVNO we have chosen the later for a number of reasons. First, back when we started doing micro services, there were not really a cloud solution for C#. Secondly, for legal reasons it was, and still is, unclear whether the jura involved permit us to send data out of the house. We have come a long way by using external providers doing only a small thing, but doing that thing very well. 

  * We store our source code in GitHub, 
  * We build and test and upload to our private Nuget-feed with Appveyour, 
  * We provision the code onto our machines using Octopus. 
  * Features are provisioned to our test environment and our live environment directly from within our AgileZen scrum board using a custom made plugin.
  * Database changes are rolled out using round-house.
  * The errorlog is serviced by Sentry
  * Requests/responses are gathered in elasticsearch
  * And finally, our performance metrics and monitoring is done with the InfluxDb tool suite.


 ## 2.2 Consistency and guarantees
A lot has been said about eventual consistency. Some argue, that you never really have a truly life picture of your data. When you run a report that takes an hour to run, and present it to the big-wigs the same afternoon, things have moved on since 9am when you started the report generation. 
 
Perhaps that is a valid point, still to many, it is a lump to swallow when you during a business operation emit an event and cross your fingers some other service will pick that up and start getting other wheels and cogs in motion. Also some event brokers do not integrate with your database transaction leaving you vulnerable to data loss if your service crash just after committing the transaction and before sending the events. 
 
Several services may need to store system state in their own persistent storage engine, hence you'll have duplicated data, and data that may be inconsistent at times.


## 2.3 Organization change
Some organizations are more conservative than others. Particularly the ones handling large sums of money. I've worked with clients who only very reluctantly agreed to upgrading our software every 2 years. Other organizations see them selves as being very agile and do releases every six months. At MVNO we release several times.. a day.

Naturally, you are not reaping all the benefits when the organization is not ready for such change. Not improving the situation, many businesses forget to assess the cost of a feature for each day it is not yet released. The concept really is quite simple. If a business case make it feasible that the business can safe 1 milion in revenue by chan
On the other hand, if you want to follow the trends of your market, how can you live with 



� Small Change - Big Impact

Any change requires full
rebuild, test and deploy
?Impact analysis is huge effort
and takes long
?Obstacle for frequent
changes and deployments



No hard module boundaries
?Quality and Modularity breaks down over
time this enforces eventual need for re-write

?Long term commitment to technology stack

?Change or try-out new technology implies rewrite

?Re-write = complete re-write

?No partial re-write



Please show your support by sharing and voting: <SocialShareButtons>

</SocialShareButtons> 


<br><br>
<CommentText>
</CommentText>

<br><br>