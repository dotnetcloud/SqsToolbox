using System;
using System.Threading.Channels;
using Amazon.SQS.Model;
using DotNetCloud.SqsToolbox.Core.Receive;

namespace DotNetCloud.SqsToolbox
{
    public class SqsPollingQueueReaderFactoryOptions
    {
        public SqsPollingQueueReaderOptions Options { get; set; } = new SqsPollingQueueReaderOptions();

        public Channel<Message> Channel { get; set; }

        public Type ExceptionHandlerType { get; set; }
    }
}
