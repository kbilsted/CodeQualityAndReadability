# Prefer declarative code over imperative code - building a command line parser in 5 lines of code
*Author: Kasper B. Graversen*
<ArticleHeaderUrls/>
<Categories Tags="Design, KBGit, Declarative_Programming, Imperative_Programming, Coding_Guideline, Code_Readability">
</Categories>


*Declarative code has many advantages over imperative code. The code is simpler code due to a good separations of concerns. The "what" is cleanly separated from the "how". Further, the declarations may find other purposes such as automatic consistent documentation.*


Please show your support by sharing and voting:

<SocialShareButtons>
</SocialShareButtons>


Table of Content

   * [Requirements for our command line parser](#requirements-for-our-command-line-parser)
   * [The imperative approach](#the-imperative-approach)
   * [The declarative approach](#the-declarative-approach)
   * [Conclusion](#conclusion)
   

One of my pet projects is to implement a working Git clone in just 500 lines of code (See [KBGit on Github](https://github.com/kbilsted/KBGit) for more details). For that I need a command line parser. Given the fairly limited line budget, I need something short and sweet... let's build a command line parser in vey few lines of code!    


## Requirements for our command line parser
Our requirements are straight forward. 

  1. We need to parse a set of pre-defined sentence such as `git log`. 
  2. A sentence may leave room for additional information such as a commit message like `git commit -m "user input here"`. 
  3. After successfully parsing a sentence, we need to invoke specific parts of the git-implementation. E.g. if the user types "git log" we shall invoke the `log()` method.
  4. If a sentence cannot be matched, print a help-message detailing parseable sentences.

  
## The imperative approach

Initially, I thought the smallest implementation was an *imperative approach*. E.g.

```
    if (args.length == 1 && args[0] == "log")
        return git.Log();
    if (args.length == 3 && args[0] == "commit" && args[1] == "-m")
        return git.Commit(args[2]);
    if (...)
    else
    {
        Console.WriteLine(@"Cannot parse input");
        Help
        git log                 for logging
        git commit -m <message> for committing
        ...");
    }
```
        
I bet you have seen plenty of code like this. Often when I encounter a wall of code like this, I cannot help but play out in my head, times toll on the code. Pressure to deliver, or perhaps lack of knowledge of a better way. And perhaps it all started out as a single if-else..then.. slowly over time.. turning into a monstrosity.

Aesthetics aside, there are a few downsides to this approach:

* Given enough if-else branching the code readability quickly deteriorate.
* First, there is no ties between the documentation and what is parsed. The code may quickly go out of sync with the printed documentation. 
* Second, for every sentence we can parse, we spend a line of code for the `if`-statement, a line of code for the invocation, and then a line for printing the help. 
* Finally, the approach does not lend itself very well to extra features such as optional arguments, or allow arguments in random order. 

        
## The declarative approach

A declarative approach operates on a more formal grammar and has a general matching algorithm applied to all grammar lines in search of a match.  It turns out we can write a declarative parser in only 5 lines of code! In addition to the parser is a line of code declaring the grammar for each sentence to match.

Let's first have a look at the parser:

```
// declarative parser
var matches = Config
	.Where(x => x.grammar.Length == cmdParams.Length)
	.SingleOrDefault(x => x.grammar.Zip(cmdParams, (gramar, arg) => gramar.StartsWith("<") || gramar == arg).All(m => m));

if (matches.grammar == null)
	return $"KBGit Help\r\n----------\r\ngit {string.Join("\r\ngit ", Config.Select(x => $"{string.Join(" ", x.grammar),-34} - {x.explanation}."))}";

// using the parser
var valueFromInvokingTheGitFunction = matches.actionOnMatch(git, cmdParams);
```

So the basic idea is  

  * We operate on a set of pre-defined grammar lines, each specifying how to parse a sentence (`config`).
  * Given some input (from the command line) `cmdParams`, match it with any grammar lines who specify the same number "words" to be parsed. 
  * Then, `Zip()` the grammar with the user input. Zipping means taking one element at a time from the user input and the grammar respectively. 
    * To denote "holes" in the grammar where the user can supply any kind of information, grammar elements starting with `<` are skipped during the zipping. 
    * If all zipped elements match (the `All(m => m)`) we have a match. 
  * If we haven't found a match, print wall of text of the commands that can be understood. The beauty here is that the help prints *the same data the parser operates on*. That way the documentation never goes out of sync with the code. If we have a match, we invoke a function pointer with the parameters from the command line. 

The only thing left to explain, is the grammar lines. Below are two examples. Each grammar line consists of three parts. A readable explanation, the sentence to parse and finally, the code to invoke on a match. We take advantage of the named tuple feature of C# here:

```
(string explanation, string[] grammar, Func<KBGit, string[], string> actionOnMatch)[] Config =
{
    ("Show the commit log", new[] { "log"}, (git, args) => git.Log()),
    ("Make a commit", new[] { "commit", "-m", "<message>"}, (git, args) => { git.Commit(args[2], "author", DateTime.Now); }),
}   
```

If you don't think about it, you almost don't see it. The grammar is quite readable. The grammar simply is `"commit", "-m", "<message>"` !


## Conclusion

The declarative implementation is a bit more advanced than the imperative implementation, but has a number of advantages. 

1. The grammar is very readble and is not concerned with how a matching strategy is implemented.
2. Since the declaration of the grammar is separate from the actual matching, we can improve the parser over time without needing to change our grammar specification (the `Config` variable above). 
3. The parser operates on the grammar and the grammar is (coincidentally) readily printable as a help text. By printing the grammar as the documentation, *our documentation is never out of sync*.
4. Lastly, and perhaps only important to the KBGit implementation, it is less lines of code!

*I hope you feel inspired to do more declarative programming and less imperative programming in the future. :-)*

More articles on this topic

* [From imperative to declarative code using LINQ extension methods](FromImperativeToDeclarativeCodeUsingLINQ.html)


Please show your support by sharing and voting:
<SocialShareButtons>
</SocialShareButtons>



<br><br>
<CommentText>
</CommentText>

<br><br>

