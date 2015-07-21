# Stepwise refinement of code
*Author: Kasper B. Graversen*  
[[Introduction]](https://github.com/kbilsted/CodeQualityAndReadability/blob/master/README.md) [[All categories]](https://github.com/kbilsted/CodeQualityAndReadability/blob/master/AllTags.md) [[All articles]](https://github.com/kbilsted/CodeQualityAndReadability/blob/master/AllArticles.md) 
<br>
[![Stats](https://img.shields.io/badge/Tag-Refactoring-99CC00.svg)](https://github.com/kbilsted/CodeQualityAndReadability/blob/master/Tags/Refactoring.md)
[![Stats](https://img.shields.io/badge/Tag-Code_Readability-99CC00.svg)](https://github.com/kbilsted/CodeQualityAndReadability/blob/master/Tags/Code_Readability.md)
<br>
[![Tweet this](https://github.com/kbilsted/CodeQualityAndReadability/blob/master/img/twitter.png)](http://spredd.it/3ahazNUZ)
[![Googleplus this](https://github.com/kbilsted/CodeQualityAndReadability/blob/master/img/gplus.png)](https://plus.google.com/share?url=https://github.com/kbilsted/CodeQualityAndReadability/blob/master/Articles/Design/StepwiseRefinementOfCode.md)  

*We all know that code left alone rot over time. Yet it is hard to get the time to solely change code for the purpose of changing it. And when you do get the time, it is hard to find the inclination and energy to really dig in.*    

*A technique I often employ to accommodate these problems, is to step-wise refine the code while reading it during the implementation of change requests or adding features.*

<SocialShareButtons>
</SocialShareButtons> 


<img src="img/pixabay.com_da_arbejder-vedligeholdelse-sejlere-665004.jpg">


Table of Content
   * [1. Introduction](#1-introduction)
   * [2. Hands on the code](#2-hands-on-the-code)
   * [3. Adding notes](#3-adding-notes)
   * [4. Removing notes](#4-removing-notes)
   * [5. Extract to private method repeatedly...](#5-extract-to-private-method-repeatedly)
   * [6. Extract to public method](#6-extract-to-public-method)
   * [7. Inline method](#7-inline-method)
   * [8. Conclusions](#8-conclusions)
   * [9. Comments](#9-comments)

   
   
## 1. Introduction

This article chronicles a real session of reading and step-wise refining a piece of code from the.

 Let me stress, that the refinements are made in small and safe increments. Each of which denotes a safe place to stop. Safety is of utmost importance. When you are afraid of making change in the fear of breaking something, you refrain from making them. 

Aside from safety, making changes in small increments has the added benefit, that you can be interrupted by say an emergency situation without much harm. Since the code is lefter better than it was, it can be shipped. We can always return to the code days or months later. With a half-done improvement left days or months is typically work lost. Certainly, it requires extra work, and possibly extra tests to ensure that nothing is broken. 



## 2. Hands on the code

The other day I was grokking some code for the [StatePrinter project](https://github.com/kbilsted/StatePrinter). I was at a point where the internal circuitry  of the project was in need of rewiring due to new features.  I fell upon the following code which judging from the name, parameters and return type converts a `Type` into a list of fields. But is that really what the method is doing? This code is by no means magic, it is the bread-and-butter for project. Hence we are interested in proper naming and/or separation of concerns.

```C#
public List<SanitiedFieldInfo> GetFields(Type type) 
{
    var fields = new HarvestHelper().GetFields(type);

    var res = new List<SanitiedFieldInfo>();
    foreach (var field in fields) 
    {
        switch (field.FieldInfo.MemberType) 
        {
            case MemberTypes.Field:
                if ((field.FieldInfo as FieldInfo).IsPublic) 
                    res.Add(field);
                break;
            case MemberTypes.Property:
                var propertyInfo = (PropertyInfo) field.FieldInfo;
                if (propertyInfo.GetGetMethod(false) != null) 
                    res.Add(field);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    return res.ToList();
}
```

After a one-pass trawl over the code, we realize that the method does two distinct things. It gets a list of fields from some reflection-wrapper, then filters away all non-public entries. After having read the code, the empty line between `fields` and `res`  (line 2 and 4) was a dead give-away of these two concerns.  While adding empty lines to show a switch of focus is best practice, it is not guaranteed that the next reader will have the same understanding - especially if he is not aware of that convention.


## 3. Adding notes

If I have difficulties reading code, maybe due to lack of experience with that part of the code base, or lack of domain knowledge, I sometimes add notes to the code. I annotate the code using comments with my first impression understanding as I go along. Now I don't want to document every single line, just highlight the main themes. This is meant as a light-weight process, I'm definitely not documenting parameters or whether a method may thrown an exception for example.  The result is:

```C#
public List<SanitiedFieldInfo> GetFields(Type type) 
{
    var fields = new HarvestHelper().GetFields(type);

    // Filter
    var res = new List<SanitiedFieldInfo>();
    ...
}
```

Naturally, in a small piece of code like this, the number of concerns are limited. Only two things are going on. Gathering data, and filtering. Notice that I didn't bother inserting a comment for the first line. I find it hard to properly document a one-liner. If I ever feel the need to do so, I'd rather rename the method being called. Only document what is not self-explanatory. 

Now, the story might as well end here. If we assume that my mental notes are correct, we have left the code a little better than how we found it. *"But we haven't changed anything"*, you argue. Well, to the compiler we didn't, but for the next reader of the code we did. We made clear to two sub processes of the method. And that has value.  Thinking about it, comments have some nice properties:

1. They are guaranteed not to change the behaviour of the code. Are you changing production code, or are you really close to a release deadline, likely you do not want to rock the boat unnecessarily. 
2. They serve as excellent bookmarks for later refactorings (made by you or some other person).

Now, there are some pitfalls as well. 

1. My notes could be wrong, after all they are my first impressions.
2. Later changes made to the code is not guaranteed change the documentation, this is especially true when merging in branches where there are no conflicts. 
3. I value the principle of *"don't document the obvious"*, so avoid that

... This is why most of the time I don't stop here.

Let us pause for a moment reflect on our deeds. Are we cheating management by improving something we are not paid to improve? No. The time-consumer here is *not* inserting the comments or figuring out how to word them. It is to read and understand the logic of the code. Remember, code is read many times more often than (re-)written. 


## 4. Removing notes
To best combat the problem of documentation gradually drifting from the code, we remove comment altogether!  One way to remove a comment this is to apply the "Extract method" refactoring pattern. Below we remove the comment `// filter` and replace it with a method `Filter()`.

```C#
public List<SanitiedFieldInfo> GetFields(Type type) 
{
    var fields = new HarvestHelper().GetFields(type);

    return Filter(fields);
}
```
and this

```C#
List<SanitiedFieldInfo> Filter(List<SanitiedFieldInfo> fields) 
{
    var res = new List<SanitiedFieldInfo>();
    foreach (var field in fields) 
    {
        switch (field.FieldInfo.MemberType) 
        {
            case MemberTypes.Field:
            ...  
```

Applying the "Extract method" refactoring may not always be a viable solution. For example,  when the extracted method takes many arguments, say 8. In such cases, and depending on the source code layout, we may spend 8-10 lines calling the method, and a similar amount of lines on defining the parameter list of the function.  Assuming the code we are extracting to a method is less than 8 lines of code, we are replacing a one-liner comment with roughly 20 lines of code for the method call and the method definition. This may instead lead to cluttering the overview and hurt readability.

Large classes is another example where the code does not lend itself to such refactorings. With a very large class, you now have to mentally juggle all the state and logic of the class along with only being able to read fragments of the code (a method). A first stab on such a code base, it is likely beneficial to separating the class into several classes. 

Naturally, the rules here are not set in store. Once I was faced with code that had so many nested scopes that extracting into methods was the only conceivable way to ensure that new requirements were added correctly. Go with your gut-feeling. 


## 5. Extract to private method repeatedly...

Now that we have understood the overall motivation for the `Filter()` method, we can focus our mental energy on the `res` variable. Clearly it is the result of the execution since it is being returned. I've come to appreciate more and more, to extract the body of looping constructs. The reason being that we separate the traversal of steps from the execution in each step. Typically the two are unrelated.

This is somewhat comparable to why I favour using a `foreach` over a `for`. With the `foreach` I do not have to concern myself with the indexing variable. Is it modified in the loop body? Is it always incremented by 1? etc. 

When we extract the method body we get

```C#
List<SanitiedFieldInfo> Filter(List<SanitiedFieldInfo> fields) 
{
    var res = new List<SanitiedFieldInfo>();
    foreach (var field in fields)
        FilterPublic(field, res);

    return res;
}

void FilterPublic(SanitiedFieldInfo field, List<SanitiedFieldInfo> res)
{
    switch (field.FieldInfo.MemberType) 
    {
        case MemberTypes.Field:
            if ((field.FieldInfo as FieldInfo).IsPublic)
               res.Add(field);
            break;
        case MemberTypes.Property:
          var propertyInfo = (PropertyInfo)field.FieldInfo;
          if (propertyInfo.GetGetMethod(false) != null)
             res.Add(field);
          break;
        default:
            throw new ArgumentOutOfRangeException();
    }
}
```

## 6. Extract to public method

We realize after further scrutiny that if there is a match, we add the current field to the `res` list. And that for each invocation of the method a maximum of one element (the field) is added. Hence it is a filter. We have a common pattern for doing filtering. It is called LINQ. There are three advantages by using LINQ in the code.

1. We use a common and easily recognizable implementation strategy. When people read `Where` they have a good idea as what is going on.
2. We are forced to separate out the "business logic" from the actual filtering (which is the reusable bit LINQ provides). 
3. Without LINQ, we are free to perform any kind of changes to the list. With LINQ we are tied into just filtering. 

This lock-in provides a lot of readability. The reader will never ponder *"what is going on in this loop, some advanced mutation I bet"*. With LINQ we have less freedom as to what we can do - ultimately  liberatting many potential detours in the readers mind. 

The result is 

```C#
List<SanitiedFieldInfo> Filter(List<SanitiedFieldInfo> fields) 
{
    return fields.Where(IsPublic).ToList();
}

bool IsPublic(SanitiedFieldInfo field) 
{
    switch (field.FieldInfo.MemberType) 
    {
        case MemberTypes.Field:
            if ((field.FieldInfo as FieldInfo).IsPublic)
                return true;
            break;
        case MemberTypes.Property:
            var propertyInfo = (PropertyInfo)field.FieldInfo;
            if (propertyInfo.GetGetMethod(false) != null)
                return true;
            break;
        default:
          throw new ArgumentOutOfRangeException();
    }
    return false;
}
```

## 7. Inline method

Finally, we realize that the `Filter()` is a simple call to `Where` and hence we might as well apply the refactoring pattern "In-line method".

The final result is 
    
```C#
public List<SanitiedFieldInfo> GetFields(Type type) 
{
    var fields = new HarvestHelper().GetFields(type);

    return fields.Where(IsPublic).ToList();
}

bool IsPublic(SanitiedFieldInfo field) 
{
    switch (field.FieldInfo.MemberType) 
    {
        case MemberTypes.Field:
            if ((field.FieldInfo as FieldInfo).IsPublic)
                return true;
            break;
        case MemberTypes.Property:
            var propertyInfo = (PropertyInfo)field.FieldInfo;
            if (propertyInfo.GetGetMethod(false) != null)
                return true;
            break;
        default:
            throw new ArgumentOutOfRangeException();
    }
    return false;
}
```


## 8. Conclusions
The article in summary form

* By step-wise refining the code, we make clearer the intentions of the code. 
* By constantly separating code into smaller parts we reduce the possibility of mistakes and enable reuse of code. 
* We always cease the chance to in-line methods when we realize we have created too small methods.
* By using LINQ we boost readability.

Notice that we did not jump directly from the first version of the code to the last. By doing simple step-wise refinements, we ensure that each step is safe. Preferably, we take each step with automated tests as the safety net.

I hope you are inspired to do safe refactorings the next time you have to understand a tangled piece of code. Simple refactorings like these do not take a lot of time, and they make life for you as your understanding shapes the code, and it makes it easier to next reader too. So everyone benefits.

A point we haven't touched upon is how distributed version control systems directly support this way of working. Every time we have completed a small step, we can issue a commit. Then, before pushing, we can combine the change sets into one. See `git reset --soft head~xx` or `hg combine xx` for further information on combining commits.


<SocialShareButtons>
</SocialShareButtons> 


## 9. Comments

<CommentText>
</CommentText>

<br>
Read the [Introduction](https://github.com/kbilsted/CodeQualityAndReadability/blob/master/README.md) or browse the rest [of the site](https://github.com/kbilsted/CodeQualityAndReadability/blob/master/AllArticles.md)

