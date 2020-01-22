using System;
using Amazon.SQS;
using DotNetCloud.SqsToolbox.Abstractions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WorkerServiceSample
{
    internal class CustomExceptionHandler : IPollingSqsExceptionHandler
    {
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly ILogger<CustomExceptionHandler> _logger;

        public CustomExceptionHandler(IHostApplicationLifetime hostApplicationLifetime, ILogger<CustomExceptionHandler> logger)
        {
            _hostApplicationLifetime = hostApplicationLifetime;
            _logger = logger;
        }

        public void OnException<T>(T exception) where T : Exception
        {
            if (exception is AmazonSQSException)
            {
                _logger.LogError(exception, "An amazon SQS exception was thrown: {ExceptionMessage}.", exception.Message);
            }

            _hostApplicationLifetime.StopApplication();
        }
    }
}
