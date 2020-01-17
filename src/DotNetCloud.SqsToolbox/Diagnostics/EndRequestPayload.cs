namespace DotNetCloud.SqsToolbox.Diagnostics
{
    public sealed class EndRequestPayload
    {
        internal EndRequestPayload(string queueUrl, int messageCount)
        {
            QueueUrl = queueUrl;
            MessageCount = messageCount;
        }

        public string QueueUrl { get; }

        public int MessageCount { get; }

        public override string ToString() => $"Received {MessageCount} messages from {QueueUrl}";
    }
}
