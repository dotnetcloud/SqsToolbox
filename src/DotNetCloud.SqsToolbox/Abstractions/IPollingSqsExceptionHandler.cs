using System;

namespace DotNetCloud.SqsToolbox.Abstractions
{
    public interface ISqsPollingExceptionHandler
    {
        void OnException<T>(T exception) where T : Exception;
    }
}
