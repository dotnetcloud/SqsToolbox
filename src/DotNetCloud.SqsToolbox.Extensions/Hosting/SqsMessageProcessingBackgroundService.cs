using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Amazon.SQS.Model;
using DotNetCloud.SqsToolbox.Abstractions;
using Microsoft.Extensions.Hosting;

namespace DotNetCloud.SqsToolbox.Extensions.Hosting
{
    public abstract class SqsMessageProcessingBackgroundService : BackgroundService
    {
        private readonly ISqsPollingQueueReader _sqsPollingQueueReader;

        protected SqsMessageProcessingBackgroundService(ISqsPollingQueueReader sqsPollingQueueReader)
        {
            _sqsPollingQueueReader = sqsPollingQueueReader ?? throw new ArgumentNullException(nameof(sqsPollingQueueReader));
        }

        protected sealed override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await ProcessFromChannel(_sqsPollingQueueReader.ChannelReader, stoppingToken);
        }

        public abstract Task ProcessFromChannel(ChannelReader<Message> channelReader, CancellationToken cancellationToken);
    }
}
