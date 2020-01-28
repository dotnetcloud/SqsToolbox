#if NETCOREAPP3_1

using System;
using Amazon.SQS;
using DotNetCloud.SqsToolbox.Abstractions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DotNetCloud.SqsToolbox.Extensions
{
    internal class StopApplicationExceptionHandler : IExceptionHandler
    {
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly ILogger<StopApplicationExceptionHandler> _logger;

        public StopApplicationExceptionHandler(IHostApplicationLifetime hostApplicationLifetime, ILogger<StopApplicationExceptionHandler> logger)
        {
            _hostApplicationLifetime = hostApplicationLifetime;
            _logger = logger;
        }

        public void OnException<T>(T exception) where T : Exception
        {
            if (exception is AmazonSQSException)
            {
                _logger.LogError(exception, "An amazon SQS exception was thrown: {ExceptionMessage}. Stopping application.", exception.Message);
            }

            _hostApplicationLifetime.StopApplication();
        }
    }
}

#endif
