using System;
using DotNetCloud.SqsToolbox.Core.Abstractions;

namespace WorkerServiceSample
{
    public class CustomExceptionHandler : IExceptionHandler
    {
        public void OnException<T1, T2>(T1 exception, T2 source) where T1 : Exception where T2 : class
        {
            Console.WriteLine("An exception occurred!!!!!!!!");
        }
    }
}
