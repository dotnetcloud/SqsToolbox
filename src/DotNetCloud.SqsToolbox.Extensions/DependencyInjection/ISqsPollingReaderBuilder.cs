using System;
using DotNetCloud.SqsToolbox.Abstractions;
using DotNetCloud.SqsToolbox.Receive;

namespace DotNetCloud.SqsToolbox.Extensions.DependencyInjection
{
    public interface ISqsPollingReaderBuilder
    {
        ISqsPollingReaderBuilder WithBackgroundService();
        ISqsPollingReaderBuilder WithMessageProcessor<T>() where T : SqsMessageProcessingBackgroundService;
        ISqsPollingReaderBuilder WithExceptionHandler<T>() where T : IExceptionHandler;
        ISqsPollingReaderBuilder Configure(Action<SqsPollingQueueReaderOptions> configure);

#if NETCOREAPP3_1
        ISqsPollingReaderBuilder WithDefaultExceptionHandler();
#endif
    }
}
