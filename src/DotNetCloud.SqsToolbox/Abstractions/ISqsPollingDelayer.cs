using System;
using System.Collections.Generic;
using Amazon.SQS.Model;

namespace DotNetCloud.SqsToolbox.Abstractions
{
    public interface ISqsPollingDelayer
    {
        TimeSpan CalculateSecondsToDelay(IEnumerable<Message> messages);
    }
}
