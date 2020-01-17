namespace DotNetCloud.SqsToolbox.Diagnostics
{
    public class EndRequestPayload
    {
        public string QueueUrl { get; set; }

        public int MessageCount { get; set; }
    }
}
