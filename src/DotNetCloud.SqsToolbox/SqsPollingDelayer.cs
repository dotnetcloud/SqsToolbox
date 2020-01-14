using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

        public async Task<int> Delay(IEnumerable<Message> messages, CancellationToken cancellationToken)
        {
            if (messages.Any())
            {
                _emptyResponseCounter = 0;
                return _emptyResponseCounter;
            }

            if (_emptyResponseCounter < 5)
            {
                _emptyResponseCounter++;
            }

            var delaySeconds = _queueReaderOptions.InitialDelayWhenEmpty.TotalSeconds;

            if (_queueReaderOptions.UseExponentialBackoff && _emptyResponseCounter > 1)
            {
                delaySeconds *= 2 ^ (_emptyResponseCounter - 1);
            }

            await Task.Delay(TimeSpan.FromSeconds(delaySeconds), cancellationToken);

            return _emptyResponseCounter;
        }
    }
}
