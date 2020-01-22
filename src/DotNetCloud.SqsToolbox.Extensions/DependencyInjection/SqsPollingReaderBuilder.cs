using DotNetCloud.SqsToolbox.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DotNetCloud.SqsToolbox.Extensions.DependencyInjection
{
    public class SqsPollingReaderBuilder : ISqsPollingReaderBuilder
    {
        public SqsPollingReaderBuilder(IServiceCollection services) => Services = services;

        public IServiceCollection Services { get; }

        public ISqsPollingReaderBuilder WithBackgroundService()
        {
            Services.AddHostedService<SqsPollingBackgroundService>();

            return this;
        }

        public ISqsPollingReaderBuilder WithMessageProcessor<T>() where T : SqsMessageProcessingBackgroundService
        {
            Services.AddHostedService<T>();

            return this;
        }

        public ISqsPollingReaderBuilder WithExceptionHandler<T>() where T : IPollingSqsExceptionHandler
        {
            Services.RemoveAll(typeof(IPollingSqsExceptionHandler));

            Services.AddSingleton(typeof(IPollingSqsExceptionHandler), typeof(T));

            return this;
        }
    }
}
