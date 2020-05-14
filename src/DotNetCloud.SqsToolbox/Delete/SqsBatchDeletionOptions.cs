using System;
using System.Diagnostics;

namespace DotNetCloud.SqsToolbox.Delete
{
    /// <summary>
    /// Provides options used to configure the processing performed by an <see cref="SqsBatchDeleter"/>.
    /// </summary>
    [DebuggerDisplay("QueueUrl = {QueueUrl}")]
    internal class SqsBatchDeletionOptions
    {
        private string _queueUrl;
        private int _batchSize = 10;

        /// <summary>
        /// The URL of the SQS queue from which to delete messages.
        /// </summary>
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

        /// <summary>
        /// The capacity of the channel which controls back-pressure in cases where producer(s) outpace the <see cref="SqsBatchDeleter"/>.
        /// </summary>
        public int ChannelCapacity { get; set; } = 100;

        /// <summary>
        /// The number of messages to include in each batch deletion request.
        /// </summary>
        public int BatchSize
        {
            get => _batchSize;
            set
            {
                if (value < 1 || value > 10)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "The value must be between 1 and 10 inclusive.");
                }

                _batchSize = value;
            }
        } 

        /// <summary>
        /// The maximum <see cref="TimeSpan"/> to wait for before forcing a batch deletion request despite the required batch size not being reached.
        /// </summary>
        public TimeSpan MaxWaitForFullBatch { get; set; } = TimeSpan.FromSeconds(60);

        /// <summary>
        /// When stopping, should any queued messages be deleted until the internal channel is deleted.
        /// </summary>
        public bool DrainOnStop { get; set; }
        
        /// <summary>A default instance of <see cref="SqsBatchDeletionOptions"/>.</summary>
        /// <remarks>
        /// Do not change the values of this instance.  It is shared by all of our blocks when no options are provided by the user.
        /// </remarks>
        internal static readonly SqsBatchDeletionOptions Default = new SqsBatchDeletionOptions();

        /// <summary>
        /// Returns a cloned instance of this <see cref="SqsBatchDeletionOptions"/>.
        /// </summary>
        /// <returns>
        /// An instance of the options that may be cached by the <see cref="SqsBatchDeleter"/>.
        /// </returns>
        internal SqsBatchDeletionOptions Clone() =>
            new SqsBatchDeletionOptions
                {
                    QueueUrl = QueueUrl,
                    ChannelCapacity = ChannelCapacity,
                    BatchSize = BatchSize,
                    MaxWaitForFullBatch = MaxWaitForFullBatch,
                    DrainOnStop = DrainOnStop
                };
    }
}
