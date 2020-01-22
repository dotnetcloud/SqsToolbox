using System;
using Amazon.SQS;
using DotNetCloud.SqsToolbox.Abstractions;
using DotNetCloud.SqsToolbox.BatchDelete;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace DotNetCloud.SqsToolbox.Extensions.DependencyInjection
{
    public static class SqsBatchDeleterServiceCollectionExtensions
    {

        public static ISqsBatchDeletionBuilder AddSqsBatchDeletion(this IServiceCollection services, IConfiguration configuration)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            AddBatchDeleterCore(services);

            var options = configuration.GetSqsBatchDeleterOptions();

            services.Configure<SqsBatchDeleterOptions>(opt =>
            {
                opt.QueueUrl = options.QueueUrl;
            });
            
            return new SqsBatchDeletionBuilder(services);
        }

        /// <summary>
        /// Adds the required SQS batch deletion services to the container, using the provided delegate to
        /// configure the <see cref="SqsBatchDeleter"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the SQS batch deletion services to.</param>
        /// <param name="configure">A delegate that is used to configure an <see cref="SqsBatchDeleter"/>.</param>
        /// <returns>An instance of <see cref="ISqsBatchDeletionBuilder"/> used to further configure the SQS batch deletion behaviour.</returns>
        public static ISqsBatchDeletionBuilder AddSqsBatchDeletion(this IServiceCollection services, Action<SqsBatchDeleterOptions> configure)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configure is null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            AddBatchDeleterCore(services);

            services.Configure(configure);

            return new SqsBatchDeletionBuilder(services);
        }

        private static void AddBatchDeleterCore(IServiceCollection services)
        {
            services.TryAddAWSService<IAmazonSQS>();

            services.TryAddSingleton<ISqsBatchDeleter, SqsBatchDeleter>();
            services.TryAddSingleton<ISqsBatchDeleteQueue, SqsBatchDeleteQueue>();

            services.TryAddSingleton(sp => sp.GetRequiredService<IOptions<SqsBatchDeleterOptions>>()?.Value);
        }
    }
}
