using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Amazon.SQS.Model;
using DotNetCloud.SqsToolbox.Abstractions;
using DotNetCloud.SqsToolbox.Extensions;
using Microsoft.Extensions.Logging;

namespace WorkerServiceSample
{
    public class Worker : SqsMessageProcessingBackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly ISqsBatchDeleteQueue _sqsBatchDeleteQueue;

        public Worker(ILogger<Worker> logger, ISqsPollingQueueReader sqsPollingQueueReader, ISqsBatchDeleteQueue sqsBatchDeleteQueue) 
            : base(sqsPollingQueueReader)
        {
            _logger = logger;
            _sqsBatchDeleteQueue = sqsBatchDeleteQueue;
        }

        public override async Task ProcessFromChannel(ChannelReader<Message> channelReader, CancellationToken cancellationToken)
        {
            await foreach (var message in channelReader.ReadAllAsync(cancellationToken))
            {
                _logger.LogInformation(message.Body);

                foreach (var (key, value) in message.Attributes)
                {
                    _logger.LogInformation($"{key} = {value}");
                }

                await _sqsBatchDeleteQueue.AddMessageAsync(message, cancellationToken);
            }
        }
    }
}
