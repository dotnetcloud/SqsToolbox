using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SQS.Model;

namespace DotNetCloud.SqsToolbox.Abstractions
{
    public interface ISqsPollingDelayer
    {
        Task<int> Delay(IEnumerable<Message> messages, CancellationToken cancellationToken);
    }
}
