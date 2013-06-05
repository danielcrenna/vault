copper
======

Copper is an eventing middleware library for distributed applications. This library is essentially
a re-imagining of Greg Young's PVC pipes library, using TPL, async/await, and Rx. 
It gives you high performing, non-blocking and parallelized eventing. You build other components out of it.

### Why would I use this?
- You're building a real-time system and want to notify your hub as things occur
- You want to synchronize activity between various application components at the event level
- You want to use the "[shared nothing](http://en.wikipedia.org/wiki/Shared_nothing_architecture)" integration pattern
- You want to denormalize events into multiple views for queries (event sourcing)
- You can build nice stuff on top of it

### Why use this over the original?
- PVC is cross-platform, this isn't yet; it has the liberty to use the TPL, async/await, `IObservable` and Rx
- It includes built-in producer/consumer implementations and chaining methods to simplify most common distributed tasks
- It has less moving parts, so it's (arguably) simpler to understand
- You can't use PVC for hot water pipes, but you can use copper, though it's a tad more expensive (bazing!)

### Usage
--------
This library provides a foundation for distributed middleware on three levels: pipes, protocols,
and event dispatch. It provides most of what you need to handle concurrency and synchronization
of arbitrary producers and consumers. Here is a simple example using two built-in components.

```csharp
var block = new ManualResetEvent(false);

var producer = new ObservingProducer<int>();
var consumer = new DelegatingConsumer<int>(i => Console.WriteLine(i));

producer.Produces(Observable.Range(1, 10000), onCompleted: () => block.Set());
producer.Attach(consumer);
producer.Start();

block.WaitOne();
```

In the example above, a producer emits a series of integers to its consumer.
The consumer writes all integers it receives to the console. When the producer
is finished, it signals the block to end the program. Both the consumer
and the producer performed their work in the background. So what happened here?
Let's take a look at the interfaces for `Produces<T>` and `Consumes<T>`:

```csharp
public interface Produces<out T>
{
    void Attach(Consumes<T> consumer);
}

public interface Consumes<in T>
{
	bool Handle(T @event);
	Task<bool> HandleAsync(T @event);
}
```

There isn't much here. We have a producer, but all we know about it is it
allows attaching a consumer. And the consumer will perform some action, 
synchronously or asynchronously, when it handles an event. 

### What's included

#### Consumers
- `AsyncConsumer`
- `BatchingConsumer`
- `CollectionConsumer`
- `CompositeConsumer`
- `DelegatingConsumer`
- `DelegatingBatchingConsumer`
- `FileConsumer`
- `ProtocolConsumer`

#### Producers
- `CollectionProducer`
- `ObservingProducer`
- `DelegatingObservingProducer`
- `FileProducer`
- `ProtocolProducer`
- `BackgroundProducer`
- `UsesBackgroundProducer`


TODO:
[ ] Reseed buffer from in-memory backlog periodically
[ ] Extension methods should cover all in-built consumers and producers
[ ] Complete documentation (for now check out the Examples project and unit tests)