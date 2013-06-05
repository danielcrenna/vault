# JSON
## A public domain JSON parser for .NET

    PM> Install-Package JSON

### Introduction

This JSON parser is designed for applications that need a fast parser for embedded serialization but don't need a full blown framework for working with JSON data. 

### Features

* Uses precompilation tags for maximum platform compatibility
* In .NET 4.0, supports dynamic deserialization from a JSON schema to .NET object properties
* Public domain; use this anywhere without restrictions, open standards should be free.

### Usage

#### Serializing and deserializing types

Serialization can occur at the type and property levels. At the type level, you use `Serialize` and `Deserialize` methods as expected. With property level serialization, you use `FromJson` and `ToJson` to convert JSON input to and from a hash. This is useful when you want to apply custom logic without requiring strong types.

Normally, you define a strong type in your application and use the generic `Deserialize` method to hydrate an instance of your object with a provided valid JSON input.

```csharp
public class Dog
{
    public string Name { get; set; }
}

var json = @"{ ""name"" : ""Spot"" }";
var spot = JsonParser.Deserialize<Dog>(json);
Console.WriteLine(spot.Name);                              // Spot
```

#### Dynamic deserialization

Deserializing with a dynamic type system works much the same way, except that you can now access the properties of the underlying schema, in the original JSON's casing convention, or in typical .NET pascal case.

```csharp
var json = @"{ ""horn_length"": 4, ""magic_powers"" : { ""cone_of_coneyness"" : true } }";
var unicorn = JsonParser.Deserialize(json);

Console.WriteLine(unicorn.HornLength);                     // 4
Console.WriteLine(unicorn.magic_powers.ConeOfConeyness);   // true
```