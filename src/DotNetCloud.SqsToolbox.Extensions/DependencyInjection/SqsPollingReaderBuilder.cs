using System;
using DotNetCloud.SqsToolbox.Abstractions;
using DotNetCloud.SqsToolbox.Receive;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

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

        public ISqsPollingReaderBuilder WithExceptionHandler<T>() where T : ISqsPollingExceptionHandler
        {
            Services.RemoveAll(typeof(ISqsPollingExceptionHandler));

            Services.AddSingleton(typeof(ISqsPollingExceptionHandler), typeof(T));

            return this;
        }

        public ISqsPollingReaderBuilder Configure(Action<SqsPollingQueueReaderOptions> configure)
        {
            Services.PostConfigure(configure);

            Services.TryAddSingleton(sp => sp.GetRequiredService<IOptions<SqsPollingQueueReaderOptions>>()?.Value);

            return this;
        }

#if NETCOREAPP3_1
        public ISqsPollingReaderBuilder WithDefaultExceptionHandler()
        {
            Services.AddSingleton<ISqsPollingExceptionHandler, StopPollingExceptionHandler>();

            return this;
        }
#endif
    }
}
