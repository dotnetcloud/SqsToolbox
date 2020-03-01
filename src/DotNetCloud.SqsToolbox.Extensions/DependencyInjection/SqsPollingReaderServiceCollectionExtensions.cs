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
    /// <summary>
    /// Extension methods for <see cref="ISqsPollingReaderBuilder"/>.
    /// </summary>
    public static class SqsPollingReaderServiceCollectionExtensions
    {
        /// <summary>
        /// Add services required for polling from an SQS queue, attempting to parse options from <see cref="IConfiguration"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the polling SQS services to.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> for the application.</param>
        /// <returns>An instance of <see cref="ISqsPollingReaderBuilder"/> from which health checks can be registered.</returns>
        public static ISqsPollingReaderBuilder AddPollingSqs(this IServiceCollection services, IConfiguration configuration)
        {
            _ = services ?? throw new ArgumentNullException(nameof(services));
            _ = configuration ?? throw new ArgumentNullException(nameof(configuration));
            
            AddPollingSqsCore(services);

            var options = configuration.GetPollingQueueReaderOptions();
            
            services.Configure<SqsPollingQueueReaderOptions>(opt =>
            {
                opt.CopyFrom(options);
            });

            services.TryAddSingleton(sp => sp.GetRequiredService<IOptions<SqsPollingQueueReaderOptions>>().Value);

            return new SqsPollingReaderBuilder(services);
        }

        /// <summary>
        /// Add services required for polling from an SQS queue using the provided <paramref name="queueUrl"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the polling SQS services to.</param>
        /// <param name="queueUrl">The URL of the SQS Queue to poll from.</param>
        /// <returns></returns>
        public static ISqsPollingReaderBuilder AddPollingSqs(this IServiceCollection services, string queueUrl)
        {
            _ = services ?? throw new ArgumentNullException(nameof(services));
            _ = queueUrl ?? throw new ArgumentNullException(nameof(queueUrl));

            AddPollingSqsCore(services);

            services.Configure<SqsPollingQueueReaderOptions>(opt =>
            {
                opt.QueueUrl = queueUrl;
            });

            services.TryAddSingleton(sp => sp.GetRequiredService<IOptions<SqsPollingQueueReaderOptions>>().Value);

            return new SqsPollingReaderBuilder(services);
        }

        /// <summary>
        /// Add services required for polling from an SQS queue.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the polling SQS services to.</param>
        /// <param name="configure">The <see cref="Action{SqsPollingQueueReaderOptions}"/> to configure the provided <see cref="SqsPollingQueueReaderOptions"/>.</param>
        /// <returns></returns>
        public static ISqsPollingReaderBuilder AddPollingSqs(this IServiceCollection services, Action<SqsPollingQueueReaderOptions> configure)
        {
            _ = services ?? throw new ArgumentNullException(nameof(services));
            _ = configure ?? throw new ArgumentNullException(nameof(configure));

            AddPollingSqsCore(services);

            services.Configure(configure);

            services.TryAddSingleton(sp => sp.GetRequiredService<IOptions<SqsPollingQueueReaderOptions>>().Value);

            return new SqsPollingReaderBuilder(services);
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

            services.TryAddSingleton<ISqsReceiveDelayCalculator, SqsReceiveDelayCalculator>();
            services.TryAddSingleton<ISqsPollingQueueReader, SqsPollingQueueReader>();
            services.TryAddSingleton<IExceptionHandler, DefaultExceptionHandler>();

            services.TryAddSingleton<SqsMessageChannelSource, DefaultSqsQueueReaderChannelSource>();
        }
    }
}
