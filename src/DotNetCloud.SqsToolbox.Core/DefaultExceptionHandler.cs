using System;
using DotNetCloud.SqsToolbox.Core.Abstractions;

namespace DotNetCloud.SqsToolbox.Core
{
    /// <summary>
    /// A default exception handler which no-ops for all exceptions.
    /// </summary>
    public sealed class DefaultExceptionHandler : IExceptionHandler
    {
        /// <summary>
        /// A reusable instance of a shared <see cref="DefaultExceptionHandler"/>.
        /// </summary>
        public static readonly DefaultExceptionHandler Instance = new DefaultExceptionHandler();

        /// <inheritdoc />
        public void OnException<T1, T2>(T1 exception, T2 source) where T1 : Exception where T2 : class
        {
            // No-op
        }
    }
}
