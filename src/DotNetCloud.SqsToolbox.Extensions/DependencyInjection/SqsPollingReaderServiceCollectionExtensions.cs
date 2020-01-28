using System;
using Amazon.SQS;
using DotNetCloud.SqsToolbox.Abstractions;
using DotNetCloud.SqsToolbox.Extensions.Diagnostics;
using DotNetCloud.SqsToolbox.Receive;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace DotNetCloud.SqsToolbox.Extensions.DependencyInjection
{
    public static class SqsPollingReaderServiceCollectionExtensions
    {
        public static ISqsPollingReaderBuilder AddPollingSqsFromConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            var options = configuration.GetPollingQueueReaderOptions();
            
            AddPollingSqsCore(services);

            services.TryAddSingleton(options);

            return new SqsPollingReaderBuilder(services);
        }

        public static ISqsPollingReaderBuilder AddPollingSqs(this IServiceCollection services, string queueUrl)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (string.IsNullOrEmpty(queueUrl))
            {
                throw new ArgumentNullException(nameof(queueUrl));
            }

            AddPollingSqsCore(services);

            services.TryAddSingleton(new SqsPollingQueueReaderOptions { QueueUrl = queueUrl });

            return new SqsPollingReaderBuilder(services);
        }

        public static ISqsPollingReaderBuilder AddPollingSqs(this IServiceCollection services, Action<SqsPollingQueueReaderOptions> configure)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configure is null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            AddPollingSqsCore(services);

            services.Configure(configure);

            services.TryAddSingleton(sp => sp.GetRequiredService<IOptions<SqsPollingQueueReaderOptions>>()?.Value);

            return new SqsPollingReaderBuilder(services);
        }

        public static IServiceCollection AddSqsToolboxDiagnosticsMonitoring<T>(this IServiceCollection services) where T : DiagnosticsMonitoringService
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddHostedService<T>();

            return services;
        }

        private static void AddPollingSqsCore(IServiceCollection services)
        {
            services.TryAddAWSService<IAmazonSQS>();

            services.TryAddSingleton<ISqsPollingDelayer, SqsPollingDelayer>();
            services.TryAddSingleton<ISqsPollingQueueReader, SqsPollingQueueReader>();
            services.TryAddSingleton<IExceptionHandler, DefaultExceptionHandler>();
        }
    }
}
