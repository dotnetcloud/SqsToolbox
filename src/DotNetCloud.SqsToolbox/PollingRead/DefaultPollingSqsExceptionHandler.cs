using System;
using Amazon.SQS;
using DotNetCloud.SqsToolbox.Abstractions;

namespace DotNetCloud.SqsToolbox.PollingRead
{
    public sealed class DefaultPollingSqsExceptionHandler : IPollingSqsExceptionHandler
    {
        public void OnSqsException(AmazonSQSException sqsException)
        {
        }

        public void OnException(Exception sqsException)
        {
        }
    }
}
