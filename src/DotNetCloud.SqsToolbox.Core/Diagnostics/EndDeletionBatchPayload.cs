using Amazon.SQS.Model;

namespace DotNetCloud.SqsToolbox.Core.Diagnostics
{
    internal sealed class EndDeletionBatchPayload
    {
        internal EndDeletionBatchPayload(DeleteMessageBatchResponse response, long millisecondsTaken)
        {
            DeleteMessageBatchResponse = response;
            MillisecondsTaken = millisecondsTaken;
        }

        public DeleteMessageBatchResponse DeleteMessageBatchResponse { get; }

        public long MillisecondsTaken { get; }

        public override string ToString() => $"Deleted batch with {DeleteMessageBatchResponse.Successful} items and {DeleteMessageBatchResponse.Failed} items, in {MillisecondsTaken} milliseconds";
    }
}
