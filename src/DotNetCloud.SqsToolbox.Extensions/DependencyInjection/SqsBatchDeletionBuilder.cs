using System;
using DotNetCloud.SqsToolbox.Delete;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace DotNetCloud.SqsToolbox.Extensions.DependencyInjection
{
    internal sealed class SqsBatchDeletionBuilder : ISqsBatchDeletionBuilder
    {
        public SqsBatchDeletionBuilder(IServiceCollection services) => Services = services;

        public IServiceCollection Services { get; }

        public ISqsBatchDeletionBuilder WithBackgroundService()
        {
            Services.AddHostedService<SqsBatchDeleteBackgroundService>();

            return this;
        }

        public ISqsBatchDeletionBuilder Configure(Action<SqsBatchDeleterOptions> configure)
        {
            Services.PostConfigure(configure);

            Services.TryAddSingleton(sp => sp.GetRequiredService<IOptions<SqsBatchDeleterOptions>>()?.Value);

            return this;
        }
    }
}
