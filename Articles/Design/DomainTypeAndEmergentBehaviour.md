# Domain types creates readability ...and emergent behaviour
*Author: Kasper B. Graversen*
<br>[[Introduction]](<BaseUrl/>) [[All categories]](<BaseUrl/>AllTags.html) [[All articles]](<BaseUrl/>AllArticles.html) [[Edit article <img src="http://firstclassthoughts.co.uk/img/edit.png"> ]](<GithubPageUrl/>)<br>
<Categories Tags="Domain_Types, Emergent_Behaviour, Code_Readability">
</Categories>


*The focus of this article is how readability and maintainability is improved by replacing simple types with domain types. Consequences of introducing domain types is both a closer relationship between model and implementation, and that the domain types establishes a conceptual foundation making it easier to extend and adapt the application for future changes.*

Please show your support by sharing and voting: 
<SocialShareButtons>
</SocialShareButtons> 


<img src="img/httpspixabay.comdasommerfugl-insekt-oejne-sensor-376876.jpg">


Table of Content

   * [1. Describing the problem domain](#describing-the-problem-domain)
   * [2. Solution 1: Using simple types](#solution--using-simple-types)
     * [2.1 Problems with using the simple types.](#problems-with-using-the-simple-types)
   * [3. Solution 2: Domain types to the rescue](#solution--domain-types-to-the-rescue)
     * [3.1 The `Tag` class](#the--class)
     * [3.2 The `Page` class](#the--class)
     * [3.3 The `PageGenerator`](#the)
     * [3.4 The `TagCollection` class](#the--class)
     * [3.5 The `DocumentParser`](#the)
   * [4. Emergent behaviour](#emergent-behaviour)
     * [4.1 Bug fixing the link name creation](#bug-fixing-the-link-name-creation)
   * [5. Conclusions](#conclusions)
   
   
   


Our investigations take outset in implementing an application with and without domain types. The implementations are easily coparably, and thus provides ample  opportunity to reflect and compare. 

What is a domain type anyways?? A domain type is a type that cover the information or intent of a phenomenon in the *problem domain* (i.e. the real life "thing" we are creating software for). The domain type is expressed in the *solution domain* (i.e. our code). Some times, a domain type is a mere wrapper for say a `string` or an `int`.  This thought may be alien to you, maybe it even feels like a complete waste of time to do all that wrappin'. Allot me to show you differently. 



## 1. Describing the problem domain

We are constructing a small program to help assist in creating a static blog. Based on the content of the blog we need to generate navigational documents linking the blog entries together. 

We must read in a bunch of documents. Each document is parsed in order to locate one or more tags inserted into the document describing the content of the document. From the collection of  tags we mus generate a *"tag file"* for each tag, containing links to all documents with that tag. In addition, we must  generate a file containing links to all *"tag files"*. Finally, we must generate a file linking to all documents of the blog where the link text is the title of the document.

An analysis of our problem domain thus reveals the following *nouns*:

| Noun      | Description      |
| --------- | ---------------- |
| Tag       | A title          |
| Page      | A title and a path   |
| Document  | A text           |
| Document tag parser | Parsing documents and locating tags          |
| Page generator      | Generate html for the navigation pages        |
| File reader/writer  | A file-reader and a file-write         |
| File system document scanner | A scanner to recursively visit directories         |


Potentially each of these nouns are to be modelled by its own class. Rather than turning this into an academic exercise, let us investigate how a lot of people would implement this application. It s straight forward to concoct a solution using simple type already provided by our programming language. We call this *solution 1*. 

Then we tweak the code to use domain types while preserving the existing structure. We call this *solution 2*... I won't be holding my breath waiting for you to applaud the naming!


## 2. Solution 1: Using simple types

For the first implementation, we will be creating classes, but if possible we avoid modelling the domain. I know I'm the guy leading the pen, and so I can make up any nonsense I want. What I am trying to accomplish, though, is to stay true to the spirit of how many people code. 

Here's how we are going to implement the above model


| Noun      | Implementation |
| --------- | -------------- |
| Tag       | `string`       |
| Page      | `Tuple<string, string>`           |
| Document  | `string`       |
| Document tag parser | A class `DocumentParser` with a parse method           |
| Page generator      | A class with a method for each page to generate          |
| File reader/writer  | `System.IO.File`          |
| File system document scanner | `System.IO.DirectoryInfo`         |


Implementing this in a top-down fashion, makes us start at the document reading and parsing.  `GetTags()` just below traverses the directory structure and for each document reads it and asks a parser to identify the tags. The tags are then accumulated in the `tagsCollection` variable and returned.

```
public class DocumentParser
{
    public Dictionary<string, List<Tuple<string, string>>> GetTags(string rootFilePath)
    {
        var tagsCollection = new Dictionary<string, List<Tuple<string, string>>>();
        
        var di = new DirectoryInfo(rootFilePath);
        foreach (var path in di.EnumerateFiles("*.txt", SearchOption.AllDirectories))
        {
            var fileContent = File.ReadAllText(path.FullName);
            var tagsForPage = ParsePage(fileContent, path.FullName);
            
            foreach(var tagForPage in tagsForPage)
            {
              if(!tagsCollection.Contains(tagForPage.Key))
                   tagsCollection.Add(tagForPage.Key), new List<Tuple<string, string>>());
              tagsCollection[tagForPage.Key].AddRange(tagForPage.Value);
            }
        }

        return tagsCollection;
    }
```

The tags extraction is a private method of the class which uses some secret regex sauce, finds the relevant information

```
readonly Regex headerEx = new Regex("^#(?<title> .*)");
readonly Regex tagEx = new Regex("... (?<tagname> .*) .*");

Dictionary<string, List<Tuple<string, string>>> ParsePage(string pageContent, string fullName)
{
    Dictionary<string, List<Tuple<string, string>>> tagsToPages = 
        new Dictionary<string, List<Tuple<string, string>>>();
    
    Match headerMatch = headerEx.Match(pageContent);
    
    string title = headerMatch.Groups["title"].Value.Trim();

    foreach (Match match in tagEx.Matches(pageContent))
    {
        string tag = match.Groups["tagname"].Value;
        if(!tags.Contains(tag))
            tags.Add(tag, new List<Tuple<string, string>>());    

        tags[tag].AddRange(Tuple.Create(title, fullName));
    }

    return tags;
}
```

A site generator will first invoke the `DocumentParser`, then call a `PageGenerator` and with its output write to the file system. The implementation details are not important as it is a matter of simple traversals over our data-structure and generating HTML. The file writing is equally simple, just a call to `File.WriteText()`, and thus also omitted.

Instead, we focus on the type signatures of the page generator. Type signatures tells you  a lot about your code and readability in general.

```
public class PageGenerator
{
    public string GenerateAllTagsPage(List<string> tagToUrl)  { ... }

    public string GenerateAllArticlesPage(List<Tuple<string, string>> pages) { ... }

    public string GenerateTagPage(string tag, List<Tuple<string, string>> links) { ... }
}
```



### 2.1 Problems with using the simple types.

Did you trawl through all that code? If not, here is a summary of what we have experienced as problems so far. Feel free to revisit the code as you read along.

**Type parameter madness**  
By now, I hope you are growing a bit tired of type definitions like `Dictionary<string, List<Tuple<string, string>>>`. See, I always feel a headache is coming on, when I have to juggle around with such long-winded types whilst trying to understand the code that I did not write myself.  Constantly, I have to remind myself, what the data components are. Of course, naming is essential here, for example, `tags2pages` would be a typical name to describe the keys and values of a dictionary. 

**Skewed variable- and parameter-names**  
As code gets refactored or changed, the first thing to erode are the variable names. The types are ensured to match by the compiler. Thus, an important aspect of code readability and navigability, has to do with expressing intention through use of types.

**Gnarly interaction with the dictionary**  
The astute reader may have noticed another issue with the code. The maintenance of the `List<>` inside the dictionary. Each time we need to add an element to a dictionary, whose value is a collection of some kind, we need to initialize if absent and do a `AddRange` with out values.  It's not so much the same code block repeating itself (although it is a violation of the DRY principle), more, it has to do with how the code reads. Its clumsy and gnarly. You want to add an element, yet you are faced, again and again, with having to deal with initialization and adding to lists. Needless to say, this calls for an abstraction too. Let's call it a `TagCollection` and define it further below.





## 3. Solution 2: Domain types to the rescue

The difficult part in this application is not the algorithm with which to extract the tags or generate the HTML, it is the data structures. And those are only difficult to understand since we are using so many simple types, rather than defining types that encompass the *problem domain*. The traversal of files, parsing using regular expressions or generating HTML is all trivial. Of course, this is not the case for all types of applications and problem domains, but I have yet to see an application that is harder to read when domain types have been used.

The "Solution 1" implementation uses the type `string` to hold the following kinds of data

* File path
* Tag
* Page title
* Page url
* Document

Since the page contains two separate pieces of information, a `Tuple<string, string>` is used in representing the page (mapping `Item1` to the title and `Item2` to the page url).

The use of the `Tuple` class, I must admit, is a bit of a provocation. In my experience with code `Tuple` is actually not used that much, whereas `string` is almost always used for representing all kinds of information. I guess the lack of popularity on `Tuple`'s behalf may originate from the illegible properties `Item1` and `Item2`. It only takes ten minutes, or maybe a day, before you have completely forgotten what those anonymous properties hold. I have tried this, and changes are that so have you. This feeling of creepiness, the chilling down your spine, this is the very feeling you must recall whenever you see e.g. `string` or `int` holding all sorts of information within the same application.

Let us first define our domain classes, hereafter evaluate how the core implementation changes.



### 3.1 The `Tag` class

The `Tag` class is merely a wrapper for a string. While any string can be supplied to the constructor of `Tag`, the **intention**  of the code, is that it be fed only tag strings. For technical reasons uninteresting to this article, spaces must be replaced by `_` in the tag name. A validation that tags do not contain spaces would be an appropriate validation in the constructor. It is not always the case that we can add validation logic or add extra functionality to your domain classes. Do not feel bad when writing a simple wrapper class like below. Eventually evolution of the requirements - or refactorings - may find your domain type to be just the place for new code.


```
public class Tag : IEquatable<Tag>
{
    public readonly string Value;

    public Tag(string value)
    {
        Value = value;
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as Tag);
    }
    
    public override bool Equals(Tag other)
    {
        if (other == null) 
            return false;
        return string.Equals(Value, other);
    }

    public override string ToString()
    {
        return Value;
    }
}
```

Phew! 30 lines of code just to wrap a string! In order to cut the lines of code, it is tempting not to implement the `Equals()` and `GetHashCode()` methods. But hold back that urge! They are absolutely vital in our program, since tags are used as keys in dictionaries. In a-soon-to-be-release article, will explore the possibilities of building a reusable domain base class in order to significantly reduce boiler plate code as seen above. 



### 3.2 The `Page` class

Probably less disturbing to your nerves, is the replacement of  `Tuple<string, string>` with a domain class. The gut feeling many people have first introduced domain types, is that of doubt and reluctance. They fail to immediately see the benefits of wrapping just one value in a separate class. Most people, luckily, are more welcoming to the thought of wrapping two or more values in a class. At least it is a huge *readability boost* to access `.Title` rather than `.Item1`, and `.Url` rather than `.Item2`. As for the implementation, it is straight forward. 

```
public class Page : IEquatable<Page>
{
    public readonly string Title;
    public readonly string FilePath;

    public Page(string title, string filePath)
    {
        Title = title;
        FilePath = filePath;
    }

    public override int GetHashCode()
    {
        return Title.GetHashCode() ^ FilePath.GetHashCode();
    }

    public bool Equals(Page other)
    {
        if (other == null) 
            return false;
        return Title == other.Title && FilePath == other.FilePath;
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as Page);
    }
}
```

Similar to the `Tag` class it is verbose. Luckily the lines are very easily written. Since the Page is not used as a key in any collection, we could omit implementing the `GetHashCode` and `Equals` methods. I prefer to keep them. If ever we were to use instances of `Page` as a key, it would just work out of the box. And when omitted all sorts of suprising behavior and hard to find bugs arise. Also when unit testing (and not using [Stateprinter](https://github.com/kbilsted/StatePrinter/), it is very convenient to have the `Equals` so the `Assert.AreEquals()` will work :-). 




### 3.3 The `PageGenerator`

Before going into the code heavy `DocumentParser` re-implementation, let's first confirm that we are on the right track by evaluating how the `PageGenerator` changes from hieroglyphic-like to plain English readable code.


```
public class PageGenerator
{
    public string GenerateAllTagsPage(List<Tag> tags)  { ... }

    public string GenerateAllArticlesPage(List<Page> pages) { ... }

    public string GenerateTagPage(Tag tag, List<Page> pagesForTag) { ... }
}
```


<img src="img/httpspixabay.comdasommerfugl-insekt-oejne-sensor-376876.jpg" align="right" width="250">  
Looking at code how readable the code is now almost brings a tear to my eyes. It sure was worth those extra lines of boiler plate code! Comparing with the previous version of the page generator, code has been morphed from a scary looking caterpillar into a beautiful colourful butterfly.    
    


### 3.4 The `TagCollection` class

The last helper class is the `TagCollection` class. This class rectifies the reoccurring problem we had when wanting to add an element to our dictionary. Since the value of the dictionary is a `List<>` we need to ensure proper initialization on each insert. We *could* have made this a static method on our main class `DocumentParser`. I've seen this done many times over. However, this makes reuse harder in other situations, or make the code seem very artificial when accessing a DocumentParser class solely for the same of some dictionary util method.

```
public class TagCollection
{
    public readonly Dictionary<Tag, List<Page>> Tags = new Dictionary<Tag, List<Page>>();

    public void Add(Tag tag, params Page[] pages)
    {
        List<Page> values;
        if (!Tags.TryGetValue(tag, out values))
            Tags.Add(tag, values = new List<Page>());
        values.AddRange(pages);
    }

    public void Add(TagCollection collection)
    {
        foreach (var kv in collection.Tags)
            Add(kv.Key, kv.Value.ToArray());
    }
}
```
    
The implementation just wrap a dictionary, and makes it easy to add one or more pages. As an extra convenience  `Add` is overloaded to accept a `TagCollection`. 

Notice that the previous domain types were immutable, this wrapper is not. A long story can be spun over the theme of visibility and control. Rather than exposing the `Tags` field of the class, should the class itself implement `IEnumerable<KeyValuePair<Tag, List<Page>>` or `Tags` could expose the underlying data through a  `ReadOnlyDictionary`. Our implementation is derived from our needs. We are not making any attempts locking down the data structure. We are merely interested in making the dictionary interaction easy. So we are as mutable as the dictionary itself. Needles to say, other situations may require a different kind of wrapping over a collection type.
    
After having created my tag collection class, I writing about it here in this article, I could not help but thinking about the class from a generalization perspective. I quickly recognized, that the problem we are solving in `TagCollection` really applies to *any* dictionary holding a `List<>` as the value. Hence we can easily change the class into a `ManyValuesDictionary<TKey, TValue>` which wrap a  `Dictionary<TKey, List<TValue>>` and solves this problem once and for all. Having such an implementation at hand, we can consider if we want to create yet another wrapper for this called `TagCollection`, subclass it to get a `TagCollection` type or if we are content with using `ManyValuesDictionary<Tag, Page>`. It really depends on the size of your application and how wide-spread through out it is using a collection og tags.


As I leave it up to the reader to complete the class, I am absolutely certain, that such a reusable construct would **never** have evolved, had we settled with implementing a helper method for adding elements. Nor, god forbid, if we felt compelled to sticking with the initial implementation with the clumsy add-logic spread all over the code base.  You don't have to accept poor code like that!


     
    
### 3.5 The `DocumentParser`    

For the sake of brevity, let us look at only a subset of the `DocumentParser` re-implementation using domain types.

```
public class DocumentParser
{
    public TagCollection GetTags(string rootFilePath)
    {
        var tags = new TagCollection();
        var di = new DirectoryInfo(rootFilePath);
        foreach (var path in di.EnumerateFiles("*.txt", SearchOption.AllDirectories))
        {
            var fileContent = File.ReadAllText(path.FullName);
            var tagsForPage = ParsePage(fileContent, path.FullName);
            tags.Add(tagsForPage);
        }

        return tags;
    }
}
```

We make the following observations

* The `tags` variable now has a type of its own. 
* The add-functionality is a simple one-liner call to an `Add()` method.



## 4. Emergent behaviour

Even for a small example such as this, going through the exercise, I have had **two** situations of emergent behaviour. The class definitions add words to my vocabulary, and it sets my mind free to do evolve already existing definitions. I believe this to be fully equivalent to the  linguistic findings in the human sciences. There many believe, that one cannot think without a language. The more words you have the better are your capabilities for abstract thought and synthesis of new systems, language and thoughts.

Earlier we saw the emergence of the `ManyValuesDictionary`. Now we will see emergence from a different perspective.



### 4.1 Bug fixing the link name creation

As mentioned earlier, the tag names are defined with an `_` substituting spaces. Our naïve HTML generation created links by the following code

```
stringBuilder.AppendFormat("* <a href='Tags/{0}.html'>{0}</a>", tag.Value);
```

effectively rendering the links with `_` separating words in the tag, rather space which is  more aesthetic to the eye. We can easily fix this  bug with a needlestick type of change - local and not disturbing anything else. 


```
stringBuilder.AppendFormat("* <a href='Tags/{0}.html'>{1}</a>", tag.Value, tag.Value.Replace("_", " "));
```

It works, but it is not elegant. And certainly, it has nothing to do with OOP. So what are the alternatives? We could extract the code into a method on the generator.


```
stringBuilder.AppendFormat("* <a href='Tags/{0}.html'>{1}</a>", tag.Value, RinseForLinkText(tag.Value));
...
private string RinseForLinkText(string s) 
{ 
    return s.Replace("_", " ") 
}
```

This is already an improvement. But we can do better. The `Tag` class is the natural place to put this method, or maybe expose as a property.

```
class Tag
{
    public string Value;
    public string LinkText { get { return Value.Replace("_", " "); } } 
}
```

with using the property

```
stringBuilder.AppendFormat("* <a href='Tags/{0}.html'>{1}</a>", tag.Value, tag.LinkText);
```

There are several observations to be made here 

* The `Tag` class was a *natural place* to extend with the needed behaviour. It is a natural extension of our "vocabulary".
* The code is more readable, we are simply stating `LinkText` rather than focusing on exactly what that means.

That being said, in the general case, there is a dichotomy between the "single responsibility principle" and OOP. The traditional OOP prescribe that an object knows how to fetch and store itself, how to present itself on a GUI etc. The single responsibility principle, prescribes the opposite and would advocate functionality on the html generator dealing with formatting html.



## 5. Conclusions

* There is no running away from the fact, that the code has become longer. I don't mind that, the extra code is hidden in external files and the extra lines of code are very easy to spew out.
* With the new domain types, we get improved readability. Code is transformed from hieroglyph-like incantation to almost plain English prose
* The domain types enable the "jump to definition" functionality in your IDE. On the domain types you now have a "go to" place for extra functionality, validations, heck, you can even sprinkle with documentation
* Despite the initial judgement call, where we deemed an abstraction over the file system  "too academic", eventually I did  create said abstraction. A `FileRepository` replacing  calls to  `DirectoryInfo` and `File`. This abstraction enabled us to do unit testing without hitting the hard drive and needing to clean up files. I guess, some times, unit testing can drive you to do domain types as well :-)


Please show your support by sharing and voting: <SocialShareButtons>

</SocialShareButtons> 


<br><br>
<CommentText>
</CommentText>

<br><br>