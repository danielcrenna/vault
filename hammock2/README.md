![hammock2](https://github.com/danielcrenna/hammock2/raw/master/logo.png) 

hammock2 is a single .cs file for making munchy munchy API. Fully dynamic, so no "client library" required.

_LOL jk there's dependencies_
```
	PM> Install-Package Microsoft.Net.Http	# Or provide your own implementation of IHttpEngine
	PM> Install-Package ServiceStack.Text	# Or provide your own implementation of IMediaConverter
	PM> Install-Package AsyncBridge         # Only required if your project is .NET 4.0, not .NET 4.5
```

Usage
-----
```csharp
// Spin up a client
dynamic twitter = new Http("https://api.twitter.com");

// GET https://api.twitter.com/users/show.json?screen_name=danielcrenna
var reply = twitter.Users.Show.Dot.Json(screen_name: "danielcrenna");    
var response = reply.Response;
var user = reply.Body;

// Get the result
Console.WriteLine(response.StatusCode + response.ReasonPhrase); // 200 OK
Console.WriteLine(user.ScreenName + ": " + user.Status.Text);   // danielcrenna: Best. Library. Evar.
```

_Bring your own web stack_

hammock2 needs engines for HTTP and JSON. If you don't want to use the defaults then don't bring 
in the partial classes that implement them. If you take only the hammock.cs file, then you need to implement
these interfaces to provide your own serializer implementation:

```csharp
public interface IMediaConverter
{
    string DynamicToString(dynamic instance);
    IDictionary<string, object> StringToHash(string json);
    string HashToString(IDictionary<string, object> hash);
    T DynamicTo<T>(dynamic instance);
    T StringTo<T>(string instance);
}

public interface IHttpEngine
{
    dynamic Request(string url, string method, NameValueCollection headers, dynamic body, bool trace);
}
```

_Concrete addiction_

If you really need concrete classes, you can do that too, by deserializing the whole body, or a portion of it:

```csharp
public class StripeCustomer
{
    public string Id { get; set; }
	// ...
}

public class Stripe
{
    private readonly dynamic _stripe;
    public Stripe()
    {
        _stripe = new Http("https://api.stripe.com/v1/");
        _stripe.Auth = HttpAuth.Basic(ConfigurationManager.AppSettings["StripeTestKey"]);
    }
    public IEnumerable<StripeCustomer> GetCustomers()
    {
        // GET https://api.stripe.com/v1/customers
        var reply = _stripe.Customers();
        
        // { count : "2", data: [{customer},{customer}] }
        var body = reply.Body;
        
        // Cherry-pick the data element where customers live
        var customers = body.Deserialize<IEnumerable<StripeCustomer>>(body.Data);
        return customers;
    }
}
```

Tracing requests and responses:

```csharp
// Uses stock .NET trace writer
var api = new Http("https://awe.sm");
api.Trace = true;
```

Authentication
--------------
*Use the `HttpAuth` helper:*
```csharp
_stripe = new Http("https://api.stripe.com/v1/");
_stripeKey = ConfigurationManager.AppSettings["StripeTestKey"];
_stripe.Auth = HttpAuth.Basic(_stripeKey);
```

*Pass in your own custom pre-request code:*
```csharp
Action<Http> auth = http =>
{
    var authorization = Convert.ToBase64String(Encoding.UTF8.GetBytes(_stripeKey + ":"));
    http.Headers.Add("Authorization", "Basic " + authorization);
};
_stripe.Auth = auth;
```

TODO
----
- match method prefix to web method
- content negotiation / formatters
- async/await