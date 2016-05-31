# MiniRack
## Tiny aggregator for self-registering HttpModules

```
PM> Install-Package MiniRack
```

This micro library leans on WebActivator to simplify self-registering HttpModules. All it adds on top is an attribute
that you can apply to IHttpModules to have them automatically registered at runtime by the participating ASP.NET 
application.

### Usage

```csharp
[Pipeline]
public class MyModule : IHttpModule
{
	// This module will be dynamically registered...
}
```

_This library works on AppHarbor._