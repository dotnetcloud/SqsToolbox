using System.Threading.Channels;
using Amazon.SQS.Model;

namespace DotNetCloud.SqsToolbox.Extensions
{
    public interface ISqsMessageChannelFactory
    {
        Channel<Message> GetOrCreateChannel(string name);
    }
}
