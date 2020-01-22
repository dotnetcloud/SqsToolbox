using System;
using Amazon.SQS;
using DotNetCloud.SqsToolbox.Abstractions;
using Microsoft.Extensions.Hosting;

namespace WorkerServiceSample
{
    internal class CustomExceptionHandler : IPollingSqsExceptionHandler
    {
        private readonly IHostApplicationLifetime _hostApplicationLifetime;

        public CustomExceptionHandler(IHostApplicationLifetime hostApplicationLifetime) => _hostApplicationLifetime = hostApplicationLifetime;

        public void OnSqsException(AmazonSQSException sqsException) => _hostApplicationLifetime.StopApplication();

        public void OnException(Exception sqsException) => _hostApplicationLifetime.StopApplication();
    }
}
