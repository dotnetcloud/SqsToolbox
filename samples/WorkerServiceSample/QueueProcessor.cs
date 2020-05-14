using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Amazon.SQS.Model;
using DotNetCloud.SqsToolbox.Extensions;
using DotNetCloud.SqsToolbox.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WorkerServiceSample
{
    public class QueueProcessor : SqsMessageProcessingBackgroundService
    {
        private readonly ILogger<QueueProcessor> _logger;

        public QueueProcessor(IChannelReaderAccessor channelReaderAccessor, ILogger<QueueProcessor> logger) : base(channelReaderAccessor)
        {
            _logger = logger;
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

                //await _sqsBatchDeleteQueue.AddMessageAsync(message, cancellationToken);
            }
        }
    }
}
