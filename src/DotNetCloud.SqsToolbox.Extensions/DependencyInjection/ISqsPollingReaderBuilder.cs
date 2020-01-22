using DotNetCloud.SqsToolbox.Abstractions;

namespace DotNetCloud.SqsToolbox.Extensions.DependencyInjection
{
    public interface ISqsPollingReaderBuilder
    {
        ISqsPollingReaderBuilder WithBackgroundService();
        ISqsPollingReaderBuilder WithMessageProcessor<T>() where T : SqsMessageProcessingBackgroundService;
        ISqsPollingReaderBuilder WithExceptionHandler<T>() where T : IPollingSqsExceptionHandler;
    }
}
