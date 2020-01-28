using System.Threading.Channels;
using Amazon.SQS.Model;

namespace DotNetCloud.SqsToolbox.Receive
{
    public abstract class SqsQueueReaderChannelSource
    {
        public Channel<Message> _messageChannel;

        internal Channel<Message> GetChannel()
        {
            if (_messageChannel is object) return _messageChannel;

            _messageChannel = InitialiseChannel();

            return _messageChannel;
        }

        protected internal abstract Channel<Message> InitialiseChannel();
    }
}
