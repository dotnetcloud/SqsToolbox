using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Amazon.SQS.Model;

namespace DotNetCloud.SqsToolbox.Core.Abstractions
{
    /// <summary>
    /// Once started, polls a queue for messages, writing them to a channel, until stopped.
    /// </summary>
    public interface ISqsPollingQueueReader
    {
        /// <summary>
        /// The <see cref="ChannelReader"/> from which received messages can be read for processing.
        /// </summary>
        ChannelReader<Message> ChannelReader { get; }

        /// <summary>
        /// Start polling the queue for messages.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the polling of the queue.</param>
        void Start(CancellationToken cancellationToken = default);

        /// <summary>
        /// Stop polling the queue for messages.
        /// </summary>
        Task StopAsync();
    }
}
