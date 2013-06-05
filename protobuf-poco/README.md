protobuf-poco
=============

A reference implementation for using protobuf-net with zero configuration

### Intro

[Google's Protocol Buffers](https://developers.google.com/protocol-buffers/) is an awesome serialization library, and Marc Gravell's [protobuf-net](http://code.google.com/p/protobuf-net/) is an excellent implementation of it for .NET.
The only thing that appears missing is a default, conventions-based way to serialize/deserialize arbitrary .NET objects without decorating 
your class with contract and member order attributes. This little project uses what's already available in protobuf-net to create a 
serializer that requires no attributes at all.

_Caveat: Because the model is set up using reflection, any changes will likely break field order, and thus not be compatible with future versions.
You should store some reference to rebuild the model type in cases where the model changes often, or simply don't use this where models change
frequently or versioning is a feature._

### Usage

Given some class:

```csharp
public class User
{
	public int Id { get; set; }
	public string Email { get; set; }
	public byte[] Image { get; set; }
	public DateTime CreatedAt { get; set; }
	public List<string> Tags { get; set; }
}
```

You can use Protocol Buffers:

```csharp
// Create a new POCO serializer instance
var serializer = new protobuf.Poco.Serializer();
            
// Create an object instance
var user = new User();
user.Id = 1;
user.Email = "good@domain.com";
user.Image = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
user.CreatedAt = DateTime.Now;
user.Tags = new List<string>();
user.Tags.Add("soccer");

// Serialize it with protobuf-net
var serialized = serializer.SerializeToStream(user);

// Deserialize it back
var deserialized = serializer.DeserializeFromStream<User>(serialized);
```
