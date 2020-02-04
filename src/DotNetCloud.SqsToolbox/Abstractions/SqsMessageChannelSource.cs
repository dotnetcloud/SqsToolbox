using System.Threading.Channels;
using Amazon.SQS.Model;

namespace DotNetCloud.SqsToolbox.Abstractions
{
    /// <summary>
    /// Base class for implementing a source of <see cref="Channel"/>
    /// </summary>
    public abstract class SqsMessageChannelSource
    {
        private static readonly object _lock = new object();

        /// <summary>
        /// The <see cref="Channel{Message}"/>.
        /// </summary>
        public Channel<Message> MessageChannel { get; private set; }

        internal Channel<Message> GetChannel()
        {
            if (MessageChannel is object) return MessageChannel;

            lock (_lock)
            {
                if (MessageChannel is object) return MessageChannel;

                MessageChannel = InitialiseChannel();
            }

            return MessageChannel;
        }

        /// <summary>
        /// Initialises a <see cref="Channel{Message}"/>.
        /// </summary>
        /// <returns>The initialised <see cref="Channel{Message}"/>.</returns>
        protected abstract Channel<Message> InitialiseChannel();
    }
}
