using Microsoft.Extensions.DependencyInjection;

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
    }
}
