using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.SQS.Model;
using DotNetCloud.SqsToolbox.Abstractions;

namespace DotNetCloud.SqsToolbox
{
    public class SqsPollingDelayer : ISqsPollingDelayer
    {
        private readonly SqsPollingQueueReaderOptions _queueReaderOptions;

        private int _emptyResponseCounter;

        public SqsPollingDelayer(SqsPollingQueueReaderOptions queueReaderOptions)
        {
            _queueReaderOptions = queueReaderOptions;
        }

        public TimeSpan CalculateSecondsToDelay(IEnumerable<Message> messages)
        {
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
                delaySeconds = Math.Pow(delaySeconds, _emptyResponseCounter);
            }
            
            return TimeSpan.FromSeconds(delaySeconds);
        }
    }
}
