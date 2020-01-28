using System;
using DotNetCloud.SqsToolbox.Abstractions;

namespace DotNetCloud.SqsToolbox
{
    public sealed class DefaultExceptionHandler : IExceptionHandler
    {
        public static readonly DefaultExceptionHandler Instance = new DefaultExceptionHandler();

        public void OnException<T>(T exception) where T : Exception { }
    }
}
