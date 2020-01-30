using System;
using DotNetCloud.SqsToolbox.Abstractions;
using DotNetCloud.SqsToolbox.Extensions.Hosting;
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

        public ISqsPollingReaderBuilder WithExceptionHandler<T>() where T : IExceptionHandler
        {
            Services.RemoveAll(typeof(IExceptionHandler));

            Services.AddSingleton(typeof(IExceptionHandler), typeof(T));

            return this;
        }

        public ISqsPollingReaderBuilder Configure(Action<SqsPollingQueueReaderOptions> configure)
        {
            Services.PostConfigure(configure);

            Services.TryAddSingleton(sp => sp.GetRequiredService<IOptions<SqsPollingQueueReaderOptions>>()?.Value);

            return this;
        }

        public ISqsPollingReaderBuilder WithChannelSource<T>() where T : SqsQueueReaderChannelSource
        {
            Services.RemoveAll<SqsQueueReaderChannelSource>();

            Services.TryAddSingleton<SqsQueueReaderChannelSource, T>();

            return this;
        }

#if NETCOREAPP3_1
        public ISqsPollingReaderBuilder WithDefaultExceptionHandler()
        {
            Services.AddSingleton<IExceptionHandler, StopApplicationExceptionHandler>();

            return this;
        }
#endif
    }
}
