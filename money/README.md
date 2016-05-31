# Money
## Painless currency management in .NET

### Introduction
Working with money is hard. You have to calculate it, for one, but more importantly you have to display it correctly,
obeying strange cultural formatting rules. This library introduces three new types: Money, CurrencyInfo, and Currency.
Similar to CultureInfo, the CurrencyInfo abstraction when coupled with the Money type makes it easy to
create currency and display it in different cultures. Currency is an enum that hides the complexity of CurrencyInfo,
so your code just has to reference one authoritative currency type to get the benefit.

### Features

* Provides currency disambiguation and native region handling
* Supports many major currencies (and it's easy to fork to add new ones)
* Simple API that gets out of your way
* Accurate currency rounding

### Usage

#### Creating Money

When you create a Money instance, it is, at construction time, forever bound to a currency. If you don't
specify the currency explicitly, it is set based on the current operation thread's culture properties.

```csharp
// Let's create $1000 in cold, hard American cash
Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
Money money = 1000;
```

The method above, when you don't specify it explicitly, will set the currency based on the current
thread's culture. You can explicitly request a currency type using the Currency enum passed into 
the constructor, or copy other currency types by passing in their CurrencyInfo instance.

```csharp
Money fin = new Money(Currency.USD, 5.00);
Money fiddy = new Money(money.CurrencyInfo, 50.00);	
```

Now that we have a money instance, we get access to all kinds of regional info:

```csharp
Console.WriteLine(money.CurrencyInfo.Code);				// Outputs: USD
Console.WriteLine(money.CurrencyInfo.DisplayCulture);   // Outputs: en-US (it's a CultureInfo instance)
Console.WriteLine(money.CurrencyInfo.DisplayName);      // Outputs: US Dollar
Console.WriteLine(money.CurrencyInfo.NativeRegion);     // Outputs: US
```

#### Creating CurrencyInfo

There are many ways to obtain a CurrencyInfo instance, from a Currency enum value,
or even RegionInfo if you're unsure of the currency in an area.

```csharp
// The current culture of the running thread
CurrencyInfo currencyInfo = CultureInfo.CurrentCulture;

// From the Currency enumeration (New Zealand Dollars)
CurrencyInfo currencyInfo = Currency.NZD;

// From a specific country using RegionInfo (Canadian Dollars)
CurrencyInfo currencyInfo = new RegionInfo("CA");

// From a formal CultureInfo instance (French Francs)
CurrencyInfo currencyInfo = new CultureInfo("fr-FR");
```

#### Displaying Money
		
Where Money really shines is when you need to display a currency from the
viewpoint of a different culture, while preserving its native value and currency.
You can do this either by calling ToString(), which will provide the currency
value _as it should be displayed in the current culture_, or explicitly using
the DisplayIn method.

```csharp
// Your customer is using your application from Paris, France...
// Let's create $1000 in cold, hard American cash
Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
Money money = 1000;
Console.WriteLine(money.ToString())		// Outputs: $1,000.00

// Display the same money in a different culture thread
Thread.CurrentThread.CurrentCulture = new CultureInfo("fr-FR");
Console.WriteLine(money.ToString());	// Outputs: $1 000,00

// Display the same money using the DisplayIn method
var explicit = money.DisplayIn(new CultureInfo("en-CA"));
Console.WriteLine(explicit);			// Outputs: $1,000.00 USD
```
	
In the last example above, since the culture (Canadian English) also uses dollars
for currency, the "USD" disambiguator is automatically added to give context to
the currency amount. If this isn't desired, you can use the overload for DisplayIn
that turns disambiguation off.

#### Culture Awareness

Because Money knows that Canada has a French equivalent, it elects to display
the currency in the "fr-CA" display culture. 

```csharp
// Your customer is using your application from Paris, France...
Thread.CurrentThread.CurrentCulture = new CultureInfo("fr-FR");

// But you want to display currency in Canadian Dollars
Money money = new Money(Currency.CAD, 1000);
Console.WriteLine(money.ToString());		// Output: 1 000,00 $
```
	
The same thing happens if we want to display Canadian currency in Britain;
Since Canada also has an English equivalent, we get the closest match for
outputting in English.

```csharp
Thread.CurrentThread.CurrentCulture = new CultureInfo("en-GB");
Money money = new Money(Currency.CAD, 1000);
Console.WriteLine(money.ToString());		// Output: $1,000.00 
```

There is no CAD equivalent in Germany, so currency display reverts to its
home and native land.

```csharp
Thread.CurrentThread.CurrentCulture = new CultureInfo("de-DE");
Money money = new Money(Currency.CAD, 1000);
Console.WriteLine(money.ToString());		// Output: $1,000.00 
```

Note that this is _not_ the same as if we displayed 
CAD in Germany; _origin matters!_

```csharp
Thread.CurrentThread.CurrentCulture = new CultureInfo("en-CA");
Money money = new Money(1000);
var german = new CultureInfo("de-DE");
Console.WriteLine(money.DisplayIn(german));  // Output: $1.000,00
```

#### Calculating Money

The Money type supports currency with large numbers of decimal places, which is troublesome
using native .NET types, particularly with rounding off. Money acts on Martin Fowler's 
suggestion of using whole numbers, and internally scales decimal places up and down
accordingly, so it should meet your needs for accurate rounding without headaches. All
standard arithmetical operators are provided so you can work with the Money type like
you normally work with .NET floating point types. In addition, for those working on 
currency conversion operations, Money stores the time it was created in CreatedDate, so that 
exchange rate precedence is established.