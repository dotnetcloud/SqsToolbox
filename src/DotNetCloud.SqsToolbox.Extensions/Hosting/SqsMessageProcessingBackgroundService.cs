using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Amazon.SQS.Model;
using DotNetCloud.SqsToolbox.Abstractions;
using Microsoft.Extensions.Hosting;

namespace DotNetCloud.SqsToolbox.Extensions.Hosting
{
    /// <summary>
    /// Base class for implementing long running queue processing.
    /// </summary>
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

        /// <summary>
        /// This method is called when the Microsoft.Extensions.Hosting.IHostedService
        /// starts. The implementation should return a task that represents a long running task
        /// which reads messages from a <see cref="ChannelReader{T}"/> of <see cref="Message"/>.
        /// </summary>
        /// <param name="channelReader">The <see cref="ChannelReader{T}"/> of <see cref="Message"/> fromw which to receive messages.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> triggered when the host is shutting down.</param>
        /// <returns>A <see cref="Task"/> that represents the long running channel reading.</returns>
        public abstract Task ProcessFromChannel(ChannelReader<Message> channelReader, CancellationToken cancellationToken);
    }
}
