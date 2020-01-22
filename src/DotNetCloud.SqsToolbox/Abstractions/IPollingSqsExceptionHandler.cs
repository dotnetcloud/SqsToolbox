using System;

namespace DotNetCloud.SqsToolbox.Abstractions
{
    public interface IPollingSqsExceptionHandler
    {
        void OnException<T>(T exception) where T : Exception;
    }
}
