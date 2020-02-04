namespace DotNetCloud.SqsToolbox.Diagnostics
{
    public static class DiagnosticEvents
    {
        public const string PollingForMessages = "DotNetCloud.SqsToolbox.PollingForMessages";

        public const string PollingForMessagesStart = "DotNetCloud.SqsToolbox.PollingForMessages.Start";

        public const string ReceiveMessagesBeginRequest = "DotNetCloud.SqsToolbox.ReceiveMessagesBeginRequest";

        public const string ReceiveMessagesRequestComplete = "DotNetCloud.SqsToolbox.ReceiveMessagesRequestComplete";

        public const string OverLimitException = "DotNetCloud.SqsToolbox.OverLimitException";

        public const string AmazonSqsException = "DotNetCloud.SqsToolbox.AmazonSQSException";

        public const string Exception = "DotNetCloud.SqsToolbox.Exception";

        public const string DeletionBatchCreated = "DotNetCloud.SqsToolbox.DeletionBatchCreated";

        public const string DeleteBatchRequestComplete = "DotNetCloud.SqsToolbox.DeleteBatchRequestComplete";
    }
}
`
