using System.Threading.Channels;
using Amazon.SQS.Model;

namespace DotNetCloud.SqsToolbox
{
    /// <summary>
    /// Provides access to a channel reader for polling queue reader.
    /// </summary>
    public interface IChannelReaderAccessor
    {
        /// <summary>
        /// Get the channel reader for a logical queue.
        /// </summary>
        /// <param name="logicalQueueName">The logical name of the queue.</param>
        /// <returns>A <inheritdoc cref="ChannelReader{T}"/> of <inheritdoc cref="Message"/>.</returns>
        ChannelReader<Message> GetChannelReader(string logicalQueueName);
    }
}
