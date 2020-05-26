using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SQS.Model;

namespace DotNetCloud.SqsToolbox.Core.Abstractions
{
    internal interface ISqsBatchDeleteQueue
    {
        Task AddMessageAsync(Message message, CancellationToken cancellationToken = default);
        Task AddMessagesAsync(IList<Message> messages, CancellationToken cancellationToken = default);
    }
}
