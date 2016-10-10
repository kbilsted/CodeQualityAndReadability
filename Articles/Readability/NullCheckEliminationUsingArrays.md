# Eliminate null-checks using arrays
*Author: Kasper B. Graversen*
<ArticleHeaderUrls/><Categories Tags="Null_Check_Elimination, Code_Readability, Design_Pattern">
</Categories>

*In this article, we show a coding pattern which eliminate the need for null-checking. since `null` typically is used to indicate that "nothing is here" - this is essentially the semantics of an empty array. The result is shorter and easier to read code.*

Please show your support by sharing and voting:
<SocialShareButtons>
</SocialShareButtons>


<img src="img/pixapay-819587_640_nullcheckeliminationusingarrays.jpg"><br>*(from https://pixabay.com/en/pay-digits-number-fill-series-819587/)*


Table of Content

   * [1. Introduction](#introduction)
   * [2. Null-checks elimination](#null-checks-elimination)
     * [2.1 Code before null-checks elimination](#code-before-null-checks-elimination)
     * [2.1 Code after null-checks elimination](#code-after-null-checks-elimination)
   * [3. Discussion](#discussion)
   * [4. Overhead](#overhead)
   * [5. Conclusions](#conclusions)

   
## 1. Introduction

One of the major problems with API's and business code alike, is the fact that methods can return `null`. This is problematic for a number of reasons.

  * It requires the caller to remember to check the result against null or things blow up
  * It makes the code more convoluted and is a factor in hiding the intent of the code
  * The null-checking leads to more branching of the code - in turn requiring more test cases to maintain test coverage numbers

In this article, I'll show [one of many techniques of eliminating the need for null-checks](<BaseUrl/Tags/Null_Check_Elimination.html). The main observation is that `null` is often returned from a method to denote that there is no result. This is equivalent to returning an empty array. Since the array is never null, there is no need to check against `null`.

Let's go through some production code that use this pattern and discuss the pro's and con's.



## 2. Null-checks elimination


### 2.1 Code before null-checks elimination

Consider the following code, an excerpt from some production code I once wrote. Based on an incoming event, it fetches a row from the database and updates it based on the event. Finally, the row is stored in the database again.

```
void Execute(Event event)
{
	var relation = FetchDbRelation(event);
	var modification = AdjustRelationToEvent(relation, event);
	if(modification != null)
		UpdateDbTable(modification);
}

Relation AdjustRelationToEvent(Relation relation, Event event);
{
	if(relation == null)
		return null;
		
	if(relation.Status != event.Status)
	{
		relation.Status = event.Status;
		return relation;
	}
	return null;
}

void UpdateDbTable(Relation modification)
{
	if(modification == null)
		return;
		
	sql = "Update ....";
	Dapper.Execute(sql, modification);
}
```

Notice the prominent place the null-checking has in the code. 

### 2.1 Code after null-checks elimination

When we utilize the empty array to denote "no work". Thus, for every null-check if determine if the semantics of the empty array is equivalent with the null. The value `null`can have many meanings, so it is not an entirely mechanical process. we can simplify the code to:

```
void Execute(Event event)
{
	var relations = FetchDbRelations(event);
	var modifications = AdjustRelationsToEvent(relations, event);
	UpdateDbTable(modifications);
}

Relation[] AdjustRelationsToEvent(Relation[] relations, Event event);
{
	var modification = new List<Modification>();
	foreach(var relation in relations)
	{
		relation.Status = event.Status;
		relation.Modified = true;
	}
	
	return relations.Where(x => Modified).ToArray();
}

void UpdateDbTable(Relation[] modifications)
{
	sql = "Update ....";
	Dapper.Execute(sql, modifications);
}
```

... and all the null-checks **are gone**.


## 3. Discussion
Let's reflect for a minute over the changes we've made.


**Smoothness**
Spend a minute or two re-reading the two examples of section 2. I hope you have come to appreciate how much smoother the flow of the code is. To be honest, I find the lack of null-checking quite liberating.


**Accustomation**
Naturally, this style of coding takes a bit of getting used to. You may find yourself and colleagues having reservations to wards this way of programming. I'm sure I wouldn't apply this pattern everywhere a variable may be null. But I've found that in code without side effects and code that "processes" data such as validation or mutation of input A to output B, the model fits well. Perhaps because in terms of semantics, there is little difference between processing one element and processing many elements one by one. 


**Readability**
When employing this pattern you may find some code becomes more readable, while other code does not. For instance, `AdjustRelationToEvent()` become `AdjustRelationsToEvent()` and is specified to be able to return several instances where only a maximum of one instance is ever returned. [Other strategies exists](<BaseUrl/>/Tags/Null_Check_Elimination.html) whose strategies better cater for revealing the intent of the method, for example the use of an "option/maybe type".



**Reusability**
The fact that you choose to operate on arrays, lists or sets, may open the door for reusability. For my example code (an extract of production code), 
it turned out that part of the code (the `FetchDbRelations()` and `UpdateDbTable()`) *was reused* for handling a related event. This event could potentially update several relations. 


## 4. Overhead

There is some overhead associated with the pattern.

  * We instantiate the empty array every time we want to return a null. In many languages arrays are immutable, and thus there is ample opportunity for caching and reusing such instances by the run-time. If the run-time does not provide such caching you can turn to an object-pool of your own.
  * We iterate arrays which is significantly slower than directly accessing a reference. For most of the business code that I've touched, this really isn't the performance issue. Typically, time is spent waiting for user input and reading/writing to databases/network.
  
Perhaps I've overlooked something essential. In that case, please let me know.



## 5. Conclusions

Using the semantics of an empty array to denote "there is no result" we have effectively eliminated all null-checking code of the example code.

The result is shorter and easier to read code, which may even be more reusable.

The strategy is [one of many existing strategies](<BaseUrl/>/Tags/Null_Check_Elimination.html) (link is work in progress) 


Please show your support by sharing and voting:
<SocialShareButtons>
</SocialShareButtons>



<br><br>
<CommentText>
</CommentText>

<br><br>