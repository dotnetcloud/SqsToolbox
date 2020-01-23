using System;

namespace DotNetCloud.SqsToolbox.Delete
{
    public class SqsBatchDeleterOptions
    {
        private string _queueUrl;
        private int _batchSize = 10;

        public string QueueUrl
        {
            get => _queueUrl;
            set
            {
                if (!Uri.TryCreate(value, UriKind.Absolute, out _))
                    throw new ArgumentException("The value must be a valid URI", nameof(value));
                
                _queueUrl = value;
            }
        }

        public int ChannelCapacity { get; set; } = 100;

        public int BatchSize
        {
            get => _batchSize;
            set
            {
                if (value > 10)
                    throw new ArgumentOutOfRangeException(nameof(value), "The value must be between 1 and 10 inclusive.");

                _batchSize = value;
            }
        } 

        public TimeSpan MaxWaitForFullBatch { get; set; } = TimeSpan.FromSeconds(30);

        public bool DrainOnStop { get; set; }
    }
}
