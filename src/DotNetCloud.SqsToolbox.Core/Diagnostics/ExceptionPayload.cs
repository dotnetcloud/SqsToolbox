using System;
using Amazon.SQS.Model;

namespace DotNetCloud.SqsToolbox.Core.Diagnostics
{
    public sealed class ExceptionPayload
    {
        internal ExceptionPayload(Exception exception, ReceiveMessageRequest request)
        {
            Exception = exception;
            Request = request;
        }

        public Exception Exception { get; }
        public ReceiveMessageRequest Request { get; }

        public override string ToString() => $"{{ {nameof(Exception)} = {Exception}, {nameof(Request)} = {Request} }}";
    }
}
