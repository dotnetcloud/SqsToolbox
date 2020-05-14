using System;
using System.Threading.Channels;
using Amazon.SQS.Model;
using DotNetCloud.SqsToolbox.Abstractions;
using DotNetCloud.SqsToolbox.Extensions.DependencyInjection;
using DotNetCloud.SqsToolbox.Extensions.Hosting;
using DotNetCloud.SqsToolbox.Receive;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace DotNetCloud.SqsToolbox.Extensions
{
    internal class DefaultSqsPollingQueueReaderBuilder : ISqsPollingReaderBuilder
    {
        public DefaultSqsPollingQueueReaderBuilder(IServiceCollection services, string name)
        {
            Services = services;
            Name = name;
        }

        public string Name { get; }

        public IServiceCollection Services { get; }
        
        public ISqsPollingReaderBuilder WithBackgroundService()
        {
            Services.AddSingleton<IHostedService>(serviceProvider => new SqsPollingBackgroundService(serviceProvider.GetRequiredService<ISqsPollingQueueReaderFactory>(), Name));

            return this;
        }

        public ISqsPollingReaderBuilder WithMessageProcessor<T>() where T : SqsMessageProcessingBackgroundService
        {
            Services.AddSingleton<IHostedService>(serviceProvider =>
            {
                var service = ActivatorUtilities.CreateInstance<T>(serviceProvider);

                service.SetName(Name);

                return service;
            });

            return this;
        }

        public ISqsPollingReaderBuilder WithExceptionHandler<T>() where T : IExceptionHandler
        {
            var type = typeof(T);

            Services.TryAddSingleton(typeof(IExceptionHandler), type);

            Services.Configure<SqsPollingQueueReaderFactoryOptions>(Name, opt => opt.ExceptionHandlerType = type);

            return this;
        }

        public ISqsPollingReaderBuilder Configure(Action<SqsPollingQueueReaderOptions> configure)
        {
            _ = configure ?? throw new ArgumentNullException(nameof(configure));

            Services.PostConfigure<SqsPollingQueueReaderFactoryOptions>(Name, opt => configure(opt.Options));
            
            return this;
        }

        public ISqsPollingReaderBuilder WithChannel(Channel<Message> channel)
        {
            Services.Configure<SqsPollingQueueReaderFactoryOptions>(Name, opt => opt.Channel = channel);

            return this;
        }

#if NETCOREAPP3_1
        public ISqsPollingReaderBuilder WithDefaultExceptionHandler()
        {
            Services.TryAddSingleton<IExceptionHandler, StopApplicationExceptionHandler>();

            return this;
        }
#endif
    }
}
