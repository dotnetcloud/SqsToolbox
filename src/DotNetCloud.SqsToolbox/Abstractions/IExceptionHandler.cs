using System;

namespace DotNetCloud.SqsToolbox.Abstractions
{
    public interface IExceptionHandler
    {
        void OnException<T>(T exception) where T : Exception;
    }
}
