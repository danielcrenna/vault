# hollywood
## A simple little tool for launching

```
PM> Install-Package hollywood
```

### Introduction
This is a little bit of code for making working on a site in "stealth mode" easier.
It is of course inspired by 37signals' definition of a [hollywood launch](http://gettingreal.37signals.com/ch13_Hollywood_Launch.php).

### Features

* Simple to set up
* Use it to do continuous deployment while maintaining a teaser page viewable to the public

### Usage
* First, create a view folder called 'Launch' and add an index view; this will be your teaser page:
/Views/Launch/Index.cshtml
```html
<p>Your princess is in another castle. Unless...</p>
```

* Next, add `web.config` settings to control prelaunch. Typically you want prelaunch turned off by default, and on in `Release`:

_Web.config_

```xml
<appSettings>
    <add key="Prelaunch" value="false" />
    <add key="PrelaunchToken" value="allyourbase" />
</appSettings>
```

_Web.Release.config_

```xml
<appSettings>
    <add key="Prelaunch" value="true" xdt:Locator="Match(key)" xdt:Transform="Replace" />
</appSettings>
```

* Finally, opt-in to the feature by calling this on application startup:

_Global.asax.cs_

```csharp
using hollywood;
Launch.InstallPrelaunch();
Launch.AddAllowedUrls("/privacy", "/terms");
```

* With everything installed, public-facing users will see your "launch page", while people in the know can pass `?token=allyourbase` into the page, and from then on access your site normally.

#### TODO
Right now, access is provided through a single token mechanism. It should grant by email address, send beta invites, etc.
