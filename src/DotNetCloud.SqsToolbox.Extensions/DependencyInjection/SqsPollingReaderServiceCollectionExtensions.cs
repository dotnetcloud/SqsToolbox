using System;
using Amazon.SQS;
using DotNetCloud.SqsToolbox.Abstractions;
using DotNetCloud.SqsToolbox.Extensions;
using DotNetCloud.SqsToolbox.Extensions.DependencyInjection;
using DotNetCloud.SqsToolbox.Extensions.Diagnostics;
using DotNetCloud.SqsToolbox.Extensions.Hosting;
using DotNetCloud.SqsToolbox.Receive;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for <see cref="ISqsPollingReaderBuilder"/>.
    /// </summary>
    public static class SqsPollingReaderServiceCollectionExtensions
    {
        public static ISqsPollingReaderBuilder AddPollingSqs(this IServiceCollection services, IConfigurationSection configurationSection)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configurationSection == null)
            {
                throw new ArgumentNullException(nameof(configurationSection));
            }

            var queueName = configurationSection["QueueName"];
            var queueUrl = configurationSection["QueueUrl"];

            if (string.IsNullOrEmpty(queueName) || string.IsNullOrEmpty(queueUrl))
                throw new InvalidOperationException("The configuration is invalid.");

            return services.AddPollingSqs(queueName, queueUrl);
        }

        public static ISqsPollingReaderBuilder AddDefaultPollingSqs<T>(this IServiceCollection services, IConfigurationSection configurationSection) where T : SqsMessageProcessingBackgroundService
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configurationSection == null)
            {
                throw new ArgumentNullException(nameof(configurationSection));
            }

            var queueName = configurationSection["QueueName"];
            var queueUrl = configurationSection["QueueUrl"];

            if (string.IsNullOrEmpty(queueName) || string.IsNullOrEmpty(queueUrl))
                throw new InvalidOperationException("The configuration is invalid.");

            return services.AddDefaultPollingSqs<T>(queueName, queueUrl);
        }

        public static ISqsPollingReaderBuilder AddPollingSqs(this IServiceCollection services, string name, string queueUrl)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }
            
            services.AddOptions();

            AddPollingSqsCore(services);

            services.TryAddSingleton<IChannelReaderAccessor, DefaultChannelReaderAccessor>();

            services.Configure<SqsPollingQueueReaderFactoryOptions>(name, options => options.Options.QueueUrl = queueUrl);

            return new DefaultSqsPollingQueueReaderBuilder(services, name);
        }

        public static ISqsPollingReaderBuilder AddDefaultPollingSqs<T>(this IServiceCollection services, string name, string queueUrl) where T : SqsMessageProcessingBackgroundService
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            services.AddOptions();

            AddPollingSqsCore(services);

            services.TryAddSingleton<IChannelReaderAccessor, DefaultChannelReaderAccessor>();

            services.Configure<SqsPollingQueueReaderFactoryOptions>(name, options => options.Options.QueueUrl = queueUrl);

            var builder = new DefaultSqsPollingQueueReaderBuilder(services, name);

            builder.WithBackgroundService();
            builder.WithMessageProcessor<T>();

#if NETCOREAPP3_1
            builder.WithDefaultExceptionHandler();
#endif
            return builder;
        }

        public static IServiceCollection AddSqsToolboxDiagnosticsMonitoring<T>(this IServiceCollection services) where T : DiagnosticsMonitoringService
        {
            _ = services ?? throw new ArgumentNullException(nameof(services));

            services.AddHostedService<T>();

            return services;
        }

        private static void AddPollingSqsCore(IServiceCollection services)
        {
            services.TryAddAWSService<IAmazonSQS>();

            services.TryAddSingleton<SqsPollingQueueReaderFactory>();
            services.TryAddSingleton<ISqsPollingQueueReaderFactory>(serviceProvider => serviceProvider.GetRequiredService<SqsPollingQueueReaderFactory>());
            services.TryAddSingleton<ISqsMessageChannelFactory>(serviceProvider => serviceProvider.GetRequiredService<SqsPollingQueueReaderFactory>());
        }
    }
}
