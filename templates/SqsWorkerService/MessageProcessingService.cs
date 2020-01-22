using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Amazon.SQS.Model;
using DotNetCloud.SqsToolbox.Abstractions;
using DotNetCloud.SqsToolbox.Extensions;
using Microsoft.Extensions.Logging;

namespace DotNetCloud.SqsWorkerService
{
    public class MessageProcessingService : SqsMessageProcessingBackgroundService
    {
        private readonly ILogger<MessageProcessingService> _logger;
        private readonly ISqsBatchDeleteQueue _sqsBatchDeleteQueue;

        public MessageProcessingService(ILogger<MessageProcessingService> logger, ISqsPollingQueueReader sqsPollingQueueReader, ISqsBatchDeleteQueue sqsBatchDeleteQueue)
            : base(sqsPollingQueueReader)
        {
            _logger = logger;
            _sqsBatchDeleteQueue = sqsBatchDeleteQueue;
        }

        public override async Task ProcessFromChannel(ChannelReader<Message> channelReader, CancellationToken cancellationToken)
        {
            await foreach (var message in channelReader.ReadAllAsync(cancellationToken))
            {
                _logger.LogInformation($"Processing {message.MessageId}");

                // TODO: Message processing

                await _sqsBatchDeleteQueue.AddMessageAsync(message, cancellationToken);
            }
        }
    }
}
