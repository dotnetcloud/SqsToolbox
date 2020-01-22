using System;
using Amazon.SQS;

namespace DotNetCloud.SqsToolbox.Abstractions
{
    public interface IPollingSqsExceptionHandler
    {
        void OnSqsException(AmazonSQSException sqsException);
        void OnException(Exception sqsException);
    }
}
