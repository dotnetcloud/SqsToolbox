namespace DotNetCloud.SqsToolbox.Receive
{
    public static class SqsPollingQueueReaderOptionsExtensions
    {
        public static void CopyFrom(this SqsPollingQueueReaderOptions destination, SqsPollingQueueReaderOptions source)
        {
            destination.QueueUrl = source.QueueUrl;
            destination.ChannelCapacity = source.ChannelCapacity;
            destination.MaxMessages = source.MaxMessages;
            destination.PollTimeInSeconds = source.PollTimeInSeconds;
            destination.InitialDelay = source.InitialDelay;
            destination.MaxDelay = source.MaxDelay;
            destination.DelayWhenOverLimit = source.DelayWhenOverLimit;
            destination.ReceiveMessageRequest = source.ReceiveMessageRequest;
        }
    }
}
