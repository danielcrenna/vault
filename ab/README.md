ab
==

* A split testing micro-framework for ASP.NET MVC, inspired by vanity *

Introduction
------------

In a post-agile world, we are asked to look beyond the technologies that enable our practice, and find ways to ensure the choices we make are informed by
customers and stand up to reality. Experiment-driven (or evidence-based) development is a way of combining run-time metrics with automated experiments, 
resulting in software that is “natural”, based on actual use and runtime performance rather than the strongest opinion.

This library fulfills the automated experiments aspect for practicing EDD in a .NET development environment.
If you're looking for the run-time metrics aspect of EDD, you can use [metrics-net](https://github.com/danielcrenna/metrics-net).
AB integrates metrics-net automatically, so any metrics you're tracking from that library are automatically displayed in AB's metrics
dashboard.

Requirements
------------
* .NET 4.0

How To Use
----------
**First**, specify AB as a dependency:

```powershell
PM> Install-Package AB
```

**Second**, define your experiments:

```csharp
using ab;

public class ExperimentConfig
{
    public static void Register()
    {
        Experiments.Register(
            name: "Jokes on link", 
            description: "Testing to prove that more people will click the link if there's a joke on it.",
            metrics: new [] { "Button clicks" },                             // Associates ticks against the "Button clicks" counter with this experiment
            alternatives: new object[] { false, true },                      // Optional experiment alternatives; default is common "A/B" binary case of false, and true
            conclude: experiment => experiment.Participants.Count()  == 10,  // Optional criteria for automatically concluding an experiment; default is never
            score: null, /* ... */                                           // Optional criteria for choosing best performer by index; default is best converting alternative
            splitOn: null /* ... */                                          // Optional criteria for splitting a cohort by the number of alternatives; default is a simple hash split
        );
    }
}
```

**Third**, start collecting experiment data, in the back:

```csharp
public class HomeController : Controller
{
    public ActionResult Index()
    {
        return View();
    }

    public ActionResult ClicketyClick()
    {
        // Increment a counter for this named metric
        M.Track("Button clicks");
        
        return View();
    }
}
```

And in the front:

```cshtml
@using ab
<p>Here, dear user, is a button you should totally click:</p>

@if(AB.Group("Jokes on link") == 1)
{
    <a href="@Url.Action("ClicketyClick", "Home")">I am boring, don't click me.</a>    
}
else
{
    <a href="@Url.Action("ClicketyClick", "Home")">Why did the chicken cross the road? To get to the other side!</a>
}
```

License
-------
AB is copyright (c) 2013 Conatus Creative Inc.

It is published under The MIT License, see [LICENSE.md](https://raw.github.com/danielcrenna/ab/master/LICENSE.md) for details.