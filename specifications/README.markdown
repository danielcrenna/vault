# Specifications

## The specification pattern for business logic in .NET

    PM> Install-Package Specifications

### Introduction

Business logic is hard. It changes so often, we find ourselves hunting down scraps of code across our applications looking for the missing link. Martin Fowler introduced us to the Specification pattern, which allows you to define a chunk of
business logic once, and then build new logic by composing it out of smaller parts; when one rule changes, you only change
one place in your code, and the effect automatically ripples through your application. 

I've been carrying this implementation of the pattern with me for years, and it's appeared in countless projects where I needed
to string logic together in a graceful way. Now, thanks to NuGet, I'll get five minutes of my life back every time I start a 
new project, and you can too.

### Features

* A well-tested, possibly sexy implementation of the [Specification pattern](http://en.wikipedia.org/wiki/Specification_pattern).
* Includes helper extension methods to make the pattern easier to read and execute (i.e. left-to-right, not right-to-left)

### Usage

First, you define your business rule component as an `ISpecification`; it's easiest if you inherit from the `SpecificationBase<T>` class,
or implement `ISpecification<T>` if that's how you roll. The predicate logic you define in the `IsSatisfiedBy` method will determine, at runtime,
if the example instance is valid based on the criteria.

```csharp
using Specifications;

internal class IsThePrettiestNarwhalInAllTheLand : SpecificationBase<Narwhal>
{
    public override bool IsSatisfiedBy(Narwhal instance)
    {
        return instance.HornLength > 100;
    }
}
```

Now that you have a portable logic block, you can call it at runtime to validate it.

```csharp
using Specifications;

var cletus = new Narwhal { Name = "Cletus", HornLength = 4 };
if(new IsThePrettiestNarwhalInAllTheLand().IsSatisfiedBy(cletus))
{
    Console.WriteLine("Darn tootin'!");
}
else
{
    Console.WriteLine("Awwwww, dang!");
}
```

The power of specifications is the ability to compose them. When specifications are
composed out of multiple logic blocks, change in code is minimal when only some aspects
of a business rule change.

```csharp
using Specifications;

var clara = new Narwhal { Name = "Clara", HornLength = 400 };

var prettiest = new IsThePrettiestNarwhalInAllTheLand(); // ISpecification
var rebel = new PlaysByHisOrHerOwnRules();               // ISpecification                                
var takesShitFromAnyone = new TakesShitFromAnyone();     // ISpecification

ISpecification queenhood = prettiest.And(rebel).And(takesShitFromAnyone.Not());

if(queenhood.IsSatisfiedBy(clara))
{
    Console.WriteLine("All hail Queen Clara!");
}
```

Now, if the citizens of Narwhalia reprioritized their royalty status, you'd only have
to change the code in one place, and not go on a wild monoceros chase.

```csharp
ISpecification isQueen = prettiest.And(rebel).And(takesShitFromAnyone.Not()).And(hasNobleBloodLine);
```

If you've noticed that it's pretty awkward to write `new Specification().IsSatisfiedBy(instance)`, you're not
alone; that's why there's an extension method you can opt for that makes writing specification code less
backwards-y. The extension method uses some mild public reflection, so don't use it if you need to count your
milliseconds.

```csharp
using Specifications;
using Specifications.Extensions;

if(cletus.Satisfies<AllSnaggleToothedAndWhatNot>())
{
    Console.WriteLine("Yeeeeeew!");
}
```