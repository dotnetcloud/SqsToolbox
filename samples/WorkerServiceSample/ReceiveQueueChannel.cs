using System.Threading.Channels;
using Amazon.SQS.Model;

namespace WorkerServiceSample
{
    public class ReceiveQueueChannel
    {
        public Channel<Message> Channel => System.Threading.Channels.Channel.CreateBounded<Message>(500);
    }
}
