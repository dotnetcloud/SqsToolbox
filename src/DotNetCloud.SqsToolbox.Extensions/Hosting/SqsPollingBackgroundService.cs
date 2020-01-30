using System.Threading;
using System.Threading.Tasks;
using DotNetCloud.SqsToolbox.Abstractions;
using Microsoft.Extensions.Hosting;

namespace DotNetCloud.SqsToolbox.Extensions.Hosting
{
    public class SqsPollingBackgroundService : IHostedService
    {
        private readonly ISqsPollingQueueReader _sqsPollingQueueReader;

        public SqsPollingBackgroundService(ISqsPollingQueueReader sqsPollingQueueReader)
        {
            _sqsPollingQueueReader = sqsPollingQueueReader;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _sqsPollingQueueReader.Start(cancellationToken);

            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _sqsPollingQueueReader.StopAsync();
        }
    }
}
