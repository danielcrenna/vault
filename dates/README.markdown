# Dates
## The missing .NET date types

### Introduction
Working with dates and times can be frustrating. There's issues of universal vs. local time, leap years effecting 
time estimates, local calendar differences, and more. On top of that, .NET doesn't provide default classes 
for easily working with date relative time, something you almost always need to handle when building 
applications that deal with payments, scheduling, repeating background events, etc.

This library introduces two new date types: DateSpan, and DatePeriod.

Similar to TimeSpan, the DateSpan abstraction is used to calculate time, expressed in a quantity of an interval (1 day, 4 months, etc.)
passing between a start and end date. This is useful when building applications that need to be aware of the passage of relative time, 
respective of leap years, weeks, months, and years. In addition, the DatePeriod type is used to calculate date occurrences with a range of 
relative time. For example, you can use it to quickly determine which dates would fall into a bi-weekly schedule over the course of six 
months, if it started tomorrow, and if dates occuring on a weekend were deferred to the following Monday.

### Features

* Simple API with only a few key methods
* Calculates the date relative time span between two dates
* Generates DateTime occurrences within a date relative period of time
* Smart handling of weekends, spanning weeks and months, leap years, localized calendars, UTC, etc.

### Usage

#### Calculating DateSpan between two dates

You can create a DateSpan instance using the default constructor that takes a start and end date.

```csharp
DateTime start = new DateTime(2009, 9, 30);
DateTime end = new DateTime(2009, 10, 31);
DateSpan span = new DateSpan(start, end);
```

In the example above, the date span between September 30th, 2009 and October 31st, 2009, would
be one month, and one day. 

```csharp
Console.WriteLine(span.Months);		// Outputs: 1
Console.WriteLine(span.Days);		// Outputs: 1
```

Notice that it's not 32 days. DateSpan is always date relative, so it knew that because
you crossed the month threshold, based on how many days there are in September, you would
need a different value.

If you wanted to calculate a value for 32 days for the same dates, you can define your desired
precision using the DateInterval enum along DateSpan's GetDifference method.

```csharp
var difference = DateSpan.GetDifference(DateInterval.Days, start, end);
Console.WriteLine(difference);		// Outputs: 32
```

Were you expecting 31 days? By default, GetDifference includes the end date in its calculation. If you
prefer to exclude it, you can pass in the excludeEndDate boolean value to the GetDifference method.

```csharp
var difference = DateSpan.GetDifference(DateInterval.Days, start, end, true /* excludeEndDate */);
Console.WriteLine(difference);		// Outputs: 31
```

Most people expect constructing a new DateSpan, however, to exclude the end date automatically, so 
there's an overload for that, too.

```csharp
DateSpan span = new DateSpan(start, end, false /* excludeEndDate */);
Console.WriteLine(span.Months);		// Outputs: 1
Console.WriteLine(span.Days);		// Outputs: 2
```

The GetDifference method has precision down to seconds, so it's possible to calculate more
detailed differences.

```csharp
var difference = DateSpan.GetDifference(DateInterval.Minutes, start, end);
Console.WriteLine(difference);		// Outputs: 46080
```

#### Getting the dates occurring in a given period

You can also calculate a range of dates based on a DateSpan or a DatePeriod. To create a DatePeriod,
use the default constructor, or use one of the common static properties.
 
```csharp
DatePeriod period = new DatePeriod(DatePeriodFrequency.Months, 2) // Bimonthly
DatePeriod period = DatePeriod.Monthly;						      
DatePeriod period = new DatePeriod(DatePeriodFrequency.Years, 1); // Annually
DatePeriod period = DatePeriod.BiAnnually;					      
```

To get a range of DateTime occurrences for a defined period, use the GetOccurrences method

```csharp
DateTime start = new DateTime(2009, 09, 01);
DateTime end = new DateTime(2010, 09, 01);
DatePeriod period = DatePeriod.Weekly;
IEnumerable<DateTime> occurrences = period.GetOccurrences(start, end);
```

By default, every occurrence using GetOccurrences skips weekend days, deferring them to the
following Monday. If you want to avoid this behavior, use the overloaded method.

```csharp	
IEnumerable<DateTime> occurrences = period.GetOccurrences(start, end, false /* skipWeekends */);
```