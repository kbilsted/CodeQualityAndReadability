draft
# Optimal naming in programming
*Author: Kasper B. Graversen*
<ArticleHeaderUrls/>
<Categories Tags="Design, Business_Pattern, Design_Pattern ">
</Categories>


*One central aspect of code readability is naming. Virtually everything in programming has a name. Well, except anonymous types and methods of course! Naming is one of the most important things in software development.  Without naming, You are unable to create abstractions. Without abstractions you end up with procedural  thinking. Procedural thinking leads to procedural code. And now you got a 3000 lines class on your hands. 

Here we look into aspects of naming in programming.*


Please show your support by sharing and voting:

<SocialShareButtons>
</SocialShareButtons>

<img src="img/httpspixabay.comdavand-anlæg-grøn-bøde-lag-abstrakt-821293.jpg" alt="from https://pixabay.com/da/vand-anl%C3%A6g-gr%C3%B8n-b%C3%B8de-lag-abstrakt-821293/" >


Table of Content

   * [1. Introduction](#introduction)
     * [1.1 Set a target "audience"](#set-a-target-audience)
     * [1.2 Encoding information in names](#encoding-information-in-names)
   * [2. Naming namespaces](#naming-namespaces)
   * [3. Naming interfaces](#naming-interfaces)
   * [4. Naming classes](#naming-classes)
     * [Encoding the layer](#encoding-the-layer)
     * [Naming technical classes](#naming-technical-classes)
   * [5. Naming fields and variables](#naming-fields-and-variables)
     * [Naming constants](#naming-constants)
   * [Avoid negation in names](#avoid-negation-in-names)
   * [Change magic numbers to constants](#change-magic-numbers-to-constants)
   * [Name constants according to usage not content](#name-constants-according-to-usage-not-content)
   * [Reconsider your constants to be variable](#reconsider-your-constants-to-be-variable)
   * [Implementing domain heavy logic](#implementing-domain-heavy-logic)
   * [Don't encode types in names](#dont-encode-types-in-names)
   * [Method names have singular and plural forms](#method-names-have-singular-and-plural-forms)
   * [Name variables by their singular or plural form](#name-variables-by-their-singular-or-plural-form)
   * [How to name types in the solution domain](#how-to-name-types-in-the-solution-domain)
   * [How to name your coordinator classes](#how-to-name-your-coordinator-classes)
   * [Name types by their singular or plural form](#name-types-by-their-singular-or-plural-form)
   * [Naming base classes](#naming-base-classes)
   * [Names are flawed, use types](#names-are-flawed-use-types)
   * [Avoid prefixing Gui variables](#avoid-prefixing-gui-variables)
   * [Avoid prefixing  members with _](#avoid-prefixing--members-with-_)
   * [Short scopes short naming](#short-scopes-short-naming)
   * [6. Naming in tests](#naming-in-tests)
   * [Summary](#summary)
	 
   ...

   
   
   
## 1. Introduction

Possibly the hardest thing about programming is to make the code readable. That means that *people* need to understand what the code does. 

One central aspect of code readability is naming. Virtually everything in programming has a name. Well, except anonymous types and methods of course! Naming is one of the most important things in software development.  Without naming, You are unable to create abstractions. Without abstractions you end up with procedural  thinking. Procedural thinking leads to procedural code. And now you got a 3000 lines class on your hands. 

Unfortunately, naming is really difficult to get right. We've all questioned a piece of shit code, thinking "what the **** is this code doing, what was that developer doing! Let me just check who... oh.. it was *me*   two years ago". When it is difficult to get naming right for yourself, no wonder it is hard finding good names, that someone else has to read!

The act of naming does not get easier with experience. But I repeatedly experienced that when I am struggling distilling the intention of a thing with a reasonable good name, I probably need to refactor it into several smaller components. Each of which will be easier to name. This applies to namespaces, types, methods and even variable names.

Therefore, make conscious attempts to live according to your values and beliefs on good naming. If in doubt, glance over this article. Here we look at aspects of naming and try to serve guidelines for what can constitute good naming practices.




### 1.1 Set a target "audience"

In the book "The art of readable code" the definition is 

> Code should be written to minimize the time it would take for someone else to understand it.

But who is this "someone else"? Is it the less experiences noob programmer? Is it the seasoned programmer without domain knowledge? When writing any piece of literature, it is **key** to be aware of your audience. In technical literature, it is a deed, if not a requirement, to make the target audience explicit. It is funny, how people seems to forget this also applies to writing programs. Any coding guideline should make explicit the expected average programmer. 

It is not uncommon for developers to complain about unreadable code, when in fact, the problem is closer to them than the code. For example, the code may implement complicated domain logic, and may well require the developer to understand the domain, before much sense can be made out of the implementation of some work flow. Often it is not the naming in itself, it is the naming combined with lack of domain knowledge.

Another example, is that of meta-programming. The act whereby the program investigates a program (possibly itself) and dynamically invoke methods and pass parameters around. It may even do run-time compilation. Such code will make any less experiences programmer look bewildered. The trail of thought is very different than ordinary business programming. And so is the API. With lack of knowledge in both worlds, no naming scheme will steer you safely to harbour. Sure you can throw in a lot of explanatory comments painstakingly detailing every line of code. But it will only explain better the API to the less experienced programmer, where as to the more experience programmer, the comments may be in the way (and with time, risk drifting from what the code actually does).




### 1.2 Encoding information in names

Before we get started, it is important to first introduce the concept of *encoding*. By encoding, I refer to the process by which you pump additional information into a name by adding one or more letters to the name. The most prominent of such encoding schemes is the [Hungarian notation](https://msdn.microsoft.com/en-us/library/Aa260976) named after Charles Simonyi, who happened to be a Hungarian. In his naming scheme, variables are prefixed with letters and numbers to reveal their data type. For example "l" denoting a `long`, "sz" a zero-terminated string, "pX" a pointer to a type X, and so forth. The popularity of the notation rooted in BCPL code as there was nothing in the language itself that helped the programmer remember the types. 

Today many regard the Hungarian notation an abomination - obscure variable names, difficult to pronounce and communicate with your colleagues. Perhaps the notation suggested by Charles Simonyi has become obsolete in modern languages, due to a much improved type system and IDE's doing on-the-fly-compilations.

But this is not to say that encoding information into names is not useful. In fact we do it all the time. For example beginner programmers have a tendency to use the prefixed `my` or `obj`. 
`Book myBook = new Book();` or `var objManager = new Manager();` comes to mind.
   
I'm not sure where pick this up, but it's sort of universal. I've seen it in code from all over the world. And you'll see it in many guides for beginners learning to program. By heuristics I've come to conclude that "My" means some best effort to come up with a name, and "obj" apparently helps the programmer in distinguishing the object from the class.
  
And there are many other modern examples. Using `m_` or `_` to denote instance variables, ALL_UPPERCASE to denote constants, "I" to denote an interface, to name a few. 

A famous proponent for using hungarian notation is Spolsky's [Making Wrong Code Look Wrong](http://www.joelonsoftware.com/articles/Wrong.html). Unfortunately, that article is a bit dated, at least, I would encode the propose information using types instead of in the name, since that provides compiler support. If you don't know the article, it is worth the read though.
   
  
  


## 2. Naming namespaces

A name space, or package, is a tool for separating the application into different compartments, making a class name unique across an application. With the Java language, a naming scheme was introduced whereby the company/organization url became part of the name space. An idea most modern projects I've looked at has taken to heart. Typically like `com.megacorp`.

For a multi-layered application, it is only natural to incorporate into the namespace name, the name of the layer. Typically postfixed after the company name like `com.megacorp.gui` and `com.megacorp.server`.


## 3. Naming interfaces

From the MSDN guide lines, 

* Name interfaces with nouns or noun phrases, or adjectives that describe behavior. For example, the interface name `IComponent` uses a descriptive noun. The name `ICustomAttributeProvider` uses a noun phrase and  `IPersistable` uses an adjective.
* Prefix interface names with the letter I, to indicate that the type is an interface.

Over time, there has been disputes as to whether the last guide line is a good idea. People argue, that it is unnatural to pronounce or talk about an IFactory or ICustomer. Instead, they propose to use interface names as suggested in the first bullet above, but without the "I". Concrete classes implementing the interfaces then is forced to use concrete names, e.g. a class cannot be named `Component` since the interface already goes by this name. 

Personally, 




## 4. Naming classes

Before going into the nitty gritty details, let us first recall what we use classes for. Typically, classes represents things or phenomena. But a class can equally well describe a (business) process. Naturally, a class name should likely be a *noun* since a noun is "the part of speech that names a person, place, thing, or idea".

It would be weird with a class named `OpenDoor`. Even worse, what do we now call the method that does the opening of the door inside such a class? `Execute()` would be a candidate. I'd instead prefer the class `DoorOpener`, and a method `Open()`.



### Encoding the layer
PCMEF+'s Class Naming Principle 
The Class Naming Principle makes it possible to recognize in the class name to what package it belongs. 
To this aim, each class name is prefixed in PCMEF with the first letter of the package name (e.g. 
EInvoice is a class in the entity package). The same principle applies to interfaces. Each interface 
name is prefixed with two capital letters – the first is the letter “I” (signifying that this is interface) and the 
second letter identifies the package (e.g. ICInvoice is an interface in the 
control package




### Naming technical classes
A technical class is a class that does something technical such as provide an attribute, an extension method, or otherwise extend the language at hand. That is, the class has little to do with the business problem at hand, that we are trying to solve through the making of an application.

In fact, a very pragmatic naming convention users of MVC web-frameworks take, is to name suffix their controllers with "Controller", their views with "View" etc. Some frameworks even enforce this naming convention, as the naming serve as an implicit configuration.



## 5. Naming fields and variables

### Naming constants





FROM BOOK
FROM BOOK
FROM BOOK
FROM BOOK
FROM BOOK
FROM BOOK
FROM BOOK




## Avoid negation in names

`doesNotAllowxxFiles = false`. seems harmless on it own.. but when are we entering the follwoing if-block


    if(!doesNotAllowxxFiles)
    {
      // work
    }  
    
        
  Yep, double negatives, negatives combined with and's and/or or's (sic!) and negatives combined with a method/var with Not in the name, get my brain in a twist every time :-) � 
  
        
  if you find yourself having trouble understanding what's going on, I think it's often useful to do thing like: simpleBool = (!(complexBool || randomfFlag)) && !randomFlag �  
  
  I would agree with you, except for the existence of Guard Clauses (which always have the exceptional condition in the THEN portion of the IF statement, not the common condition). �  
  


      return !type.IsGenericType 
        ? type.Name 
        : type.Name.Substring(0, type.Name.IndexOf("`"));

vs

      return type.IsGenericType 
        ? type.Name.Substring(0, type.Name.IndexOf("`"))
        : type.Name; 
        
        

## Change magic numbers to constants

    A magic variable is a variable that holds a meaning that is not reflected by its contents. The integer value '10' reflects the meaning of the number 10, so there is no need to make it a constant NumberTen = 10 That is pointless as the number 10 will not be redefined. MaxRetryCount = 10 That has a point a we may want to change the max retry count. private const char SemiColon = ';';  Dumb. 

and

    Do not use literal values, either numeric or strings, in your code other than to define symbolic constants. Use the following pattern to define constants:
    public class Whatever
    {
      public static readonly Color PapayaWhip = new Color(0xFFEFD5);
      public const int MaxNumberOfWheels = 18;
    }
    There are exceptions: the values 0, 1 and null can nearly always be used safely. Very often the values 2 and -1 are OK as well. Strings intended for logging or tracing are exempt from this rule. Literals are allowed when their meaning is clear from the context, and not subject to future changes.

and

    the point is to have readable code. 
    Consider the difference between: if(request.StatusCode == 1) and if(request.HasSucceeded). In this case, I would argue the latter is far more readable, but that doesn't mean you can't ever have code like int MaxNumberOfWheels = 18. 


    
However, I find `""` much more readable than `string.Empty`.





## Name constants according to usage not content

what do you do when you find people are creating atrocious constants like the following



    private const char SemiColon = ';';
    private const char Space = ' ';
    private const int NumberTen = 10;
    The argument you need to be making with your colleague isn't about naming a literal space as Space but his poor choice of name for his constants.

    Let's say your code's job is to parse a stream of records which contain fields separated by semicolons (a;b;c) and are themselves separated by spaces (a;b;c d;e;f). If whoever wrote your spec calls you up a month from now and says, "we were mistaken, the fields in the records are separated by pipe symbols (a|b|c d|e|f)," what do you do?


    Under the value-as-name scheme your colleague prefers, you'd have to either change the value of the literal (SemiColon = '|') and live with code that continues to use SemiColon for something that isn't really a semicolon anymore. That will lead to negative comments in code reviews. To abate that, you could change the name of the literal to PipeSymbol and go through and change every occurrence of SemiColon to PipeSymbol. At that rate you might as well have just used a literal semicolon (';') in the first place, because you'll have to evaluate each use of it individually and you'll be making the same number of changes.

    Identifiers for constants need to be descriptive of what the value does, not what the value is, and that's where your colleague has made a left turn into the weeds. In the field-splitting application described above, the semicolon's purpose is a field separator, and the constants should be named accordingly:

    private const char FieldSeparator = ';';    // Will become '|' a month from now
    private const char RecordSeparator = ' ';
    private const int MaxFieldsPerRecord = 10;
    This way, when the field separator changes, you change exactly one line of code, the declaration of the constant. Someone looking at the change will see just that one line and will immediately understand that the field separator changed from a semicolon to a pipe symbol. The remainder of the code, which didn't need to change because it was using a constant, remains the same, and the reader doesn't have to dig through it to see what else was done to it.    
    
    
## Reconsider your constants to be variable

constants tie you down and make unit testing difficult. How often are your constants really constant? 

for example, const'ing your database connection string brings you in deep water in terms of testing. maybe it would have been better to look up that connection string, and then in unit testing mode inject some one else to respond






  
    
## Implementing domain heavy logic 

See its very easy to stand 5 km high, above the clouds and tell people to use "descriptive names" and avoid one-letter variables. But what do you do when you are implementing science. By that I mean, you are applying a knowledge, algorithms and formulas from a specific scientific field. You'll find that in most fields of science there exist a specific nomenclature, a set of names used in a particular science by a community. The implementation of a formula becomes easier to read when you use the ubiquitously used names from within that field of science.


Newtons law of universal gravitation

 $$F = G \frac{m_1 m_2}{r^2}\ $$
 
where:
F is the force between the masses,
G is the gravitational constant,
m1 is the first mass,
m2 is the second mass, and
r is the distance between the centers of the masses.    
       
    If you find yourself unable to understand mathematical code because of short variable names, be aware that it may be because you don't understand the mathematics, not because the names are too short. Altering mathematical expressions you don't understand is not a high-reliability process. Once you understand the math, the length of the variable names is irrelevant. Do others a favor and leave a citation (in a comment) to some relevant description of the math, though, if you had to learn it!
       
and




    It appears that these variable names are based on the abbreviations you'd expect to find in a physics textbook working various optics problems. This is one of the situations where short variable names are often preferable to longer variable names. If you have physicists (or people that are accustomed to working the equations out by hand) that are accustomed to using common abbreviations like Rin, Rout, etc. the code will be much clearer with those abbreviations than it would be with longer variable names. It also makes it much easier to compare formulas from papers and textbooks with code to make sure that the code is actually doing the computations properly.

    Anyone that is familiar with optics will immediately recognize something like Rin as the inner radius (in a physics paper, the in would be rendered as a subscript), Rout as the outer radius, etc. Although they would almost certainly be able to mentally translate something like innerRadius to the more familiar nomenclature, doing so would make the code less clear to that person. It would make it more difficult to spot cases where a familiar formula had been coded incorrectly and it would make it more difficult to translate equations in code to and from the equations they would find in a paper or a textbook.

    If you are the only person that ever looks at this code, you never need to translate between the code and a standard optics equation, and it is unlikely that a physicist is ever going to need to look at the code in the future perhaps it does make sense to refactor because the benefit of the abbreviations no longer outweighs the cost. If this was new development, however, it would almost certainly make sense to use the same abbreviations in the code that you would find in the literature.

    
and

     include the comments on where to find the original reference material .
    
    
    
    
    
    example from web
    
    
    double dR, dCV, dK, dDin, dDout, dRin, dRout
    dR = Convert.ToDouble(_tblAsphere.Rows[0].ItemArray.GetValue(1));
    dCV = convert.ToDouble(_tblAsphere.Rows[1].ItemArray.GetValue(1));
    ... and so on
    
    Maybe it's just me, but it told me essentially nothing about what they represented, which made understanding the code further down difficult. All I knew was that it was a variable parsed out specific row from a specific table, somewhere. After some searching, I found out what they meant:

    dR = radius
    dCV = curvature
    dK = conic constant
    dDin = inner aperture
    dDout = outer aperture
    dRin = inner radius
    dRout = outer radius

    
    
    
    
    
    
    
    
    
## Don't encode types in names   
    
    string s_UserComment = Encode(req.usUserComment);
    
  =>
    
    SafeString userComment = Encode(req.UserComment);
    
    
* joel spolsky "Making Wrong Code Look Wrong"  http://www.joelonsoftware.com/articles/Wrong.html

bad advice, no guarantee - and easily enforced by the compiler




## Method names have singular and plural forms

    IEnumerable<string> GetCode(DateTime time)

    void ProcessOrderItem(List<OrderItems> items)
    
  =>
  
    IEnumerable<string> GetCodes(DateTime time)

    void ProcessOrder(List<OrderItems> items)
    
We use _verbs_ and _noun_ for naming methods, typically in the form `<verb><noun>`.  _Verbs_ are "doing words" that express actions or state. _Nouns_ are often described as referring to persons, places, things. In programming we choose the noun to represent the data that is worked on, or returned by the method. Examples: `ProcessOrder`, `GetNames`. 

Nouns can either be in their singular (_'s??gy?l?r_) or plural (_'pl??r?l_) form. The plural form often appends **-s** or **-es** to the noun: "Code" becomes "Codes and "Dish" becomes "Dishes" in plural form. 

Whether to use the noun in its singular or plural form, has a big impact in terms of setting the expectations of the reader for the code he is about to indulge in. The reader will do a double  take if he encounters the method `Ticket[] GetTicket()`.

* When working on, or returning a single piece of data consider using the singular form of the noun.
* When working on, or returning multiple data consider using the plural form, or use a noun that indicates the multiplicity.

Lets put the theory to practice. 

    IEnumerable<string> GetCode(DateTime time)

The return type here is a dead give-away. We are returning multiple codes for a particular time. Clearly, themethod should be named `GetCodes()` But what about the following?

    Tuple<string, string> GetName()
    
What are we returning here? It depends on the project of course. But I could assume it is the the first name and last name. More importantly, however, had the method been named `GetNames()` I may interpret and construct am assumption about a primary name and maybe a calling handle of some sort.  In either case, we should consider writing a class that holds the two strings. Of course such deliberations should take the project size into consideration as one of many parameters. This enables us to **name** the properties we are returning and whilst at the same time making the method signature more readable

    Name GetName()

  See xxx on types over names for a further discussion.





## Name variables by their singular or plural form



## How to name types in the solution domain

    class ExtractUserData 
    
  =>
   
     class UserDataExtractor

The short answer is to look for nouns in the problem domain, for example, your use cases. _"A user enters the library and makes a book reservation"_. Straight forward you can identify the domain classes `User`, `Book`. But what about reservation? It could be a method on the book class, and as such could easily be named `Reserve()`. Imagine for a moment, that we are building a large enterprise system for a library, chances are, that a reservation is a laborious process involving non-trivial work, communication with a number of services amongst other things. In this case, we'll probably have the reservation be handled by a separate entity. Maybe a `ReservationHandler` or `ReservationCoordinator`. Notice the naming is on the form `<noun><verb>`. Where _nouns_  are referring to a person, a places, or a thing, where  _verbs_ are "doing words" that express actions or state. This is the direct opposite of naming methods!

Thus a class such as `Reservation` or `BookReservation` badly convey that we are implementing a process of some kind. Now some people feel very passionately about avoiding postfixing `Manager` to your classes. "Manager" is a heavily used verb in programming and it conveys very little meaning as to what the responsibilities of your class. I don't feel strongly against "manager" as such. To me it is  as descriptive as "Coordinator", "Handler" or "Container". Needles to say, though, if your so-called manager class is only building, say like a file path, `FilePathBuilder` is superior to `FilePathManager`. Similar bashing is often applied to "Helper". It has a similar broad meaning and thus fit virtually any context. And if raises the question, why is the content of the helper is not simply added to the entity that it is helping.


So be aware of verbs that are very general. Now you know the dangers they pose. But more importantly, is for your team to establish a nomenclature, define the meaning of typically verbs and consistently sticking to it in the code. For example, "manager" in your code base, indicated a class responsible for the life-cycle of objects. I've seen code where consistent use of "Adapter" denoted the augmentation of Gui forms, holding information about each element of the form and its status (changed, old value etc).

 

Lastly, an alternative, is to simply ignore the verb altogether. So you can stick with `HttpUploader` rather than `HttpUploadManager`. And instead of `UrlManager` use `Url`.


To serve as inspiration here are a list of often used verbs in code. 

- Coordinator
- Builder
- Writer
- Reader
- Handler
- Protocol
- Target
- Converter
- View
- Factory
- provider
- service
- factory

and some more general words that you may use with caution

- Helper 
- Manager 
- Util
- Controller
- Wrapper
- Container

naming extensions that augment data or possibly your domain entities

- attribute
- type
- collection
- info
- element
- node
- option
- context
- item
- designer
- base
- editor    
- Entity
- Bucket



## How to name your coordinator classes





## Name types by their singular or plural form

enums

DO name flag enums with plural nouns or noun phrases and simple enums with singular nouns or noun phrases. (http://msdn.microsoft.com/en-us/library/ms229058.aspx)



## Naming base classes

`Base` or `Abs` to indicate that these are "hooks" into your api...


## Names are flawed, use types

* Call your methods whatever you want. Names are flawed. It is the type that are of importance. What are the types of the input parameters, what are the return type. Those are the true revealing cues when you first glance a method and needing to understand it.



## Avoid prefixing Gui variables 

    Label lblOk = new Label(..)
    Button buttonOk = new Button(..)
    Action actionOk = ..
    
  =>
  
    var ok = new LabeledButton(new Label(..), new Button(..), action);

I never understood why, but its a reoccuring theme, that when you dive into Gui code, all sorts of weird prefixing pops up. I'm guessing that programmers most of the time are less experience with  Gui programming and that gui programming is of a different nature - it has to do with layouts and nice user experiences. Or maybe it is just that it is the junior programmer who is assigned the Gui tasks since the senior staff rather hold on to a red-hot iron pole than touching that part of an application. I've seen my fair share of variable such as

    Label lblOk = ...
    Button buttonOk = ...
    Action actionOk = ...
    Label lblCancel = ...
    Button buttonCancel = ...
    Action actionCancel= ...

   
... .... grouping makes for reusable components. less code duplication etc. Changes are that your design and code is of higher quality and more attention to detail once you start reusing it rather than copy-pasting it.
  
  


## Avoid prefixing  members with _ 

    private Price computePrice(Product product)
    {
        Price result = calculator.ComputePrice(product, Path_Default);

        if (_ruleset.isEnabled)
        {
            result.applyRuleset(_ruleset);
        }

        return result;
    }
    
    By glancing at this piece of code, you easily notice both _ruleset, but don't visually recognize Path_Default as a potential match.

    About its benefits...

    While this improves readability, the usage of underscore character as a prefix is not always a good idea. Some standards and style guides, such as the official C# style guide, forbid using underscore as a prefix for private variables, in the same way as it is forbidden to use Hungarian notation. The reason for that is that:

    First, the cases where you need to know the visibility of a variable is pretty rare, and in those cases, the IDE helps you to find this information in less than three seconds.

    Second, all those notations add visual clutter. Since it is not essential, it should be removed.

    Third, it is not unusual to end up with code like this:

    public string _Title { get; set; }
    and the sole fact that such convention may lead to misleading code and easily avoidable bugs, while the IDE will never lie is a good reason to avoid such conventions.

    This being said, other communities using other languages find underscore prefix useful enough to communicate information. They may take a risk for the underscore prefix to become misleading (if the variable changes from private to public or from public to private but is not renamed properly), the risk similar to one that each developer takes when he writes a comment which may not be updated by the successive maintainers of the piece of code.


and

  First, the cases where you need to know the visibility of a variable is pretty rare, and in those cases, the IDE helps you to find this information in less than three seconds.

  Second, all those notations add visual clutter. Since it is not essential, it should be removed.

  Third, it is not unusual to end up with code like this:

  public string _Title { get; set; }
  and the sole fact that such convention may lead to misleading code and easily avoidable bugs, while the IDE will never lie is a good reason to avoid such conventions.

  
  
  
  
  
## Short scopes short naming 

set by uncle bob in clean code page xx...  and carries a good explanation. I'd go as far as to put forth the same recommendation!

but lets see how quickly we can break this rule.

eg. from page 13 mike kr�ger http://www.icsharpcode.net/technotes/sharpdevelopcodingstyle03.pdf 


what does the following code do?

    for (int i = 1; i < num; ++i) 
      meetsCriteria[i] = true; 

    for (int i = 2; i < num / 2; ++i) 
    {
      int j = i + i;
      while (j <= num) 
      {
        meetsCriteria[j] = false;
        j += i;
       }
    }

    for (int i = 0; i < num; ++i) 
      if (meetsCriteria[i]) 
        Console.WriteLine(i + " meets criteria");
     
try intelligent naming:

    for (int primeCandidate = 1; primeCandidate < num; ++primeCandidate)
     isPrime[primeCandidate] = true; 

    for (int factor = 2; factor < num / 2; ++factor) 
    {
      int factorableNumber = factor + factor;
      while (factorableNumber <= num) 
      {
        isPrime[factorableNumber] = false;
        factorableNumber += factor;
      }
    }
    
    for (int primeCandidate = 0; primeCandidate < num; ++primeCandidate)
     if (isPrime[primeCandidate]) {
       Console.WriteLine(primeCandidate + " is prime.");


see even for short-scoped code we struggle with identifying what the meaning of `i` and `j` is.


Another problem with very short names to look out is when the names //looks// the same in the editor. The lower case letters `i` and `j` are visually quite similar, the same for `n` and `m`. Using variables that visually almost look the same makes it difficult for the reader to read the code. Also a simple mistype are not easily. Take the following implementation of the //Sieve of Eratosthenes//:

    for (int i = 2; i*i <= N; i++) 
      if (isPrime[i]) 
        for (int j = i; i*j <= N; i++) 
          isPrime[i*j] = false;

Now did you spot the error? By accident I am incrementing `i` rather than `j` in the second loop. 











FROM BOOK
FROM BOOK
FROM BOOK
FROM BOOK
FROM BOOK






## 6. Naming in tests

## Summary




            | kind                  | short    |   Long    |   Encoded   |  
            |-----------------------|----------|----------|--------------|
			| class                 |           |          |    |
			| interface             |           |          | x   |
			| design pattern impl   |           |          |    |
			| extension method impl |            |         |   |
			| public method         |            |         |   |
			| private method         |            |        |   |
			| method returning bool|            |          |   |
			| method returning null/maybe|            |         |   |
 			| bool variable          |            |         | |
 			| variable in small scope |            |         | |
			| variable in large scope |            |         | |
			| variable for gui code  |            |         |  |
			| field                  |            |         |  |
			


Please show your support by sharing and voting:
<SocialShareButtons>
</SocialShareButtons>



<br><br>
<CommentText>
</CommentText>

<br><br>
