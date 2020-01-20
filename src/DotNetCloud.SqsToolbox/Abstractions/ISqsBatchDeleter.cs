using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SQS.Model;

namespace DotNetCloud.SqsToolbox.Abstractions
{
    public interface ISqsBatchDeleter
    {
        void Start(CancellationToken cancellationToken = default);
        Task StopAsync();
        Task AddMessageAsync(Message message, CancellationToken cancellationToken = default);
        Task AddMessagesAsync(IList<Message> messages, CancellationToken cancellationToken = default);
    }
}
