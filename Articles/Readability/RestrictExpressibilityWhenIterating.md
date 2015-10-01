# Restrict expressibility when iterating 
*Author: Kasper B. Graversen*
<br>[[Introduction]](<BaseUrl/>) [[All categories]](<BaseUrl/>AllTags.html) [[All articles]](<BaseUrl/>AllArticles.html) [[Edit article <img src="http://firstclassthoughts.co.uk/img/edit.png"> ]](<GithubPageUrl/>)<br>
<Categories Tags="Code_Readability, Coding_Guideline, Looping, Iteration, goto, foreach, while, for, LINQ">
</Categories>

*The more restrictive the language mechanisms in use, the more readable the code tends to become. To solidify this claim, we show a number of approaches all implementing "iterate-and-collect logic" over a collection. We discuss each approach and its affects on readability. We then show how to express more complicated requirements when looping.*

Please show your support by sharing and voting:
<SocialShareButtons>
</SocialShareButtons>


<img src="img/staircase-209914_640.jpg">


Table of Content

   * [1. Introduction](#introduction)
   * [2. Simple iteration over all elements of a collection](#simple-iteration-over-all-elements-of-a-collection)
     * [2.1 The `goto` approach](#the--approach)
     * [2.2 The unbounded `while` approach](#the-unbounded--approach)
     * [2.3 The bounded `while` approach](#the-bounded--approach)
     * [2.4 The bounded `for` approach](#the-bounded--approach)
       * [2.4.1 The separated loop-body](#the-separated-loop-body)
     * [2.5 The enumerator approach](#the-enumerator-approach)
     * [2.6 The `foreach` approach](#the--approach)
     * [2.7 The LINQ approach](#the-linq-approach)
   * [3. Complicated iteration over some elements](#complicated-iteration-over-some-elements)
     * [3.1 Complex iteration](#complex-iteration)
     * [3.2 Separation of loop body and loop logic](#separation-of-loop-body-and-loop-logic)
       * [3.2.1 Delta or absolute position](#delta-or-absolute-position)
   * [4. Conclusions](#conclusions)
   
   
   

   
## 1. Introduction

What makes code readable? That is a very hard question to which many answers are correct. There are many facets to code readability, and that's what makes it interesting to discuss, study and so forth. *One* perspective is enforcing a coding style *of the least expressibility*. 

Iteration is a very general subject, but it's something we happen to do a lot in code. And quite interestingly, iterations can be performed in a lot of ways. This means you are bound to be exposed to virtually all approaches in a larger code base. 

When writing a program, you want to write it in such a fashion that future readers mentally are taken in a certain direction when they read the code. Preferably, readers are taken by their hands, and shown bit by bit, a complex task broken down in smaller steps. Using construct with *limited  expressibility* naturally leads the reader down a narrower garden path. With less expressive constructs or language mechanisms, the ability to introduce complexity is reduced. Less complex code involves less possibilities/choices, and hence is easier to overview. 

I happen to like narrow garden paths. It takes away my worries. Just like the picture above. In order to get from A to B, simply follow the stair case (to heaven?). When I need to fix a bug and I see the code is inside a loop-construct that is so extensive that it can not fit a screen, I get a little nervous divulging into the beast. And depending on which looping mechanism is used, the more things I tend to worry about. I cannot help but get a little paranoid. The looping, how is it done? Are all elements of an array being visited? Are some visited twice (by design - or by accident?) etc. 

We investigate 8 different ways to iterate over, and perform, some business logic to a collection. The examples gradually **decrease in expressibility**, while at the same time **increase in readability**. 

Then, we turn to more complex business rules where simple iteration does not suffice and show one way I've found to combat complexity.



## 2. Simple iteration over all elements of a collection

Our first section deals with iterating over a collection and performing an action on each element. Bear in mind, that the code examples have been distilled to their absolute minimum. Hence to understand that there really is a difference between the examples, you have to envision a code body of roughly a few pages resting inside each loop body.



### 2.1 The `goto` approach

To make my point clearer, we start in the extreme. While I have never met code like this - thank god - it illustrates nicely the very point of this article. So now, what does this code do?


```
public string Gotos(int[] numbers)
{
    string res = "";
    int i = 0;

    agn:
    res += numbers[i];
    i++;
    if (i == numbers.Length)
        goto stp;
    goto agn;

    stp:
    return res;
}
```

With `goto` we have the ultimate freedom. We can jump out of scopes in a completely different way than with `return` and `break`. We can skip multiple nested loops, and jump half up a method body. With `goto` we can simulate all looping constructs, and even `continue`,  `break` and `return`. Thankfully,  goto's have gotten a bad reputation and are not as common as they once were. I rarely use them, but there was a time (in the mid 1970s) where the war raged between "structured programming" (Dijkstra) and "go to programming" (where amongst others, D. E. Knuth showed situations where the use of gotos would be more advantageous). The use of `goto´'s was ingrained in developers back then. I wasn't really around in that sphere at the time, so I can only speculate. But I'll bet it having to do with performance (due to much slower hardware), and having to do with explicit memory allocation of the languages. 

Glancing down the code (while imagining it being several pages long), the structure does not easily reveal that we are dealing with a loop. There is no indented scope (`{}`-block). Purposely I named the labels badly to make it harder to read the small code snippet, to really make you feel the pain. You can substitute `agn` with `again` and `stp` with `stop` to make it a bit more readable.

Before I reveal what the method does, lets look at a more readable version.



### 2.2 The unbounded `while` approach
Replacing the labels and goto with an endless loop is a step in the right direction. Certainly now the keyword `while` outright reveals that something is happening more than once. But how many times are we doing something? Well, that will depend on the number of `break`, `continue` and nested branching structures within the while-body.

```
public string While(int[] numbers)
{
    string res = "";
    int i = 0;
    while (true)
    {
        res += numbers[i];
        i++;
        if (i == numbers.Length)
           break;
    }

    return res;
}
```

The structure of the code does not reveal whether we have started an endless process, for example a background worker thread, or we are performing some repetition a limited amount of times. Since we are just iterating a finite collection we are *misusing*, or the very least *misleading the reader*.  The `while`  is far more expressive a construct than what we need.

There is nothing wrong with using an endless loop - if you have logic requiring a such. In the scope of code that iterates, this is rarely the case. 



### 2.3 The bounded `while` approach

This next example is also expressing our logic using a `while` statement. Contrary to the unbounded version we now clearly state we iterate a number of times equal to the length of the `numbers` array. 

```
public string WhileWithBounds(int[] numbers)
{
    string res = "";
    int i = -1;
    while (i < numbers.Length - 1)
    {
        // move iteration reference(s)
        i++;

        // body
        res += numbers[i];
    }

    return res;
}
```


Notice how the *index reference* `i` starts from `-1` rather than  `0`. This is a technique to remedy the problem that arises, when a while body  contains several `continue` instructions: Each time you issue a `continue`, you must remember to first increment/decrement the iteration reference. By incrementing `i` at the top of the while-body, we ensure progress in each iteration, and thus preventing the code spiralling down into an endless loop. 

Another disadvantage of the while loop is that we have to define our "iteration reference" outside the scope of the while - and hence it can be re-used  further down the method. You should be careful with reusing variables if they serve different purpose. If for nothing else, then because it makes me uncomfortable. 





### 2.4 The bounded `for` approach

The `for` construct is the next example to dissect. The looping construct has a number of advantages over the `while` construct. Most importantly: `for` better describe our current situation. The intent of `for` is to express a limited iteration, whereas `while`'s intention is to express an unbound or dynamic bound iteration. Further more there are some more or less technical advantages:

* It allows us to define an iteration reference that is only visible within the scope of the loop-body
* It allows us to define the increment/decrement logic outside the body - no more weird indexes starting from `-1` as we saw above.

```
public string For(int[] numbers)
{
    string res = "";
    for (int i = 0; i < numbers.Length; i++)
    {
        res += numbers[i];
    }

    return res;
}
```

There are still problems with the code. There is nothing preventing the code from changing the value of `i` inside the for-body. For this short example this is not a problem, but recall our imaginary 2-page loop-body full of code! Ans what happens when we spot a change of `i` within the body? Then we have to figure out if this is intended or a bug. *The structure of the code does not reveal this intent*. A natural solution is to separate the looping logic from the looping body. We shall see that next.


#### 2.4.1 The separated loop-body

By extracting the loop body into a separate method, it becomes vividly clear to anyone, that we are in fact processing the elements of `numbers` on element at a time, from the first to the last element.

```
public string ForWithExtractMethod(int[] numbers)
{
    string res = "";
    for (int i = 0; i < numbers.Length; i++)
    {
        res = Concat(numbers[i], res);
    }

    return res;
}

string Concat(int number, string res)
{
    return res + number;
}
```

This same technique is just as useful when more complex business logic require us to e.g. skip elements. In section 3. we deal with this situation. 




### 2.5 The enumerator approach

Most collections in C# (and perhaps in your favourite language) has the ability to return an enumerator with which you can enumerate over the collection. Perhaps I have been sloppy with my use of words. And perhaps you have only notices just now, that I have interchanged iteration and enumeration. Just for the sake of completeness, let us explain the difference of meaning. Iteration is referred to the process of repeating a block of code, while enumeration means to go through all values in a collection of values. Enumerating, therefore, usually entail some form of iteration.

In C# this looks like

```  
public string Enumerator(int[] numbers)
{
    string res = "";
    var enumerator = numbers.GetEnumerator();
    while (enumerator.MoveNext())
    {
        var elem = enumerator.Current 
        res += elem;
    }

    return res;
}
```    

This approach is advantageous in that there is no longer an iteration reference (such as `i` in the above examples). Hence when we read this code, we *know* that we are going through the elements one at a time. The inner logic dictates this, so the code cannot digress. So why are enumerators so rarely used then if they are so cool. Perhaps because they are verbose compared to other language mechanisms. It takes a line of code to define the `while` and a line of code to read the currently enumerated element.

Going slightly off topic, implementing an enumerator can be surprisingly difficult. This stems from several situations that has to be catered for

* `.Current` may be access before `MoveNext()`
* `.Current` may be access anywhere from 0 to many times in a given enumeration step.


Anyhow, let's move on to the more delicious coding styles.


    
### 2.6 The `foreach` approach

The `foreach` is perhaps my favourite language construct when it comes to iteration. It is very succinct, it conveys its meaning, and like the enumeration approach it deals with elements of a collection without involving an index or iteration reference. 

```
public string Foreach(int[] numbers)
{
    string res = "";
    foreach (var number in numbers)
        res += number;

    return res;
}
```

On the other hand, the `foreach` is severely limited in what you can express with it. We cannot directly specify to only visit every other element, where with the `for` construct we can simply substitute the `i++` with `i+=2`. I take this as an *advantage*, rather than a disadvantage:

* Future readers need not spend mental energy in discovering what kind of looping that is to take place
* Future maintenance, or bug fixing cannot introduce mistakes in the iteration part of the logic

When I see the use of a `for` I can only think that there is a reason why the `foreach` was not used. And this makes me search the code for that very reason.
 If I don't see a reason why, I'll immediately refactor it to using the less expressive, but more readable `foreach`. 
  


### 2.7 The LINQ approach 

Now this final example is cheating a bit. Repeatedly, I've stated we need to imagine a lengthy loop-body in order for this discussion to carry much relevance. Of readability of really short loop bodies may be important too, since they easily see future growth.

If we for a second just imagine that the is no large looping body. There is only what we have shown so far. A simple "stringification" of an int-array. And assume for a moment we can live with the performance overhead of this kind of string concatenation rather than using say `StringBuilder`. *Then*, we can use an even less expressive construct found in LINQ. The `Aggregate` function operates over a set of values and accumulate them. Well, this is what we have been doing all along. So we can refactor our previous code into just 

```
public string Linq(int[] numbers)
{
    return numbers.Aggregate("", (res, number) => res + number);
}
```

Is this more readable? I would tend to think so. Of course it is more abstract to the person never having seen LINQ or `Aggregate` in action. But to those who have, we have ultimately locked ourselves down in terms of expressibility. We can only aggregate now. No detours allowed. Of course, LINQ does *not* always guarantee maximum readability! You can still create some horror story LINQ expression that make your eyes bleed. Code with conscience 

This actually brings forth an ever existing paradox. How do we define code readability? In terms of how easy the master programmer reads code, or how easy the less experienced programmer does? It's a many-faceted discussion, one that deserves more room than would be appropriate to allocate here. I have also touched upon this paradox in <BaseUrl/>TheChangingNotionOfReadability.html




## 3. Complicated iteration over some elements

Now its time to look at iteration with a bit more complicated logic. Let us assume the following business rules.

For a list of numbers return a string constructed from

* Of the pair-wise sum of numbers (a, b)
* Except for a-numbers divisible by 5, then skip this pair
* Except for b-numbers divisible by 2, then skip this and the next pair


probably this formalisation makes it all the more complicated. All I want here is some business logic requiring more than one element at a time while also require that under certain conditions, not all elements of the list are partaking in the answer.

The input: `1, 1, 2, 1, 1, 5, 1, 5`  
Yields: `2266`  

Sure those business rules are weird, but having to do more complex iteration is not uncommon. I do it all the time in the [StatePrinter](https://github.com/kbilsted/StatePrinter) project where we turn a stream of tokens into an output format (e.g. JSon, Xml or C#-like). I call these [output formatters](https://github.com/kbilsted/StatePrinter/tree/master/StatePrinter/OutputFormatters). 


### 3.1 Complex iteration

The first approach is a direct translation of the business requirements into code

```
public string IterationSkippingSome(int[] numbers)
{
    string res = "";

    for (int i = 0; i < numbers.Length - 1; i++)
    {
        var current = numbers[i];
        if (current % 5 == 0)
            continue;

        var next = numbers[i + 1];
        if (next % 2 == 0)
        {
            i++;
        }
        else
        {
            res += current + next;
        }
    }
    return res;
}
```

We see that there are three different things happening. A `continue` that effectively short circuits an iteration. A `i++` that skips this and the following iteration. And finally a modification to `res`.

Effectively we now have multiple places `i` changes value, and the structure of the code does little to reveal the skipping inside the body of the loop.




### 3.2 Separation of loop body and loop logic

As we have shown before, there are benefits to separating methods into several methods if we find that they do too much. Especially, when the method is susceptible to splitting. In the following example we split the logic of iteration with that of the business logic.


```
public string IterationSkippingSomeExtracted(int[] numbers)
{
    string res = "";

    for (uint i = 0; i < numbers.Length - 1; i++)
    {
        uint delta = RestrictedPairwiseSum(numbers[i], numbers[i+1], ref res);
        i += delta;
    }

    return res;
}

uint RestrictedPairwiseSum(int current, int next, ref string res)
{
    if (current % 5 == 0)
        return 0;

    if (next % 2 == 0)
        return 1;

    res += current + next;
    return 0;
}
```

Here we have to either turn to `out` parameters or `ref` or returning a `Tuple<string, int>`. I chose to use `ref` - effectively making the string reference the same in both methods. If we were collecting values in a mutable object such as a `StringBuilder` we could do without the use of `ref`.

Rather than inserting a dynamic check that `delta` is non-negative, I instead used a type that cannot express negative numbers. Meet the `uint`. 
    
I see a couple of benefits here

* The looping logic is very transparent. You immediately understand why a `for` has been used rather than a `foreach`. You see that you are extracting pair-wise and that as a result of an application you may skip *forward* in the list.
* The `RestrictedPairwiseSum()` is expressed in terms close to the requirements. It doesn't care about the input format. Be it an array, a `List<>` or whatever.
* The `RestrictedPairwiseSum()` can easily be tested in isolation.


#### 3.2.1 Delta or absolute position

There is a design choice to be made when returning a value from the extracted method. We can choose between

* Returning a delta value
* Returning an absolute position
* Returning a *skip instruction* 

There are advantages to returning delta's in that the business logic then is independent of knowing too much about the structure of the input data. On the other hand, returning an absolute position may be required e.g. to signal a full-stop or skipping towards a new section of the data. A disadvantage of having to return an absolute position is that you need to pass into the business logic the current position, and possibly the last position.

A *skip instruction* is perhaps preferable when the skipping itself requires some logic. Imagine the input data being some structured data, e.g. a book containing chapters, which contains sections, etc. Perhaps from a section you want to skip until the next chapter. Expressing that as a delta value or an absolute position requires some extra scaffolding. Instead we can express our skipping behaviour as an enum and then react to the enum in the code that handles the looping rather than in looping body.

Here is how we could express the above example

```
enum SkippingBehaviour
{
    NoSkip, SkipNext
}

public string IterationSkippingSomeExtractedAndSkipLogic(int[] numbers)
{
    string res = "";

    for (int i = 0; i < numbers.Length - 1; i++)
    {
        var skipping = RestrictedPairwiseSum2(numbers[i], numbers[i + 1], ref res);
        i += GetDelta(skipping);
    }

    return res;
}

int GetDelta(SkippingBehaviour skipping)
{
    switch (skipping)
    {
        case SkippingBehaviour.NoSkip:
            return 0;
        case SkippingBehaviour.SkipNext:
            return 1;
    }
    return 0;
}

SkippingBehaviour RestrictedPairwiseSum2(int current, int next, ref string res)
{
    if (current % 5 == 0)
        return SkippingBehaviour.NoSkip;

    if (next % 2 == 0)
        return SkippingBehaviour.SkipNext;

    res += current + next;
    return SkippingBehaviour.NoSkip;
}

```

It is a bit more verbose, but the intent of the code is very clear. It also supports complicated skipping behaviour that can now be tested in isolation.  And by making the skipping behaviour a translation from an enum value to a delta, you can even imagine this translation to be pluggable. 



## 4. Conclusions

It is easy to dismiss this article with the argument that if you are used to code using `goto` all over the place, they are not at all unreadable. In fact, not using `goto` would break the common style of the code base. Habit truly is a significant factor when discussing code. But I think that we have established, that despite being used to `goto`'s (or not), being able to replace a construct like a `while` or `goto` with a `foreach`, and often even better, a LINQ expression, reduces the possible things that can be expressed in code, thus leading the reader down the correct path - Automatically making the code easier to understand. Just by looking at the first line he can determine eg. *"that we are just looping linearly over all elements"*. Simply due to the fact that with the lesser expressive constructs, there is no room to wiggle in the wrong direction. By design you cut yourself off a whole range of potential bugs.

We have argued for using LINQ over `foreach` where possible. We have argued for using `foreach` over `for` where possible. And we have argued for using `for` over `while` where possible and finally, only use `goto` when none of the others are suitable. 

To paraphrase a bit, we can boil it down to the following scale

| Typically more readable |           |             |       |         | Typically less readable |
|:-----------------------:|-----------|-------------|-------|---------|:-----------------------:|
| LINQ                    | `foreach` | enumeration | `for` | `while` | `goto`                  |

Of course, this does not hold water when in each iteration we need several elements of the array. Then it's more like


| Typically more readable |                 | Typically less readable |
|:-----------------------:|-----------------|:-----------------------:|
| `for`                   | `while`         |                  `goto` |

And when we are implementing an forever running background worker, we may have even further limits of choice with regards to implementation


| Typically more readable | Typically less readable |
|:-----------------------:|:-----------------------:|
| `while`                 | `goto`                  |


Finally, we have argued for using an intent-revealing structure when you are performing non-trivial iteration over a collection by separating the looping logic from the looping body.



Please show your support by sharing and voting:
<SocialShareButtons>
</SocialShareButtons>



<br><br>

<CommentText>
</CommentText>


<br>
<br>
<br>

