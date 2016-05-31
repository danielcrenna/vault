logging
=====

```powershell
PM> Install-Package logging
```

This is just another logging abstraction used in webstack. 
It's difference is it forces you to use functions for deferred log execution, which means you avoid
accidentally hobbling your application by doing string building in debug-level logging, for example.
