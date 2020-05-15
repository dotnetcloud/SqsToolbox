using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace DotNetCloud.SqsToolbox.Extensions.Hosting
{
    public class SqsPollingBackgroundService : IHostedService
    {
        private readonly ISqsPollingQueueReaderFactory _sqsPollingQueueReader;
        private readonly string _name;

        public SqsPollingBackgroundService(ISqsPollingQueueReaderFactory sqsPollingQueueReader, string name)
        {
            _sqsPollingQueueReader = sqsPollingQueueReader;
            _name = name;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _sqsPollingQueueReader.GetOrCreateReader(_name).Start(cancellationToken);

            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _sqsPollingQueueReader.GetOrCreateReader(_name).StopAsync().ConfigureAwait(false);
        }
    }
}
