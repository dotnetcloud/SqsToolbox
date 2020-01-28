namespace DotNetCloud.SqsToolbox.Diagnostics
{
    public sealed class DeletionBatchCreatedPayload
    {
        internal DeletionBatchCreatedPayload(int messageCount, long millisecondsTaken)
        {
            MessageCount = messageCount;
            MillisecondsTaken = millisecondsTaken;
        }

        public int MessageCount { get; }

        public long MillisecondsTaken { get; }

        public override string ToString() => $"Created batch with {MessageCount} items, in {MillisecondsTaken} milliseconds";
    }
}
