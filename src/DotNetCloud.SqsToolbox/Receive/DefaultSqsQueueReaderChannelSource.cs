using System;
using System.Threading.Channels;
using Amazon.SQS.Model;

namespace DotNetCloud.SqsToolbox.Receive
{
    public class DefaultSqsQueueReaderChannelSource : SqsQueueReaderChannelSource
    {
        private readonly int _capacity;

        public DefaultSqsQueueReaderChannelSource(SqsPollingQueueReaderOptions queueReaderOptions)
        {
            _ = queueReaderOptions ?? throw new ArgumentNullException(nameof(queueReaderOptions));

            _capacity = queueReaderOptions.ChannelCapacity;
        }

        public DefaultSqsQueueReaderChannelSource(int capacity)
        {
            if (capacity < 1) throw new ArgumentOutOfRangeException(nameof(capacity));

            _capacity = capacity;
        }

        protected override Channel<Message> InitialiseChannel()
        {
            return Channel.CreateBounded<Message>(new BoundedChannelOptions(_capacity));
        }
    }
}
