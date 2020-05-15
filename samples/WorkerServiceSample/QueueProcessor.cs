using System.Threading;
using System.Threading.Tasks;
using Amazon.SQS.Model;
using DotNetCloud.SqsToolbox.Extensions;
using DotNetCloud.SqsToolbox.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WorkerServiceSample
{
    public class QueueProcessor : MessageProcessorService
    {
        private readonly ILogger<QueueProcessor> _logger;

        public QueueProcessor(IChannelReaderAccessor channelReaderAccessor, ILogger<QueueProcessor> logger) : base(channelReaderAccessor)
        {
            _logger = logger;
        }

        public override Task ProcessMessageAsync(Message message, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(message.Body);

            foreach (var (key, value) in message.Attributes)
            {
                _logger.LogInformation($"{key} = {value}");
            }

            // more processing / deletion etc.

            return Task.CompletedTask;
        }
    }
}
