# SQS Toolbox

This is a work-in-progress repository for a set of libraries, extensions and helpers to support working with AWS Simple Queue Service from .NET applications.

## Base Package

| Package | NuGet Stable | NuGet Pre-release | Downloads | 
| ------- | ------------ | ----------------- | --------- | 
| [DotNetCloud.SqsToolbox](https://www.nuget.org/packages/DotNetCloud.SqsToolbox) | [![NuGet](https://img.shields.io/nuget/v/DotNetCloud.SqsToolbox.svg)](https://www.nuget.org/packages/DotNetCloud.SqsToolbox) | [![NuGet](https://img.shields.io/nuget/vpre/DotNetCloud.SqsToolbox.svg)](https://www.nuget.org/packages/DotNetCloud.SqsToolbox) | [![Nuget](https://img.shields.io/nuget/dt/DotNetCloud.SqsToolbox.svg)](https://www.nuget.org/packages/DotNetCloud.SqsToolbox) | 

## Extensions

| Package | NuGet Stable | NuGet Pre-release | Downloads | 
| ------- | ------------ | ----------------- | --------- | 
| [DotNetCloud.SqsToolbox.Extensions](https://www.nuget.org/packages/DotNetCloud.SqsToolbox.Extensions) | [![NuGet](https://img.shields.io/nuget/v/DotNetCloud.SqsToolbox.Extensions.svg)](https://www.nuget.org/packages/DotNetCloud.SqsToolbox.Extensions) | [![NuGet](https://img.shields.io/nuget/vpre/DotNetCloud.SqsToolbox.Extensions.svg)](https://www.nuget.org/packages/DotNetCloud.SqsToolbox.Extensions) | [![Nuget](https://img.shields.io/nuget/dt/DotNetCloud.SqsToolbox.Extensions.svg)](https://www.nuget.org/packages/DotNetCloud.SqsToolbox.Extensions) | 

# Features

## SQS Polling Queue Reader

**Status**: Work in progress, released in alpha.

Supporting types for polling an SQS queue repeatedly for messages in a background `Task`.

### Quick Start

**WARNING**
These packages are considered alpha quality. They are not fully tested and the public API is likely to change during development and based on feedback. I encourage you to try the packages to provide your thoughts and requirements, but perhaps be wary of using this in production!

The most convenient consumption pattern is to utilise the DotNetCloud.SqsToolbox.Extensions package which provides extensions to integration with the Microsoft dependency injection and configuration libraries.

Add the latest alpha NuGet package from [nuget.org](https://www.nuget.org/packages/DotNetCloud.SqsToolbox.Extensions).

Inside an ASP.NET Core worker service you may register a polling queue reader as follows:

```csharp
services.AddPollingSqs(hostContext.Configuration.GetSection("TestQueue"))
    .Configure(c => c.UseExponentialBackoff = true)
    .WithBackgroundService()
    .WithMessageProcessor<QueueProcessor>()
    .WithDefaultExceptionHandler();
```

Various builder extension methods exist to constomise the queue reader and message consumption. These are optional and provide convience use for common scenarios.

The above code registers the polling queue reader, loading it's logical name and URL from an `IConfigurationSection`.

Additional configuration can be provided by calling the `Configure` method on the `ISqsPollingReaderBuilder`.

`WithBackgroundService` registers an `IHostedService` which will start and stop the queue reader for the `IHost`.

`WithMessageProcessor` allows you to register an special kind of `IHostedService` which consumes message from the channel. You must derive from the abstract `SqsMessageProcessingBackgroundService` class to provide the basic message handling functionality you require. 

For example:

```csharp
public class QueueProcessor : SqsMessageProcessingBackgroundService
{
    private readonly ILogger<QueueProcessor> _logger;

    public QueueProcessor(IChannelReaderAccessor channelReaderAccessor, ILogger<QueueProcessor> logger) 
        : base(channelReaderAccessor)
    {
        _logger = logger;
    }
    
    public override async Task ProcessFromChannel(ChannelReader<Message> channelReader, CancellationToken ct)
    {
        await foreach (var message in channelReader.ReadAllAsync(ct))
        {
            _logger.LogInformation(message.Body);

            foreach (var (key, value) in message.Attributes)
            {
                _logger.LogInformation($"{key} = {value}");
            }

            // more processing work
        }
    }
}
```

*NOTE: I'm considering changing the SqsMessageProcessingBackgroundService to wrap more of this so you simply provide an `Action<Message>`.*

Back to the builder, `WithDefaultExceptionHandler()` registered a simple exception handler which logs major failures, such as lack of queue permissions and then gracefully shuts down the host. You may provide a custom `IExceptionHandler` for this if you require different behaviour.

For more usage ideas, see the sample project.

# Diagnostics

I've started plumbing in some `DiagnosticListener` logging for activty tracing. This is available but not documented yet.

# Planned Features

## SQS Batch Deleter

Support for registering messages for deletion in batches, with an optional timer that triggers the batch if the batch size has not been met.

Status: Work in progress

# Feedback

I welcome ideas for features and improvements to be raised as issues which I will respond to as soon as I can. This is a hobby project so it may not be immediate!
