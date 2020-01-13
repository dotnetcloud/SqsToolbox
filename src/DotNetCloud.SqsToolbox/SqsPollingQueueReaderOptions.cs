using System;

namespace DotNetCloud.SqsToolbox
{
    public class SqsPollingQueueReaderOptions
    {
        private string _queueUrl;

        public string QueueUrl
        {
            get => _queueUrl;
            set
            {
                if (!Uri.TryCreate(value, UriKind.Absolute, out _))
                {
                    throw new ArgumentException("The value must be a valid URI", nameof(value));
                }
                
                _queueUrl = value;
            }
        }

        public int ChannelCapacity { get; set; } = 100;

        public int MaxMessages { get; set; } = 10;

        public int PollTimeInSeconds { get; set; } = 20;
    }
}
