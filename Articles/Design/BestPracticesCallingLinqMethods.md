draft
# Best practices calling LINQ extension methods

*LINQ extension methods are often easy to describe, and quickly you can built something that seems to work. But there *are* pitfalls that will bite you. This article describe the best practices you need to follow when writing LINQ extension methods.* 


# Introduction

Extension methods and LINQ are great inventions and makes it really nice writing C# code as it enables you to write more declarative code. However, there are some pitfalls when dealing with LINQ and the `IEnumerable<>` type in general. 




## LINQ syntax or chained method calls

* you can hover the mouse over to see the return type and F12 to jump to definition
* You can easier set break points
* You can easier introduce debugging assistant code to help visualize the flow of data

TODO sikre sig at det er korrekt

## Ensure we have evaluated the expression when defining it, or when receiving as method argument 

I Prefer ToArray() over ToList() - 

* since we get a collection we cannot add to - signalling that we are not touching the elements.
* Array has a slightly less overhead than List


## Avoid multiple iterations 

```
public void DoSomething(List<string> list)
{
    if (list.Any())
    {
        // ...
    }
    foreach (var item in list)
    {
        // ...
    }
}
```

or

```
var newYork = cities.Where(x.City == "New York";
var newYorkerRichars = people.Where(x => x.Name("Richard") && x.City = newYork));
```
 
