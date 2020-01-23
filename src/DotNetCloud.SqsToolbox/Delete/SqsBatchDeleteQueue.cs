using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SQS.Model;
using DotNetCloud.SqsToolbox.Abstractions;

namespace DotNetCloud.SqsToolbox.Delete
{
    public class SqsBatchDeleteQueue : ISqsBatchDeleteQueue
    {
        private readonly ISqsBatchDeleter _batchDeleter;

        public SqsBatchDeleteQueue(ISqsBatchDeleter batchDeleter) => _batchDeleter = batchDeleter;

        public Task AddMessageAsync(Message message, CancellationToken cancellationToken = default) =>
            _batchDeleter.AddMessageAsync(message, cancellationToken);

        public Task AddMessagesAsync(IList<Message> messages, CancellationToken cancellationToken = default) =>
            _batchDeleter.AddMessagesAsync(messages, cancellationToken);
    }
}
