# OAuth
## A public domain OAuth library

### Introduction
Working with OAuth 1.0a is hard. You have to get the signature handling just right, or you'll get
vague, unhelpful errors from most servers that implement the specification. But OAuth is important
because it's an effective way to do useful things with your user's external data without having 
to ask for or store their important credentials from other sources. This library provides a set of
tools, centered around the OAuthRequest class, for making it easy to build applications that talk
to OAuth servers.

### Features

* Battle-tested - This code has been used for over two years to make millions of requests
* Simple API that helps reduce the complexity of making OAuth requests
* Supports OAuth 1.0a as a standalone library - Use it wherever you need it
* Public domain - open specifications should be free!

### Usage

#### A Typical Workflow

Making an OAuth request involves a lot more context than other security credentials. You can find
all the details of the OAuth spec at [the official site](http://oauth.net), and plenty of tutorials online to determine
the "What", and this library will provide the "How". In a typical OAuth workflow you need to accomplish 
the following things:

* Obtain a "request token" from the OAuth server using a preset "consumer key" and "consumer secret"
provided to you by the application you are consuming (i.e. Twitter, Google, etc.)

* Use the request token data you retrieved to redirect your user to the OAuth site where they can
safely enter their credentials and allow (or deny) your application's access to their data

* Either the OAuth server then redirects back to your application using a known callback URL, or
it presents a "verifier", or PIN number, to the user, that they then enter in to your application
to obtain access.

* Finally, your application uses the verification information provided in the callback or user
entry in the previous step, to obtain an "access token". This access token can then be used to 
make requests to the OAuth provider's API and retrieve user data on your user's behalf

#### Making Requests

You can either create a new OAuthRequest instance yourself and add the appropriate properties
that make up your request, or use the static methods if you need a little help with default
settings and what's required for each request type. You always need to provide your consumer key,
consumer secret, and set the RequestUrl property that you intend to make the request with. Since
this library only prepares credentials, you can send the request using whatever HTTP client you
prefer.

```csharp
// Creating a new instance directly
OAuthRequest client = new OAuthRequest
{
    Method = "GET",
    Type = OAuthRequestType.RequestToken,
    SignatureMethod = OAuthSignatureMethod.HmacSha1,
    ConsumerKey = "CONSUMER_KEY",
    ConsumerSecret = "CONSUMER_SECRET",
    RequestUrl = "http://twitter.com/oauth/request_token",
    Version = "1.0a",
    Realm = "twitter.com"
};

// Creating a new instance with a helper method
OAuthRequest client = OAuthRequest.ForRequestToken("CONSUMER_KEY", "CONSUMER_SECRET");
client.RequestUrl = "http://twitter.com/oauth/request_token";
```

Once you have an OAuthRequest instance, you can obtain either the appropriate HTTP Authorization
header value, or the URI query string value, using one of two methods. Most OAuth providers
support both of these authentication style specs.

```csharp
// For HTTP header authorization
string auth = client.GetAuthorizationHeader();

/// For URL query authorization
string auth = client.GetAuthorizationQuery();
```

From this point, you just need to pass this information to your HTTP client to send
to the endpoint you specified in RequestUrl; remember the HTTP method and endpoint must 
match exactly, since they are used in the signature generation process.

```csharp
// Using HTTP header authorization
string auth = client.GetAuthorizationHeader();
HttpWebRequest request = (HttpWebRequest) WebRequest.Create(client.RequestUrl);           

request.Headers.Add("Authorization", auth);
HttpWebResponse response = (HttpWebResponse) request.GetResponse();

// Using URL query authorization
string auth = client.GetAuthorizationQuery();
var url = client.RequestUrl + "?" + auth;
var request = (HttpWebRequest)WebRequest.Create(url);
var response = (HttpWebResponse)request.GetResponse();
```

#### XAuth

This library also supports XAuth, which is a client authenticating form of OAuth that allows you
to pass a user and password and obtain an access token in one step; this is useful for mobile
applications, or when migrating from basic security to OAuth, and normally requires further
steps from the OAuth provider (i.e. applying for access), as this certainly defeats the purpose
of OAuth beyond limiting credential input to a single time use.

```csharp
OAuthRequest client = OAuthRequest.ForClientAuthentication("CONSUMER_KEY", "CONSUMER_SECRET", "USERNAME", "PASSWORD");
client.RequestUrl = "https://api.twitter.com/oauth/access_token";
```

#### OAuth Echo

Sometimes applications need to make third-party requests through a security "double hop". For
example, an image posting service that posts to Twitter but also has an API, needs a way to
authorize that the user of their API has the same credentials as Twitter's API. OAuth Echo is
accomplished by using special HTTP headers that point to a specific endpoint at the main provider's 
site.

```csharp
// Get an OAuthRequest instance for the main site's echo endpoint
OAuthRequest client = OAuthRequest.ForProtectedResource("CONSUMER_KEY", "CONSUMER_SECRET", "ACCESS_TOKEN", "ACCESS_TOKEN_SECRET");
client.RequestUrl = "https://api.twitter.com/account/verify_credentials.json";
var auth = client.GetAuthorizationHeader();

// Make the request to the third-party site and provide the correct echo headers
HttpWebRequest echo = (HttpWebRequest) WebRequest.Create("http://api.twitpic.com); 
echo.Headers.Add("X-Auth-Service-Provider", client.RequestUrl);
echo.Headers.Add("X-Verify-Credentials-Authorization", auth);
```