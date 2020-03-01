using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.SQS.Model;
using DotNetCloud.SqsToolbox.Abstractions;

namespace DotNetCloud.SqsToolbox.Receive
{
    /// <inheritdoc />
    public class SqsReceivePollDelayCalculator : ISqsReceivePollDelayCalculator
    {
        private readonly SqsPollingQueueReaderOptions _queueReaderOptions;
        private int _emptyResponseCounter;

        public SqsReceivePollDelayCalculator(SqsPollingQueueReaderOptions queueReaderOptions)
        {
            _queueReaderOptions = queueReaderOptions ?? throw new ArgumentNullException(nameof(queueReaderOptions));
        }

        /// <inheritdoc />
        public TimeSpan CalculateSecondsToDelay(IEnumerable<Message> messages)
        {
            _ = messages ?? throw new ArgumentNullException(nameof(messages));

            if (messages.Any())
            {
                _emptyResponseCounter = 0;

                return TimeSpan.Zero;
            }

            if (_emptyResponseCounter < 5)
            {
                _emptyResponseCounter++;
            }

            var delaySeconds = _queueReaderOptions.InitialDelay.TotalSeconds;

            if (_queueReaderOptions.UseExponentialBackoff)
            {
                delaySeconds = Math.Min(Math.Pow(delaySeconds, _emptyResponseCounter), _queueReaderOptions.MaxDelay.TotalSeconds);
            }
            
            return TimeSpan.FromSeconds(delaySeconds);
        }
    }
}
