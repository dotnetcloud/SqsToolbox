using System.Threading.Channels;
using Amazon.SQS.Model;

namespace DotNetCloud.SqsToolbox.Abstractions
{
    /// <summary>
    /// Base class for implementing a source of a <see cref="Channel{T}"/> of <see cref="Message"/>.
    /// </summary>
    public abstract class SqsMessageChannelSource
    {
        private static readonly object _lock = new object();

        private Channel<Message> _messageChannel;

        /// <summary>
        /// Get an instance of a <see cref="Channel{T}"/> of <see cref="Message"/>.
        /// </summary>
        /// <returns>A <see cref="Channel{T}"/> of <see cref="Message"/>.</returns>
        public Channel<Message> GetChannel()
        {
            if (_messageChannel is object) return _messageChannel;

            lock (_lock)
            {
                if (_messageChannel is object) return _messageChannel;

                _messageChannel = InitialiseChannel();
            }

            return _messageChannel;
        }

        /// <summary>
        /// Initialises a <see cref="Channel{T}"/> of <see cref="Message"/>.
        /// </summary>
        /// <returns>The initialised <see cref="Channel{T}"/>.</returns>
        protected abstract Channel<Message> InitialiseChannel();
    }
}
