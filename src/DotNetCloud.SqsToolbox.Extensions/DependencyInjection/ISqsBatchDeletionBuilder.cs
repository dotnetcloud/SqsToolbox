using System;
using DotNetCloud.SqsToolbox.BatchDelete;

namespace DotNetCloud.SqsToolbox.Extensions.DependencyInjection
{
    public interface ISqsBatchDeletionBuilder
    {
        ISqsBatchDeletionBuilder WithBackgroundService();
        ISqsBatchDeletionBuilder Configure(Action<SqsBatchDeleterOptions> configure);
    }
}
