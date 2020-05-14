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
        private readonly ILoggerFactory _loggerFactory;

#if NETCOREAPP3_1
        public StopApplicationExceptionHandler(IHostApplicationLifetime hostApplicationLifetime, ILoggerFactory loggerFactory)
        {
            _appLifetime = hostApplicationLifetime ?? throw new ArgumentNullException(nameof(hostApplicationLifetime));
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        }
#else
        public StopApplicationExceptionHandler(IApplicationLifetime applicationLifetime, ILoggerFactory loggerFactory)
        {
            _appLifetime = applicationLifetime ?? throw new ArgumentNullException(nameof(applicationLifetime));
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        }
#endif

        public void OnException<T1, T2>(T1 exception, T2 source) where T1 : Exception where T2 : class
        {
            _ = exception ?? throw new ArgumentNullException(nameof(exception));
            _ = source ?? throw new ArgumentNullException(nameof(source));

            var logger = _loggerFactory.CreateLogger<T2>();

            if (exception is AmazonSQSException)
            {
                logger.LogError(exception, "Stopping application. An amazon SQS exception was thrown: {ExceptionMessage}", exception.Message);
            }

            _appLifetime.StopApplication();
        }
    }
}
