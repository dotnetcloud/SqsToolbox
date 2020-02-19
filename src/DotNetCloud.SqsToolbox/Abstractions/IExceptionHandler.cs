using System;

namespace DotNetCloud.SqsToolbox.Abstractions
{
    /// <summary>
    /// Handler for exceptions which occur within critical paths.
    /// </summary>
    public interface IExceptionHandler
    {
        /// <summary>
        /// Handler for an exception.
        /// </summary>
        /// <typeparam name="T1">The type of the <see cref="Exception"/></typeparam>
        /// <typeparam name="T2">The type of the source for the exception.</typeparam>
        /// <param name="exception">The exception being handled.</param>
        /// <param name="source">The source of the exception.</param>
        void OnException<T1, T2>(T1 exception, T2 source)
            where T1 : Exception
            where T2 : class;
    }
}
