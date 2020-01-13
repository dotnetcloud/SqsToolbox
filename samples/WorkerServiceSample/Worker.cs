using System.Threading;
using System.Threading.Tasks;
using Amazon.SQS.Model;
using DotNetCloud.SqsToolbox.Abstractions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WorkerServiceSample
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IPollingQueueReader<Message> _pollingQueue;

        public Worker(ILogger<Worker> logger, IPollingQueueReader<Message> pollingQueue)
        {
            _logger = logger;
            _pollingQueue = pollingQueue;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _pollingQueue.Start(stoppingToken);

            await foreach (var message in _pollingQueue.ChannelReader.ReadAllAsync(stoppingToken))
            {
                _logger.LogInformation(message.Body);
            }

            await _pollingQueue.StopAsync();
        }
    }
}
