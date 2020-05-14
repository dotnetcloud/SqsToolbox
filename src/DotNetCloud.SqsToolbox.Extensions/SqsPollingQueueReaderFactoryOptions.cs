using System;
using System.Threading.Channels;
using Amazon.SQS.Model;
using DotNetCloud.SqsToolbox.Receive;

namespace DotNetCloud.SqsToolbox.Extensions
{
    public class SqsPollingQueueReaderFactoryOptions
    {
        public SqsPollingQueueReaderOptions Options { get; set; } = new SqsPollingQueueReaderOptions();

        public Channel<Message> Channel { get; set; }

        public Type ExceptionHandlerType { get; set; }
    }
}
