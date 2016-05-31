# Depot
## A dependency cache for .NET

    PM> Install-Package Depot

_This work is in progress, so documentation may not reflect functionality at the present time_

### Introduction

Depot is a combination content and object cache layer that supports dependencies. It provides
a common interface for caching client implementations, supports time and disk-based dependency caching, 
and object graph-based dependency caching. It provides support for ASP.NET MVC to facilitate intelligent
caching at the controller level.

### Features

* Thoughtful - Interface makes it easier to move from in-process to distributed caching when you scale
* Dependency Graph Aware - Locates cached objects across multiple queries
* Web Savvy - MVC support for automatically generating cache keys and combining cached and fresh results

### Usage

Depot uses a Set-Add-Replace caching metaphor, which is typical for distributed caching clients. This helps make
the transition from an in-process cache (like ASP.NET's Cache object) to distributed caching (like Memcached) an
easier process since it maps cleanly to distributed caching clients. 

#### Why use a cache interface? Why use this one?

Simply because you will eventually want to move to distributed caching in your web application, and this is the
simplest way to make your caching mechanism replaceable and transparent in your code. Since a caching layer is a fairly
common programming task, it is popular to roll your own, and there are many open source alternatives. Depot is an interesting
choice because it also provides graph-based dependency caching, making it suitable for web applications, where graphing
cuts down on by-rote caching logic in controller methods and other litter that makes object caching tedious and error prone.

#### What is the difference between a content cache and an object cache?

Depot makes this distinction for mental purposes, but it is this: 

* A content cache stores static content or data, like CSS, JavaScript, and images, and usually has a dependency associated with 
it that is based on time (absolute or sliding expiration), or files (the true representation of the content on disk).

* An object cache stores application data types, like Users and Accounts, and usually has a dependency associated with the 
data type, such as child collections, where changes to the underlying dependent objects trigger the cache to invalidate the parent.






