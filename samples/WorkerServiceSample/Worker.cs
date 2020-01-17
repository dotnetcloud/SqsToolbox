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

        public Worker(ILogger<Worker> logger, ISqsPollingQueueReader sqsPollingQueueReader) 
            : base(sqsPollingQueueReader)
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
            }
        }
    }
}
