using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Amazon.SQS.Model;
using Microsoft.Extensions.Hosting;

namespace DotNetCloud.SqsToolbox.Hosting
{
    /// <summary>
    /// Base class for implementing long running queue processing.
    /// </summary>
    public abstract class SqsMessageProcessingBackgroundService : BackgroundService
    {
        private readonly IChannelReaderAccessor _channelReaderAccessor;
        private bool _hasStarted;

        protected SqsMessageProcessingBackgroundService(IChannelReaderAccessor channelReaderAccessor)
        {
            _channelReaderAccessor = channelReaderAccessor ?? throw new ArgumentNullException(nameof(channelReaderAccessor));
        }

        protected string Name { get; private set; }

        /// <summary>
        /// Sets the logical name of the channel to process.
        /// </summary>
        /// <param name="name">The logical name of the channel.</param>
        internal void SetName(string name)
        {
            if (Name is object)
                throw new InvalidOperationException("Name cannot be set twice.");

            if (_hasStarted)
                throw new InvalidOperationException("Name cannot be set once the service has been started.");

            Name = name;
        }

        /// <inheritdoc />
        protected sealed override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await ProcessFromChannelAsync(_channelReaderAccessor.GetChannelReader(Name), stoppingToken);
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(Name)) return Task.CompletedTask;

            _hasStarted = true;

            return base.StartAsync(cancellationToken);
        }

        /// <summary>
        /// This method is called when the Microsoft.Extensions.Hosting.IHostedService
        /// starts. The implementation should return a task that represents a long running task
        /// which reads messages from a <see cref="ChannelReader{T}"/> of <see cref="Message"/>.
        /// </summary>
        /// <param name="channelReader">The <see cref="ChannelReader{T}"/> of <see cref="Message"/> from which to receive messages.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> triggered when the host is shutting down.</param>
        /// <returns>A <see cref="Task"/> that represents the long running channel reading.</returns>
        public abstract Task ProcessFromChannelAsync(ChannelReader<Message> channelReader, CancellationToken cancellationToken = default);
    }
}
