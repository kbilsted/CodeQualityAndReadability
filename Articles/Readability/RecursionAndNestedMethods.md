# Recursion and nested methods 

*Author: Kasper B. Graversen*
<ArticleHeaderUrls/><Categories Tags="Code_Readability, Coding_Guideline">
</Categories>

*Using recursion With the introduction of nested methods or "inner functions" to C# 7, we are able to do things not previously possible. Now there is a lot to say about a programming style with heavy 
use of nested methods (such usage is quite common in functional programming), but here, I'll focus on how be better can express recursion in our code.*

Please show your support by sharing and voting:
<SocialShareButtons>
</SocialShareButtons>



Table of Content

   * [Traditional implementation of recursion](#traditional-implementation-of-recursion)
   * [Recursion using nested methods](#recursion-using-nested-methods)
     * [Placement of the nested method](#placement-of-the-nested-method)
   * [What about Lambdas?](#what-about-lambdas?)
   * [Summary](#summary)

   
## Traditional implementation of recursion

With traditional C# or Java, recursion is often expressed using two methods. A public and a private, where the private is a helper method to the public. An example from my [KBGit project](https://github.com/kbilsted/KBGit) where I need to get all reachable nodes in a graph from a particular position within that graph.


        public List<KeyValuePair<Id, CommitNode>> GetReachableNodes(Id id)
		{
			var result = new List<KeyValuePair<Id, CommitNode>>();
			GetReachableNodes(id, result);
			return result;
		}

which sets up data and invokes the recursive helper method:

		private void GetReachableNodes(Id id, List<KeyValuePair<Id, CommitNode>> result)
		{
			var current = Hd.Commits[id];
			result.Add(new KeyValuePair<Id, CommitNode>(id, current));
			foreach (var parent in current.Parents)
			{
				GetReachableNodes(parent, result);
			}
		}

Now, of course there are other ways to define a recursive method, but the style with two methods is the one I've found the most comprehensible. Now there is one downside to this implementation strategy. We have two methods exposed to the class. Inside the class which of the `GetReachableNodes()` are we supposed to call? 

Another problem is the state propagation, parameters used in the public methods must be manually wired and defined again in the private method. Not a big problem, but a bit clumsy. Particularly when the number of arguments grow.. as they sometimes do. In our example, only the `id` parameter is repeated. 

This is a concrete example of the general problem that sometimes in order for a method to be more comprehensible, we split it into several methods, but really those methods we want no one to see except for that one method. Behold, nested methods is the answer.


## Recursion using nested methods
		
With nested methods we can in-line one `GetReachableNodes()` inside the other `GetReachableNodes()`, thus making the interface simple both internally and externally.


	public List<KeyValuePair<Id, CommitNode>> GetReachableNodes(Id id)
	{
		var result = new List<KeyValuePair<Id, CommitNode>>();
		GetReachableNodes(id);

		void GetReachableNodes(Id currentId)
		{
			var commit = Hd.Commits[currentId];
			result.Add(new KeyValuePair<Id, CommitNode>(currentId, commit));
			foreach (var parent in commit.Parents)
			{
				GetReachableNodes(parent);
			}
		}

		return result;
	}

We notice a few things

* We in-line the recursive methods, which is less clumsy than having two methods.
* Parameters of the recursive methods are minimized since the scope is shared.

### Placement of the nested method

Some languages such as ML and F# require the nested method/function to be declared before use. I find this requirement a bit annoying as it leads me to read code backwards. I have to start with the last line of the outer method and read from there since any nested-calls and the context for such calls will be declared here. For this short example it won't matter much but I think you can imagine this style scales badly with multiple methods or more code.

	public List<KeyValuePair<Id, CommitNode>> GetReachableNodes(Id id)
	{
		void GetReachableNodes(Id currentId)
		{
			var commit = Hd.Commits[currentId];
			result.Add(new KeyValuePair<Id, CommitNode>(currentId, commit));
			foreach (var parent in commit.Parents)
			{
				GetReachableNodes(parent);
			}
		}

		var result = new List<KeyValuePair<Id, CommitNode>>();
		GetReachableNodes(id);
		return result;
	}

I prefer to place the nested method near the first call to it or at the end of the outer method.


## What about Lambdas?

Nested methods are a good alternative to lambdas in cases such as this. 

	public List<KeyValuePair<Id, CommitNode>> GetReachableNodes(Id id)
	{
		Action<int> getReachableNodes = null;
		getReachableNodes = id => 
		[
			var commit = Hd.Commits[currentId];
			result.Add(new KeyValuePair<Id, CommitNode>(currentId, commit));
			foreach (var parent in commit.Parents)
			{
				getReachableNodes(parent);
			}
		}
		
		var result = new List<KeyValuePair<Id, CommitNode>>();
		getReachableNodes(id);
		return result;
	}
	

Remarks.
  * The lambda syntax is less clumsy, with the `Action<...>` syntax. 
  * It require a non-obvious "hack" in order for recursion to compile. The hack is first to define `getReachableNodes = null`, and then assign the code body. 
  * The closure of the lambda can reach the parameters of the outer method, which is nice.
  * Finally, the lambda require the `getReachableNodes` is defined and assigned a code body before usage.


  
## Summary

With nested methods we can better express our intent - internal functionality not to be used by other than one method. The in-lined method thus is more expressible and less clumsy than both a private helper method and the lambda syntax. It also allows the sharing of parameters and variables declared in the outer method.

But as with anything, if your nested methods are growing large, your a probably using them wrong. Or if you use a LOT of nested methods inside a method, you may find it hard to properly unit test it. 

As with anything, "don't overuse".


Please show your support by sharing and voting:
<SocialShareButtons>
</SocialShareButtons>



<br><br>

<CommentText>
</CommentText>


<br>
<br>
<br>