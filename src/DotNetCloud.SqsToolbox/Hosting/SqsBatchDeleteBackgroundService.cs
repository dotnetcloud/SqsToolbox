using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace DotNetCloud.SqsToolbox.Hosting
{
    //public class SqsBatchDeleteBackgroundService : IHostedService
    //{
    //    private readonly ISqsBatchDeleter _sqsBatchDeleter;

    //    public SqsBatchDeleteBackgroundService(ISqsBatchDeleter sqsBatchDeleter)
    //    {
    //        _sqsBatchDeleter = sqsBatchDeleter;
    //    }

    //    public Task StartAsync(CancellationToken cancellationToken)
    //    {
    //        _sqsBatchDeleter.Start(cancellationToken);

    //        return Task.CompletedTask;
    //    }

    //    public async Task StopAsync(CancellationToken cancellationToken)
    //    {
    //        await _sqsBatchDeleter.StopAsync();
    //    }
    //}
}
