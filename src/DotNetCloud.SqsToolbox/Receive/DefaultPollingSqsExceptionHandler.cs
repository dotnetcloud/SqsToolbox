using System;
using DotNetCloud.SqsToolbox.Abstractions;

namespace DotNetCloud.SqsToolbox.Receive
{
    public sealed class DefaultSqsPollingExceptionHandler : ISqsPollingExceptionHandler
    {
        public static readonly DefaultSqsPollingExceptionHandler Instance = new DefaultSqsPollingExceptionHandler();

        public void OnException<T>(T exception) where T : Exception { }
    }
}
