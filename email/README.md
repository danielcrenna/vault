Introduction
------------
Applications of any significance have to send email to their users. This library helps you do that in your
.NET applications. It covers most major scenarios for the delivery of email without trying to do more than it should.
It uses .NET 4's parallel features for predictable, stable blocking queues, etc.

Features
--------
- Easily render and send emails in your applications, in text, HTML, or both
- Supports Postmark (http://postmarkapp.com), and plain old SMTP out of the box (and it's trivial to write your own IEmailProvider implementation)
- Supports Liquid (http://liquidmarkup.org), ensuring safe and fast rendering of batched emails with a well-known standard
- A built-in delivery service that can deliver email from a pickup directory (ideal for hosting in a Windows Service) in parallel
- Support for time-released delivery (i.e. "send this email in 3 days") and email throttling (i.e. "deliver 100 emails per hour")
- A Reactive Extensions hook for when you want to send email from an observable (with a built-in directory watcher)

Benefits
--------
- Adds a layer of robustitude between you and your chosen ESP
- Can be hosted standalone so you're not beholden to any particular web framework
- Lends itself well to CMS solutions where users have the ability to edit their own email templates (templates can't execute server-side code!)
- Debugging support allows you to rewrite all recipients to a testing mail account, or capture all outgoing messages in a pickup directory

Is it any good?
---------------
Yes.

How do I use it?
----------------
Pretty much everything you want is in `DeliveryService`. You pass it the dependencies `IEmailService` and `IDeliveryConfiguration`, which allow you
to decide what service you're sending to, and your delivery options such as retry policy. That's pretty much it, the service will run in the background,
picking messages off the queue, until it is stopped. Messages still in the queue are backlogged to disk. You can start and stop the service, and queue
new messages for delivery.

When you queue up an email directly, you use the `EmailMessage` class. It contains your usual MIME attributes, and also has a few properties
for you to probe for delivery statistics (how many attempts were made, when it was reported delivered, etc.). You use `TextBody` and `HtmlBody` to define your
message content directly. Most of the time, though, you want to use `IEmailService` to create new messages you can send with `DeliveryService`. 
This interface provides templating features using the Liquid (http://liquidmarkup.org) language. This means you can send templated emails using C#'s dynamic 
features to build up an `EmailMessage` instance. Something like this:

```
var message = service.CreateTextEmail("Hello, {{ YouThere }}!", new { YouThere = "Bobby Tables", From = "me@me.com", To = "ipwnsql@bobbytables.com", Subject = "email test" });
deliveryService.Send(message);
```

I've also left you an example of how you could run this as a Windows Service. In the source code, just check out the `email.Service` project.

The Fine Print
--------------
I like to share my work and put in a reasonable amount of effort to ensure my little libraries are well documented, serve a single purpose,
and can be easily brought into your own source code. I do not offer support for this code (and am not good at getting back to you on issues). 
I'll happily take patches that are tested, and use the coding style and conventions of the project (if I can tell someone else wrote it, I'll 
probably reject it). If you want to do your own thing, just fork it, or download the source.

Cheers,

Daniel Crenna
(http://danielcrenna.com)

