draft
# The good, the bad and other aspects of refactoring practices


## 1. Introduction

*Refactoring*, formalized and popularized by Martin Fowlers book *"Refactoring, Improving the Design of Existing Code"*. According to Fowler, "refactoring" is a *"disciplined technique for restructuring an existing body of code, altering its internal structure without changing its external behavior with the intention of making it easier to understand and cheaper to modify"*. 

A longish definition, which perhaps is the reason people tend to forget parts of it. Most catch the "changing existing code". Fewer remember the "without changing its external behaviour". Few recall the motivation "with the intention of making it easier to understand and cheaper to modify".

Fowlers book was published in 1999 and to new programmers I recommend skimming through it. While most of the patterns presented are long since automated in modern IDE's, it is eye-opening the first time you read about changing structures of a programs on a higher level of abstraction.  Due to the automation, we perform tens of refactorings every day, for example, renaming a method has become second nature. You simply click "Rename..." and give it no further though. But there is a fine line between "refactoring" and "restructuring" of code. Deceptively, both are carried out using the IDE's inbuilt "refactoring facilities":


> The term "refactoring" is often used when it's not appropriate. 
> If somebody talks about a system being broken for a couple of days while 
> they are refactoring, you can be pretty sure they are not refactoring. 
> If someone talks about refactoring a document, then that's not refactoring. 
> Both of these are restructuring.
> Martin Fowler, 2004 


So in many organizations, we sometimes do restructuring and calling it "refactoring". Perhaps, Martin is trying to forego critique that refactoring is problematic, by stating that those problematic situations in fact arise from restructuring.  


So a lot of the time when we use the word "refactoring" we really are meaning "restructuring". That is important. 


## Refactor as a code clean up. 

For example, during the development of
some feature you encounter serious deficiencies in the code you are
about to change or extend, or maybe some of the surrounding code.
Rather than fixing these issues, report them to your backlog, and then
go about making your planned change. The advantage to this approach is
that the planned change is as small as possible, thus ensuring fewer
places to introduce bugs, and fewer places the test department needs
to test. Everyone is happy. The developer has a change to finish
within the estimate and changes are, to the extend possible, planned.
There is the risk that the backlog grows due to lack of hours (and
commitment from management). This is a real risk. When the backlog
grows out of control, developers will not care to report outstanding
refactorings and the code starts to rot. Developers get increasingly
annoyed about the code and quality is doomed. Of course there are the
odd occasion where a refactoring is either done before the change, or
making the change will take unjustifiable long time to make, or that
the refactoring will become much more cumbersome. In those cases, you
should probably favor refactoring straight away.



## Refactoring paving the way for change

I have seen some ugly code in
my time. I say ugly rather than complex, since the code after a bit of
refactoring wasn't that difficult to manage. I was once tasted with
separating a calculate method into two separate methods since
depending on its parameters, it would actually carry out two different
sets of calculations. A fairly simple change only involving moving
code around. Then imagine the method consisting of a switch statement
spanning more than 5000 lines of code, and for each __case__ you would
have hundres of lines of code in a nest of nested __if__'s. Cutting
and pasting from this code into a new class was near impossble. There
were so many scope brackets, scopes were several screens heigh. The
risk of overlooking a bracket was high and there we little support
within the IDE at that time. After iterations of performing simple
refactorings the code became quite mallable. First the method was
moved into a separate class. Then each case into its own method. Then
major branching of each method was split into methods. at this point
scopes 


## Refactor only when you understand

``` 
It's Not Refactoring, It's Untangling
By Cody on March 18, 2013 12:26 AM | No TrackBacks

Every codebase has at least one component that is widely hated and feared.  It does too much, it has too many states, too many other entities call it.  When it comes time to pay down technical debt, you should definitely focus on this component.  However, if you have an incomplete understanding of this component and you stop everything to completely rewrite it, your odds of success are low.  That component, as scary and complex as it appears, is actually way more scary and complex than you think. 

How do you think that component got into this unfortunate shape? Is it because the company hired a nincompoop and let him run wild in the code base for years?  Or is it because the component was originally a sound abstraction, but its scope of responsibilities had grown over the years due to changing requirements?  (For the sake of my ego, I'm hoping the Career Killer is the latter.)  In all likelihood, this component arrived at its current, scary state via smart people with good intentions.  You know what you are right now?  Smart people with good intentions.  If you proceed with a big refactor, you'll trade one form of technical debt for another.

In order to truly pay this debt down, you need to untangle the complexity around the problem.  You need to spend time looking at all the clients calling this component.  You need to spend time talking with your colleagues, learning more about the component's history and how it's used.  You need to make a few simplifying changes around the periphery of the component and see what works.  Each week, you spend a little more time and untangle the problem just a little bit more.  Given a long enough timeframe, you'll eventually untangle all of the complexity and brought a teeny bit of order to the universe.

Practically speaking, what do you do here?  Rather than 3 full months on a complete refactor, spend 25% of your time over the next year.  It's the same time commitment either way, but with the 25% plan, you get time to analyze and plan.  You get time to untangle.

``` 



**Irresponsible refactoring**

``` 
from: http://www.thekua.com/atwork/2013/01/independent-refactoring-is-irresponsible/


JANUARY 21, 2013
INDEPENDENT REFACTORING IS IRRESPONSIBLE


I remember working with one client who spun up a “refactoring team”.
It sounded great at the outset – a legacy application that had plenty
of code as they knew the development team cut corners to meet their
milestones. Rather than completely halt new development, they split
out a small team who would refactor mercilessly. This team spent one
whole month renaming classes, adding tests, adding patterns here and
there, fighting the big cyclomatic complexity numbers and then
claiming victory on their poor opponent (the codebase). The result
after the refactoring team… new feature development slowed down even
more.

When investigating why feature development slowed down, we discovered
the following:

New, and unfamiliar designs – The refactoring team did some wonderful
work cleaning up certain parts of the code base. They made some places
consistent, introduced a few patterns to tackle common smells and
honestly helped reduce the code base. What they neglected to do was to
inform the other developers of the new design, where they now needed
to look for the same functionality and the intention behind the newly
named classes. Instead, the developers working on new functionality
struggled to find the huge, very finely commented code they were
familiar with and then when they tried to apply their old technique
for fixes, failed to do.
Immediately irrelevant refactoring – With most codebases, there are
parts that change a lot, parts that change a bit, and parts that
almost never change. In my experience (disclaimer: not researched)
those parts that change a lot and are highly complex end up as a huge
source of bugs. Those parts that don’t change can remain ugly and
still be perfectly fine. In the case of this client, a lot of effort
spent refactoring ended up in areas where new functionality wasn’t
being added.
A divide in cultures – I heard a few snarky comments during this time
from the new feature development team about the refactoring team,
basically implying most of them to be developer-divas whilst they had
to do all the grunt work. The result… by outsourcing refactoring, the
new feature team basically cared less about the codebase and I’m sure
they weren’t helping the clean up with the code they added.


My reading on lean thinking taught me that you need to Build Quality
In and that separately quality from the product ends up costlier and
results in poorer quality.

The answer… is actually quite clear in Martin Fowler’s book on Refactoring:

Refactor because you want to do something else (i.e. add function, or fix a bug)

I return to my current situation. We achieved the tripling in velocity
because we spent time thinking about why adding a new feature was so
cumbersome. I’ll admit that I spent a lot of time (almost a day)
trying to add part of a new feature, attempting a few refactors and
rolling back when they did not work. I was trying to get a feel for
what steps we did most often, and attempted several approaches (most
failed!) to make it simpler, clearer and added the least amount of
code. We did settle on some patterns and we realised its benefits
almost immediately – adding a new feature that previously took us a
day to implement now took us only an hour or two with tests.

I find that sometimes the most satisfying part of software development
is actually reshaping existing code so that the addition of a new
feature is just a single method call, or just a single instance of a
class. Unfortunately I don’t see this often enough.

Written by Patrick Posted in Agile, Development
``` 


