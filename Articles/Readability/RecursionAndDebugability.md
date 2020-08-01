# Debugability of recursive code

*Author: Kasper B. Graversen*
<ArticleHeaderUrls/><Categories Tags="Code_Readability, Coding_Guideline, SOLID, Refactoring">
</Categories>

*There are many ways in which you can express code with recursive functions. We look beyond the aestethics and turn to the debugability of the code*

Please show your support by sharing and voting:
<SocialShareButtons>
</SocialShareButtons>


Table of Content

   * [Intro](#intro)
   * [The functional approach](#the-functional-approach)
   * [Separate initial step from the rest of the steps](#separate-initial-step-from-the-rest-of-the-steps)
   * [Separation of concerns](#separation-of-concerns)
   * [Summary](#summary)

## Intro

When writing code, it is important for it to be readable. Often we judge short code more readable than longer code. At times this may *hurt* how debugable the code is. Its "debugability".

I think we need to make a concious trade-off between short code and debugable code. This is also why I shy away from very long chained LINQ statements.

> "Debugable code" is code that you can reason about the current state of input and output - perhaps even make it possible to change past or future computations as well. it is also code where you can insert conditional break points without bending over backwards.

At times I do smaller coding projects in my spare time. It allows me to think in context and experiment with code. One such project is a small implementation of GIT (the source revision control system). You can find it at https://github.com/kbilsted/KBGit. When implementing a  `git log` functionality showing the history of commits, I went ahead and did three implementations for contrast and comparrison.

If details are a bit hazy to you, the standard git log looks like

    $> git log
    commit f7ad4bdd31868bb8f057e1dda148a45281258e9c (HEAD -> master, origin/master, origin/HEAD)
    Author: Kasper B. Graversen <kbilsted@users.noreply.github.com>

        bugfix include index.md in article list

    commit 650ec01888f712076f33ea7c75c777259c09be46
    Author: Kasper B. Graversen <kbilsted@users.noreply.github.com>

        optimize css

    commit d9d86ad0eeb833b55a9a0a52d80d7c8a70c6d470
    Author: Kasper B. Graversen <kbilsted@users.noreply.github.com>

        Optimize file access

    commit 935b0a1790430b056486ca9d0ffe4a007f495f64
    Author: Kasper B. Graversen <kbilsted@users.noreply.github.com>
    ...

Given the commit history of GIT is a Directed Acyclic Graph, in order to print the log, we must recursevely traverse the DAG and visit each parent of each commit and append its output with the output of the commit. This way, we cater for both merging and branching along the commit history.

Let's look at each of the ways I implemented it. Quite possibly there exists other approaches to implementing this, if you feel you found an illustrative example, feel free to send a PR or raise a bug for discusion on github [https://github.com/kbilsted/CodeQualityAndReadability/](https://github.com/kbilsted/CodeQualityAndReadability/)


## The functional approach

This approach is a typical recursive coding strategy where we call ourselves until a stop-condition is met.

     public string Log_1(Id? id = null, HashSet<Id> seenBefore = null)
     {
        // stop conditions
        id ??= headHandling.ReadHead();
        if (id == null)
            return "";

        seenBefore ??= new HashSet<Id>();
        if (seenBefore.Contains(id))
            return "";
        seenBefore.Add(id);

        // logic
        var commit = Commit.Parse(objectDb.ReadObject(id));

        var ancestors = string.Join("\r\n", commit.Parents.Select(x => Log_1(x, seenBefore)));
        return $"commit {commit.Content}\r\nAuthor: {commit.Author}\r\n\r\n    {commit.CommitMessage}\r\n\r\n{ancestors}";
    }

From a debugability perspective, the problem lies in the `ancestor`variable assignment. This is the recursive bit. Ultimately, the state of the code is on the call stack, and not in some explicit local variable. 

This makes it impossible to go back one iteration and change stuff, or stop the debugger with a condition that some specific log has been processed.

Also, I don't like how the parameters are null for the initial call and the filled out by the recursive calls. It is not clear, that the parameters are only for internal use.


## Separate initial step from the rest of the steps
In the second implementation we split the code into the code that initializes the state and a private method for the logic. A `StrinbBuilder` is passed around explicitly, holding the processed state so far. This makes it easier to reason about the previous calculations and it makes it easier to set conditional break points base on that.

        public string Log_2()
        {
            var id = headHandling.ReadHead();
            if (id == null)
                return "";

            var sb = new StringBuilder();
            Log_2(sb, new HashSet<Id>(), id);

            return sb.ToString();
        }

        private void Log_2(StringBuilder sb, HashSet<Id> seenBefore, Id id)
        {
            if (seenBefore.Contains(id))
                return;
            seenBefore.Add(id);

            var commit = Commit.Parse(objectDb.ReadObject(id));

            sb.AppendLine($"commit {commit.Content}\r\nAuthor: {commit.Author}\r\n\r\n    {commit.CommitMessage}\r\n");

            commit.Parents.Each(x => Log_2(sb, seenBefore, x));
        }

As showed in [http://firstclassthoughts.co.uk/Articles/Readability/RecursionAndNestedMethods.html](http://firstclassthoughts.co.uk/Articles/Readability/RecursionAndNestedMethods.html) we can also experiment with hiding the private method even further by turning it into a nested method, making it impossible for other methods of the class to call it.



## Separation of concerns
With the advice taken straight out of the SOLID principles we separate the two concerns of the code. One concern is the traversal and picking up unique commit nodes. The other concern is the formatting of a textual represenation. 

Clearly there are two benefits reaped right of the bat. We get a simpler log-functionality, and we get a general purpose functionality for fetching all parent nodes of a commit. This functionality can is freely available (and useful) to the rest of the code base.

        public string Log_3()
        {
            var id = headHandling.ReadHead();
            if (id == null)
                return "";

            var ancestors = new List<Commit>();
            GetReachableNodes(id, new HashSet<Id>(), ancestors);
            
            return string.Join("\r\n", ancestors.Select(x => $"commit {x.Content}\r\nAuthor: {x.Author}\r\n\r\n    {x.CommitMessage}\r\n"));
        }

        public void GetReachableNodes(Id from, HashSet<Id> seenBefore, List<Commit> result)
        {
            if (seenBefore.Contains(id))
                return;
            seenBefore.Add(id);
            
            var commit = Commit.Parse(objectDb.ReadObject(id));

            result.Add(commit);

            commit.Parents.Each(x => GetReachableNodes(x, seenBefore,  result));
        }

I've written before about this in "Separate updating logic from calculations" ([http://firstclassthoughts.co.uk/Articles/Readability/SeparateUpdatingLogicFromCalculationLogic.html](http://firstclassthoughts.co.uk/Articles/Readability/SeparateUpdatingLogicFromCalculationLogic.html)) if you want more examples of this strategy and its benefits.

## Summary

When coding weigh in both readability and debugability. Through the article different implementation approaches were shown.

Recursion and readability is a large topic also covered in oher articles on the site see

<Categories Tags="Code_Readability, Coding_Guideline, SOLID, Refactoring">
</Categories>

Please show your support by sharing and voting:
<SocialShareButtons>
</SocialShareButtons>



<br><br>

<CommentText>
</CommentText>


<br>
<br>
<br>