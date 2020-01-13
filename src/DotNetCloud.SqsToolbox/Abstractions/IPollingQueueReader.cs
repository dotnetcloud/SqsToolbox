using System.Threading.Channels;

namespace DotNetCloud.SqsToolbox.Abstractions
{
    /// <summary>
    /// Once started, polls a queue for messages, writing them to a channel, until stopped.
    /// </summary>
    public interface IPollingQueueReader<T> where T : class
    {
        /// <summary>
        /// The <see cref="ChannelReader"/> from which received messages can be read for processing.
        /// </summary>
        ChannelReader<T> ChannelReader { get; }

        /// <summary>
        /// Start polling the queue for messages.
        /// </summary>
        void Start();

        /// <summary>
        /// Stop polling the queue for messages.
        /// </summary>
        void Stop();
    }
}
