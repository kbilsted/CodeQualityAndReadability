# Explicit and implicit interface implementations in F#
*Author: Kasper B. Graversen*
<br>[[Introduction]](<BaseUrl/>) [[All categories]](<BaseUrl/>AllTags.html) [[All articles]](<BaseUrl/>AllArticles.html) [[Edit article <img src="http://firstclassthoughts.co.uk/img/edit.png"> ]](<GithubPageUrl/>)<br>

<Categories Tags="F#, FSharp">
</Categories>

*F# has support for implementing interfaces defined in other .Net languages, e.g. C#. In this article we show that there are two ways of implementing an interface.*

Please show your support by sharing and voting:

<SocialShareButtons>
</SocialShareButtons>

Table of Content

   * [1. Explicit interface implementation in FSharp](#explicit-interface-implementation-in-fsharp)
   * [2. Implicit interface implementation in FSharp](#implicit-interface-implementation-in-fsharp)
   * [3. Investigating the underlying code generation](#investigating-the-underlying-code-generation)
   
   
   
In C# there are two ways to implement an interface - implicitly and explicitly. When search the web for *"F# implement interface"* you will be shown that there is only explicit interface implementations. But actually, you can implement the interface in such a way that it acts as a implicit interface implementation. Knowing both approaches may be important when integrating your F# code with say a C# framework.

First lets recap what it means to explicitly implement an interface. The idea is that you need to explicitly cast an object reference to the interface type, before you can access its properties. Implicit interface implementation is when you are not required to cast the reference before you access the properties of the interface. The explicit interface implementation disambiguates situations where a class implements several interfaces which accidently have methods with signature. E.g. the method `Draw()` and the interfaces `IDrawable` and `ICowboy`, We can't just call `Draw()` in a meaningful way and have both pieces of code execute. 

Now let's turn to F#.

## 1. Explicit interface implementation in FSharp

When searching the net for how to implement interfaces in F# you will be shown something along the lines of:

```
type Cowboy () =
    interface ICowboy with
        member self.Draw () =
            printfn "bang bang!"
```


Here we define a `Cowboy` implementing `ICowboy`.  We can instantiate an instance `lucky1`, but we cannot call `Draw`


``` 
let luckyLuke = (new Cowboy())
// luckyLuke.Draw() <-- NOT Possible
```

but if we cast our reference we can 

```
let luckyLuke = (new Cowboy() :> ICowboy)
luckyLuke.Draw()
```




## 2. Implicit interface implementation in FSharp
An alternative way to implementing interfaces is to define the functions of your interface in your type directly, and then implement the interface by pointing to the already defined functions. This looks like this

```
type Cowboy () =
    member self.Draw () =
            printfn "bang bang!"

    interface ICowboy with
        member self.Draw () = self.Draw ()
```

This implementation strategy, although slightly more verbose, have a number of advantages. Firstly, that you can now access `Draw()` without casting your reference, and secondly, the code that is generated differs. This may be important when inter-operating with C# libraries.


``` 
let luckyLuke = (new Cowboy())
luckyLuke.Draw() // <-- Possible
```

 
## 3. Investigating the underlying code generation

I discovered the alternative way of implementing interfaces the hard way. I was using a C# library taking `ICowboy`'s as arguments. The library would then locate the `Draw()` using reflection. Specifically using the `GetMethods()` method. Let's look at the methods defined in the type.


**Explicit interface implementation**

```
typeof(Cowboy).GetMethods()
```

yields

```
{System.Reflection.MethodInfo[4]}
    [0]: {System.String ToString()}
    [1]: {Boolean Equals(System.Object)}
    [2]: {Int32 GetHashCode()}
    [3]: {System.Type GetType()}
```


**Implicit interface implementation**

```
typeof(Cowboy).GetMethods()
```
yields

```
{System.Reflection.MethodInfo[5]}
    [0]: {Void Draw()}
    [1]: {System.String ToString()}
    [2]: {Boolean Equals(System.Object)}
    [3]: {Int32 GetHashCode()}
    [4]: {System.Type GetType()}
```


As you can see with the explicit implementation, we cannot see our `Draw()`. Alternatively, we can use the `GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)` which will return the `Draw()` for both interface implementation strategies.



Please show your support by sharing and voting:
<SocialShareButtons>
</SocialShareButtons>



<br><br>
<CommentText>
</CommentText>

<br><br>
