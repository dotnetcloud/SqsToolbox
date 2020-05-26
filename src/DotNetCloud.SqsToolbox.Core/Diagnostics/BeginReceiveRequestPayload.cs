namespace DotNetCloud.SqsToolbox.Core.Diagnostics
{
    public sealed class BeginReceiveRequestPayload
    {
        internal BeginReceiveRequestPayload(string queueUrl)
        {
            QueueUrl = queueUrl;
        }

        public string QueueUrl { get; }

        public override string ToString() => $"QueueUrl = {QueueUrl}";
    }
}
