using System.Threading.Channels;
using Amazon.SQS.Model;
using DotNetCloud.SqsToolbox.Receive;

namespace WorkerServiceSample
{
    public class CustomSqsQueueReaderChannelSource : SqsQueueReaderChannelSource
    {
        private readonly ReceiveQueueChannel _receiveQueueChannel;

        public CustomSqsQueueReaderChannelSource(ReceiveQueueChannel receiveQueueChannel)
        {
            _receiveQueueChannel = receiveQueueChannel;
        }

        protected override Channel<Message> InitialiseChannel()
        {
            return _receiveQueueChannel.Channel;
        }
    }
}
