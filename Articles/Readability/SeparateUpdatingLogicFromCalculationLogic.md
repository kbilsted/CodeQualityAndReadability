# Separate updating logic from calculations
*Author: Kasper B. Graversen*
<ArticleHeaderUrls/><Categories Tags="Code_Readability, Coding_Guideline, Refactoring, Functional_core_Imperative_shell">
</Categories>

*The readability of code dramatically improves, when the logic for updating state is separated from the calculations that provide the state changes. We show how both the updating and the calculation code is simplified when. While the principles of separation are generally applicable, we focus on methods updating state.*

Please show your support by sharing and voting:
<SocialShareButtons>
</SocialShareButtons>

<img src="img/chameleon-885595_640.jpg">


Table of Content

   * [1. Introduction](#introduction)
   * [2. A typical updating method](#a-typical-updating-method)
   * [3. The updating method after refactoring](#the-updating-method-after-refactoring)
   * [4. The extracted calculations](#the-extracted-calculations)
     * [4.1 The Benefits class](#the-benefits-class)
     * [4.2 The extracted CalculateBenefits](#the-extracted-calculatebenefits)
   * [5. Perspective](#perspective)
   * [6. Summary](#summary)

   
   
## 1. Introduction

Generally, methods with many responsibilities benefit from being split up in smaller parts. For this article, though, we focus on methods that update state. Update state? Yes. In methods that update state, I often see a mixture of simple assignments and computation whose results are used in assignments. It is very easy to persuade oneself, that a bit of computation does not hurt. In programming books, classes and flow is always made to look easy. Small examples that caters for easy overview. In the real world, we operate with classes containing many fields. 20 or 30 are not uncommon. And when updating fields in that number, we surpass the 5-9 number of things that our minds are capable of holding focus on concurrently. *Needles to say, we are loading work onto the brain and we easily loose overview.*


## 2. A typical updating method 

Let's have a look at an example from real life. Although the code is in English, it seems more like Chinese if you don't have business knowledge! **Don't get bogged down on the details of the code, just glance over it**. A concrete understanding matters not for our discussion. **The main take-away of the code is that state is updated based on some values.** 

The code in all its glory.

```
public void Replace(Product productData, ProductReplacementInfo replacement)
{
	var target = replacement.Product;
	var replacer = replacement.ReplacedByProduct;
	target.ReplacedCoverage = target.Coverage;
	target.ReplacedIsAdditionalPremium = target.IsAdditionalPremium;
	target.ReplacedBenefitCalculationMethod = target.BenefitCalculationMethod;
	target.ReplacedExternalPolicyNumber = null;

	decimal benefitFixed = replacer.Benefit.FixedAmount.IsDefined
		? replacer.Benefit.FixedAmount.Value
		: 0;
	decimal benefitPct = replacer.Benefit.PercentOfSalary.IsDefined
		? replacer.Benefit.PercentOfSalary.Value
		: 0;

	if (target.ReplacedBenefitCalculationMethod == replacer.Benefit.CalculationMethod)
	{
		target.TargetBenefitFixedAmount = benefitFixed;
		target.TargetBenefitPercentOfSalary = benefitPct;
	}
	else
	{
		// from fixed to %
		if (target.ReplacedBenefitCalculationMethod == BenefitCalculationMethod.FixedAmount
			&& target.BenefitCalculationMethod == BenefitCalculationMethod.PercentageOfSalary)
		{
			target.TargetBenefitFixedAmount = 0;
			target.TargetBenefitPercentOfSalary =
				(int)(100 * target.TargetBenefitFixedAmount / productData.BeneficialSalary);
		}
		else
		{
			// from % to fixed
			if (target.ReplacedBenefitCalculationMethod == BenefitCalculationMethod.PercentageOfSalary
				&& target.BenefitCalculationMethod == BenefitCalculationMethod.FixedAmount)
			{
				target.TargetBenefitFixedAmount = (int)(productData.BeneficialSalary * (benefitPct / 100));
				target.TargetBenefitPercentOfSalary = 0;
			}
			else
			{
				throw new Exception($"Cannot convert {target.BenefitCalculationMethod} -> {target.ReplacedBenefitCalculationMethod}"));
			}
		}
	}
	
	reasonRepository.AddChangeReason(RiskCoverageChangeReason.PensionSchemeChange);
	target.Coverage = replacer.Info.CoverageId;
	target.IsAdditionalPremium = replacer.Info.IsAdditionalPremium;
	replacement.Product.ExtraInfo.PresenceInPensionScheme = CoveragePresenceInPensionScheme.Replaced;
}
```
 
Here are the problems I have with the code

  * Assignment of state is a mix of values and complex calculations. 
  * It is not clear that all fields are set, e.g. `TargetBenefitFixedAmount` and `TargetBenefitPercentOfSalary` 
  * The many if-else branching inhibits readability to the extend that comments were used as a separator.
  * Some values must be rounded to whole numbers (they are stored in `int`'s). This rounding logic is intermixed with the assignments.
  * The helper variables `benefitFixed` and `benefitPct` are unnecessarily visible.

 

## 3. The updating method after refactoring

Let's have a look at the code after having extracted away the calculation bits into a separate method.

Notice the overview of the code that was achieved. We can easily follow which fields are updated, and all the clutter with regards to rounding etc. is hidden away. 


``` 
public void Replace(Product productData, ProductReplacementInfo replacement)
{
	var target = replacement.Product;
	var replacer = replacement.ReplacedByProduct;
	target.ReplacedCoverage = target.Coverage;
	target.ReplacedIsAdditionalPremium = target.IsAdditionalPremium;
	target.ReplacedBenefitCalculationMethod = target.BenefitCalculationMethod;
	target.ReplacedExternalPolicyNumber = null;

	var benefits = CalculateBenefits(target, productData, replacer.Benefit.Value);
	target.TargetBenefitFixedAmount = benefits.FixedAmount;
	target.TargetBenefitPercentOfSalary = benefits.PercentOfSalary;

	reasonRepository.AddChangeReason(RiskCoverageChangeReasonEnum.PensionSchemeChange);
	target.Coverage = replacer.Info.CoverageId;
	target.IsAdditionalPremium = replacer.Info.IsAdditionalPremium;
	replacement.Product.ExtraInfo.PresenceInPensionScheme = CoveragePresenceInPensionScheme.Replaced;
}
``` 

The "magic" lies in the introduction of the `CalculateBenefits()`, which is a simple "extract method" refactoring. I'm not sure I'd leave the helper method inside the `Replace()` for long. In a future iteration of the code, I'd move it out and add a `benefits` parameter instead. That way `Replace()` has one less  responsibility.


## 4. The extracted calculations

Now that was a very happy story. But how about the extracted bits? How ugly are they? That is a good question. Let's have a look.


### 4.1 The Benefits class
Our `CalculateBenefits()` returns two results, which we wrap in a class of its own. We could have used a tuple instead, but I feel it quickly gets confusing due to the anonymous field names. A third approach is to use `out` parameters, but they often have a clumsy feel to them, especially if when the number of `out` parameters exceeds 2 or 3.

```
class Benefits
{
	public readonly decimal FixedAmount, PercentOfSalary;

	public Benefits(decimal fixedAmount, decimal percentOfSalary)
	{
		FixedAmount = fixedAmount;
		PercentOfSalary = percentOfSalary;
	}
}
```

It can be tempting to place the rounding logic inside this class. For now it is a simple cast to `int`, but in the future it could get more involved. Personally, though, I think I prefer placing the rounding logic with the calculation logic. This way the data class acts as a simple vessel for values. It also does not require any testing. 

I've also made the class immutable. Perhaps its a bit overkill compared to the use cases of the class, but it was easy to do.
 

 
### 4.2 The extracted CalculateBenefits

Here is the extracted code for the calculation. 

```
public Benefits CalculateBenefits(
	RiskCoverageInfo origin,
	Product productData
	IRiskCoverageBenefitAggregator replacer)
{
	decimal benefitFixed = replacer.FixedAmount.IsDefined
		? replacer.FixedAmount.Value
		: 0;
	decimal benefitPct = replacer.PercentOfSalary.IsDefined
		? replacer.PercentOfSalary.Value
		: 0;

	if (origin.BenefitCalculationMethod == replacer.CalculationMethod)
		return new Benefits((int) benefitFixed, (int) benefitPct);

	if (origin.BenefitCalculationMethod == BenefitCalculationMethod.FixedAmount
		&& replacer.CalculationMethod == BenefitCalculationMethod.PercentageOfSalary)
	{
		return new Benefits(0, (int) 100 * origin.TargetBenefitFixedAmount/productData.BeneficialSalary);
	}

	if (origin.BenefitCalculationMethod == BenefitCalculationMethod.PercentageOfSalary
		&& replacer.CalculationMethod == BenefitCalculationMethod.FixedAmount)
	{
		return new Benefits((int) productData.BeneficialSalary * (benefitPct / 100), 0);
	}

	throw new Exception($"Cannot convert {target.BenefitCalculationMethod} -> {target.ReplacedBenefitCalculationMethod}"));
}
```

It is worth noticing how much cleaner this code also has become. 
 
  * **Although the code still fairly long-winded, we now know all it does it return two simple values.**
  * Nesting is reduced by using `return` statements.
  * A large lump of code has been assigned a name and is nicely tugged away.
  * The variables `benefitFixed` and `benefitPct` are no longer available within `Replace()`.


  
## 5. Perspective

**We have shown the power of extracting methods. It is much more than a simple textural reshuffling of the code. We ended up with two pieces of code both of which were significantly simpler.** We could end the story here, but I think there is more to the story.

I think the great difference in readability was achieved by *what* we extracted. We did not simply cut the method in half. We extracted the *updating logic* from the *calculation logic*. It is this kind of separation that packs a punch. As we suggested above, the updating method should not invoke the calculation method, rather the updating method should take the values object as an argument. In turn, what we are implementing is a pattern known as ["functional core, imperative shell"](<BaseUrl/>Tags/Functional_core_Imperative_shell.html).

Finally, I've made the `CalculateBenefits` method `public`. Hopefully this is a bit thought-provoking. Ordinarily one should think such a method is private to the replace implementation. But it really doesn't hurt that it is more visible so long the method does not change any state (it's simply returning new values). Anyone is free to call the method without breaking anything. We could stress this further by making it `static` if we wanted to. By making it more public we are making it easy to test the calculating (i.e. the difficult) bits of the updating separately. It is a light-weight alternative to wrapping the CalculateBenefits-method in a class which implement an interface `ICalculateBenefits`, and injecting that interface into the `Replace()` method.   


## 6. Summary

  * We have shown that the refactoring using "Extract method" may have a huge impact on readability.
  * We have given the perspective that helper methods may be public if it is beneficial and when they do not change state.

For more related articles, see <Categories Tags="Code_Readability, Coding_Guideline, Refactoring, Functional_core_Imperative_shell">
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

