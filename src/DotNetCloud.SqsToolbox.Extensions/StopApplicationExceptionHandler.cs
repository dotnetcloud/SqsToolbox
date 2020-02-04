using System;
using Amazon.SQS;
using DotNetCloud.SqsToolbox.Abstractions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DotNetCloud.SqsToolbox.Extensions
{
    internal class StopApplicationExceptionHandler : IExceptionHandler
    {
#if NETCOREAPP3_1
        private readonly IHostApplicationLifetime _appLifetime;
#else
        private readonly IApplicationLifetime _appLifetime;
#endif
        private readonly ILogger<StopApplicationExceptionHandler> _logger;

#if NETCOREAPP3_1
        public StopApplicationExceptionHandler(IHostApplicationLifetime hostApplicationLifetime, ILogger<StopApplicationExceptionHandler> logger)
        {
            _appLifetime = hostApplicationLifetime ?? throw new ArgumentNullException(nameof(hostApplicationLifetime));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
#else
         public StopApplicationExceptionHandler(IApplicationLifetime applicationLifetime, ILogger<StopApplicationExceptionHandler> logger)
        {
            _appLifetime = applicationLifetime ?? throw new ArgumentNullException(nameof(applicationLifetime));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
#endif

        public void OnException<T>(T exception) where T : Exception
        {
            _ = exception ?? throw new ArgumentNullException(nameof(exception));

            if (exception is AmazonSQSException)
            {
                _logger.LogError(exception, "An amazon SQS exception was thrown: {ExceptionMessage}. Stopping application.", exception.Message);
            }

            _appLifetime.StopApplication();
        }
    }
}
