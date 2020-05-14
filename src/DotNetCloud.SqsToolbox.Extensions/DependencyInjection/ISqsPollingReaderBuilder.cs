using System;
using System.Threading.Channels;
using Amazon.SQS.Model;
using DotNetCloud.SqsToolbox.Abstractions;
using DotNetCloud.SqsToolbox.Extensions.Hosting;
using DotNetCloud.SqsToolbox.Receive;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetCloud.SqsToolbox.Extensions.DependencyInjection
{
    public interface ISqsPollingReaderBuilder
    {       
        /// <summary>
        /// Gets the name of the client configured by this builder.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the application service collection.
        /// </summary>
        IServiceCollection Services { get; }

        ISqsPollingReaderBuilder WithBackgroundService();
        ISqsPollingReaderBuilder WithMessageProcessor<T>() where T : SqsMessageProcessingBackgroundService;
        ISqsPollingReaderBuilder WithExceptionHandler<T>() where T : IExceptionHandler;
        ISqsPollingReaderBuilder Configure(Action<SqsPollingQueueReaderOptions> configure);
        ISqsPollingReaderBuilder WithChannel(Channel<Message> channel);

#if NETCOREAPP3_1
        ISqsPollingReaderBuilder WithDefaultExceptionHandler();
#endif
    }
}
