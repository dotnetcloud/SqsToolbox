using System.Threading.Channels;
using Amazon.SQS.Model;

namespace DotNetCloud.SqsToolbox.Extensions
{
    public sealed class DefaultChannelReaderAccessor : IChannelReaderAccessor
    {
        private readonly ISqsMessageChannelFactory _channelFactory;

        public DefaultChannelReaderAccessor(ISqsMessageChannelFactory channelFactory)
        {
            _channelFactory = channelFactory;
        }

        public ChannelReader<Message> GetChannelReader(string logicalQueueName) => _channelFactory.GetOrCreateChannel(logicalQueueName).Reader;
    }
}
