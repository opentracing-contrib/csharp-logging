[![Build status][ci-img]][ci] [![NuGet][nuget-img]][nuget]

# OpenTracing ILogger Instrumentation

OpenTracing instrumentation for `Microsoft.Extensions.Logging`.

## Installation

Install the [NuGet package](https://www.nuget.org/packages/OpenTracing.Contrib.Logging/):

    Install-Package OpenTracing.Contrib.Logging

## Usage

Log entries are always logged to the current active `ISpan` which is retrieved from a `IScopeManager`. You have the choice to either create a shared instance of `IScopeManager` if you have multiple tracers that all should be logged, or you can just use the instance created by default for your `ITracer`.

All of the following examples will create an `ILogger` instance that attaches logs to the active span. The main difference between the different .NET versions consists in how to create the `ILoggerFactory`.

### ASP.NET Core

It's possible to use this library with ASP.NET Core, but it's prefered to use https://github.com/opentracing-contrib/csharp-netcore instead. This way, you can also already manage span creation.

An example using the Jaeger client implementation can be found at https://github.com/jaegertracing/jaeger-client-csharp/tree/master/examples/Jaeger.Example.WebApi

### .NET Core 2.1

- Instantiate `ITracer`
- Instantiate `ILoggerFactory`
- Call `AddOpenTracing` on `ILoggerFactory` instance

```csharp
using OpenTracing;
using OpenTracing.Contrib.Logging;

public class YourServer {

    private readonly ILogger _logger;
    private readonly ITracer _tracer;

    private YourServer() {
        _tracer = ...; // Create tracer with the OpenTracing client library of your choice
        var loggerFactory = new LoggerFactory()
            .AddOpenTracing(new OpenTracingLoggerOptions
            {
                ScopeManager = _tracer.ScopeManager
            });
        _logger = loggerFactory.CreateLogger<YourServer>();
    }

    private void Run() {
        // We need an active span to be able to log anything!
        using (_tracer.BuildSpan("foo").StartActive(true))
        {
            _logger.LogInformation("It works!");
        }
    }
}
```

### .NET Core 2.2

In .NET Core 2.2, the construction of `LoggerFactory` got deprecated in favor of dependency injection (DI). This example show's the prefered way of Microsoft to use the framework outside of ASP.NET Core. If you do not want to use DI, you can still use the example from .NET Core 2.1 and ignore the warning.

- Instantiate `ITracer`
- Register `ILoggerFactory` with DI
- Request `ILoggerFactory` from DI
- Call `AddOpenTracing` on `ILoggerFactory` instance

```csharp
using OpenTracing;
using OpenTracing.Contrib.Logging;

public class YourServer {

    private readonly ILogger _logger;
    private readonly ITracer _tracer;

    private YourServer() {
        _tracer = ...; // Create tracer with the OpenTracing client library of your choice
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging(builder => 
            builder.AddOpenTracing(new OpenTracingLoggerOptions
            {
                ScopeManager = _tracer.ScopeManager
            }));

        using (var serviceProvider = serviceCollection.BuildServiceProvider()
        using(var loggerFactory = serviceProvider.GetService<ILoggerFactory>())
        {
            _logger = loggerFactory.CreateLogger<YourServer>();
        }
    }

    private void Run() {
        // We need an active span to be able to log anything!
        using (_tracer.BuildSpan("foo").StartActive(true))
        {
            _logger.LogInformation("It works!");
        }
    }
}
```

### .NET Core 3.0

- Instantiate `ITracer`
- Instantiate `ILoggerFactory`
- Call `AddOpenTracing` on `ILoggerFactory` instance

```csharp
using OpenTracing;
using OpenTracing.Contrib.Logging;

public class YourServer {

    private readonly ILogger _logger;
    private readonly ITracer _tracer;

    private YourServer() {
        _tracer = ...; // Create tracer with the OpenTracing client library of your choice
        var factory = LoggerFactory.Create(builder =>
            builder.AddOpenTracing(new OpenTracingLoggerOptions
            {
                ScopeManager = _tracer.ScopeManager
            }));
        _logger = factory.CreateLogger<YourServer>();
    }

    private void Run() {
        // We need an active span to be able to log anything!
        using (_tracer.BuildSpan("foo").StartActive(true))
        {
            _logger.LogInformation("It works!");
        }
    }
}
```

## Options

The logging can be configured using `OpenTracingLoggerOptions`. This examples will show the difference on each option. The following code snipped will be used for all examples.

```csharp
var options = new OpenTracingLoggerOptions {
    ScopeManager = _tracer.ScopeManager,
    IncludeLoggerName = [true|false],
    IncludeKeyValuePairs = [true|false]
};
using (var loggerFactory = LoggerFactory.Create(builder => builder.AddOpenTracing(options)))
{
    var logger = factory.CreateLogger<YourServer>();
    logger.LogInformation("It works {greatness}!", "awesome");
}
```

### ScopeManager

This *MUST* be set to an instance used by the tracer. If this is `null`, logging will effectively be disabled.

Log entries created by this library will always be attached to the `ISpan` retrieved from the `IScopeManager` instance through `scopeManager.Active.Span`.

### IncludeLoggerName

*Notice: This example assumes `IncludeKeyValuePairs = false` for simplicity.*

Using `IncludeLoggerName = false` will create a log entry in the span with the following key-value-pairs:

|   key   |       value       |
|---------|-------------------|
| event   | Information       |
| message | It works awesome! |
| logger  | YourServer        |

Using `IncludeLoggerName = true` will create a log entry in the span with the following key-value-pairs:

|   key   |       value       |
|---------|-------------------|
| event   | Information       |
| message | It works awesome! |

### IncludeKeyValuePairs

*Notice: This example assumes `IncludeLoggerName = false` for simplicity.*

Using `IncludeKeyValuePairs = false` will create a log entry in the span with the following key-value-pairs:

|    key    |       value       |
|-----------|-------------------|
| event     | Information       |
| message   | It works awesome! |

Using `IncludeKeyValuePairs = true` will create a log entry in the span with the following key-value-pairs:

|    key    |       value       |
|-----------|-------------------|
| event     | Information       |
| message   | It works awesome! |
| greatness | awesome           |

[ci-img]: https://ci.appveyor.com/api/projects/status/github/opentracing-contrib/csharp-logging?svg=true
[ci]: https://ci.appveyor.com/project/opentracing/csharp-logging
[nuget-img]: https://img.shields.io/nuget/v/OpenTracing.Contrib.Logging.svg
[nuget]: https://www.nuget.org/packages/OpenTracing.Contrib.Logging/
