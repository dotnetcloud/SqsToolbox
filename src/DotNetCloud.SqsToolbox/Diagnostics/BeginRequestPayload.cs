namespace DotNetCloud.SqsToolbox.Diagnostics
{
    public sealed class BeginRequestPayload
    {
        internal BeginRequestPayload(string queueUrl)
        {
            QueueUrl = queueUrl;
        }

        public string QueueUrl { get; }

        public override string ToString() => $"QueueUrl = {QueueUrl}";
    }
}
