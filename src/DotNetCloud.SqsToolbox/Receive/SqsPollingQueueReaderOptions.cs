using System;
using Amazon.SQS.Model;

namespace DotNetCloud.SqsToolbox.Receive
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

        /// <summary>
        /// <para>The maximum number of messages to request per receive attempt.</para>
        /// <para>The value must be between 1 and 10. The default value is <value>10</value>.</para>
        /// </summary>
        public int MaxMessages { get; set; } = 10;

        public int PollTimeInSeconds { get; set; } = 20;

        public bool UseExponentialBackoff { get; set; } = true;

        public TimeSpan InitialDelay { get; set; } = TimeSpan.FromMinutes(1);

        public TimeSpan MaxDelay { get; set; } = TimeSpan.FromMinutes(5);

        public TimeSpan DelayWhenOverLimit { get; set; } = TimeSpan.FromMinutes(5);

        public ReceiveMessageRequest ReceiveMessageRequest { get; set; }
    }
}
