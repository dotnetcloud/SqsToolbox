using System.Threading.Channels;
using Amazon.SQS.Model;

namespace DotNetCloud.SqsToolbox
{
    public interface ISqsMessageChannelFactory
    {
        Channel<Message> GetOrCreateChannel(string name);
    }
}
