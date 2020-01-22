using System;
using DotNetCloud.SqsToolbox.Abstractions;

namespace DotNetCloud.SqsToolbox.PollingRead
{
    public sealed class DefaultPollingSqsExceptionHandler : IPollingSqsExceptionHandler
    {
        public static readonly DefaultPollingSqsExceptionHandler Instance = new DefaultPollingSqsExceptionHandler();

        public void OnException<T>(T exception) where T : Exception { }
    }
}
