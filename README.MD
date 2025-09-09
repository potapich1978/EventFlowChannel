![Tests](https://img.shields.io/endpoint?url=https://gist.githubusercontent.com/potapich1978/c15fb9d197b16fda54ddaa8758566ce4/raw/test-status.json)
![Coverage](https://img.shields.io/endpoint?url=https://gist.githubusercontent.com/potapich1978/7fc2289aef2d6408005f3fa009069786/raw/coverage.json)
![License](https://img.shields.io/endpoint?url=https://gist.githubusercontent.com/potapich1978/193c9aa84b5eb75ae534e41d9764e65c/raw/license.json)

# Event Dispatcher Architecture with Channel-Based Processing

## Overview
A high-performance event-driven processing system built on **.NET Standard 2.0** that provides thread-safe message dispatching with configurable channel options.  
The architecture combines the flexibility of event-driven programming with the robustness of channel-based communication patterns.
---

## Key Features
- **Thread-Safe Event Dispatching** - Safe concurrent event publishing and consumption  
- **Configurable Channel Types** - Support for both bounded and unbounded channels  
- **Fluent Configuration API** - Intuitive builder pattern for channel configuration  
- **Multiple Reader Support** - Configurable number of concurrent consumers  
- **.NET Standard 2.0 Compatibility** - Works with .NET Framework 4.6.1+ and .NET Core 2.0+  
- **Dependency Injection Ready** - Seamless integration with Microsoft DI container  


## Thread Safety
- **Thread-Safe Publishing** - Multiple writers supported through channel synchronization
- **Concurrent Consumption** - Multiple readers process events simultaneously
- **Lifecycle Management** - Atomic start/stop operations with proper cancellation support
- **Exception Handling** - Isolated error handling per event with continuous processing


## Best Practices
- Handler Design
- Keep handlers focused and single-purpose
- Implement proper error handling
- Use async operations for I/O-bound work

## Channel Configuration
- Use bounded channels for predictable memory usage
- Match reader count to available cores for CPU-bound work
- Enable multiple writers for high-throughput scenarios

## Monitoring
- Implement comprehensive logging

## Performance Considerations
- Bounded Channels - Prevent memory overgrowth with configurable capacity
- Reader Count - Optimize based on workload (I/O vs CPU bound)
- Channel Options - SingleWriter/SingleReader optimizations available
- Async/Await - Non-blocking operations throughout the pipeline

## Example Use Cases
- Microservices Communication - Event-based service coordination
- Data Processing Pipelines - Parallel event processing
- Real-time Applications - WebSocket/gRpc message processing
- Background Processing - Async task execution

## Core Components

### Event Interfaces
- [`IGenericEvent<T>`](ChannelReader/Abstract/IGenericEvent.cs)   - Base event contract with typed identifier  
- [`IGenericEventHandler<T>`](ChannelReader/Abstract/IGenericEventHandler.cs) - Event handler contract  
- [`IEventDispatcher<T>`](ChannelReader/Abstract/IEventDispatcher.cs) - Event routing abstraction  

### Channel Infrastructure
- [`IChannel<T>`](ChannelFactory/Abstract/IChannel.cs) - Channel lifecycle management  
- [`IChannelReader<T>`](ChannelReader/Abstract/IChannelReader.cs) - Event consumption logic  
- [`IChannelFactory<T>`](ChannelFactory/Abstract/IChannelFactory.cs) - Channel creation abstraction  

### Builder Pattern
- [`IChannelBuilder<T>`](EventChannelBuilder/Abstract/IChannelBuilder.cs) - Fluent API for channel configuration  
- [`IBoundedChannelBuilder<T>`](EventChannelBuilder/Abstract/IBoundedChannelBuilder.cs) - Bounded channel-specific options  
- [`IUnboundedChannelBuilder<T>`](EventChannelBuilder/Abstract/IUnboundedChannelBuilder.cs) - Unbounded channel configuration  


## Installation
```xml
<PackageReference Include="Potapich.EventFlowChannel" Version="1.0.1" />
```

## Quick Start

### 1. Define Events and Handlers
```cs
public enum EventType { UserCreated, OrderProcessed }

public class UserCreatedEvent : IGenericEvent<EventType>
{
    public EventType EventType => EventType.UserCreated;
    public string UserName { get; set; }
}

public class UserEventHandler : IGenericEventHandler<EventType>
{
    public EventType EventType => EventType.UserCreated;
    
    public async Task HandleAsync(IGenericEvent<EventType> @event, CancellationToken token)
    {
        var userEvent = (UserCreatedEvent)@event;
        // Process event
    }
}
```

### 2. Configure Services
```cs
services.AddEventChannelBuilder<EventType>(typeof(Startup).Assembly)
        .AddSingleton<IGenericEventDispatcherLogger, MyLogger>();
```

### 3. Create and Use Channel
```cs
var channel = provider.GetService<IChannelBuilder<EventType>>()
    .Bounded()
    .WithCapacity(1000)
    .WithReadersCount(4)
    .WithMultipleWriters()
    .Build();

channel.Start();

await channel.Enqueue(new UserCreatedEvent { UserName = "John" });
```

## Advanced Configuration

### Bounded Channel with Custom Options
```cs
var channel = builder.Bounded()
    .WithCapacity(5000)
    .WithReadersCount(8)
    .WithFullMode(BoundedChannelFullMode.DropWrite)
    .WithMultipleWriters(false)
    .Build();

channel.Start();

await channel.Enqueue(new UserCreatedEvent { UserName = "John" });
```

### Unbounded Channel

```cs
var channel = builder.Unbounded()
    .WithReadersCount(2)
    .WithMultipleWriters(true)
    .Build();

channel.Start();

await channel.Enqueue(new UserCreatedEvent { UserName = "John" });
```

## Logging Integration

### Microsoft.Extensions.Logging
```cs
public class MicrosoftLoggerAdapter : IGenericEventDispatcherLogger
{
    private readonly ILogger _logger;

    public MicrosoftLoggerAdapter(ILoggerFactory factory)
    {
        _logger = factory.CreateLogger("EventDispatcher");
    }

    public void LogWarning(string message) => _logger.LogWarning(message);
    public void LogError(string message) => _logger.LogError(message);
    public void LogError(string message, Exception ex) => _logger.LogError(ex, message);
}
```

### Serilog
```cs
public class SerilogAdapter : IGenericEventDispatcherLogger
{
    public void LogWarning(string message) => Serilog.Log.Warning(message);
    public void LogError(string message) => Serilog.Log.Error(message);
    public void LogError(string message, Exception ex) => Serilog.Log.Error(ex, message);
}
```
