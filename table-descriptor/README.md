# TableDescriptor

```
PM> Install-Package TableDescriptor
```

### Introduction

TableDescriptor takes a regular C# object and deduces it's database table structure
by convention. This is useful when building a bespoke micro-ORM out of high performance
parts. This library does _not_ deal with mapping database fields to object properties.
That's much better left to more suitable libraries like Dapper. This library
will save you the work of performing meta-data oriented tasks, and provide
fast member access for the times you want to invoke get/set against that 
mapping.

### Features

### Mapping a type by convention

### Mapping a type with your own convention

