using System;
using Amazon.SQS.Model;
using DotNetCloud.SqsToolbox.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DotNetCloud.SqsToolbox.Extensions.DependencyInjection
{
    public static class SqsPollingReaderServiceCollectionExtensions
    {
        public static IServiceCollection AddPollingSqs(this IServiceCollection services, string queueUrl)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (string.IsNullOrEmpty(queueUrl))
            {
                throw new ArgumentNullException(nameof(queueUrl));
            }

            services.AddSingleton(new SqsPollingQueueReaderOptions{ QueueUrl = queueUrl });
            services.AddSingleton<IPollingQueueReader<Message>, SqsPollingQueueReader>();
            
            return services;
        }

        public static IServiceCollection AddPollingSqs(this IServiceCollection services, Action<SqsPollingQueueReaderOptions> configure)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configure is null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            services.Configure(configure);

            services.AddSingleton(sp => sp.GetRequiredService<IOptions<SqsPollingQueueReaderOptions>>()?.Value);

            services.AddSingleton<IPollingQueueReader<Message>, SqsPollingQueueReader>();

            return services;
        }
    }
}
