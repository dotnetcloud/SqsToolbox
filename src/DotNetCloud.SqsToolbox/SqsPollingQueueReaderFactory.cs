using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Channels;
using Amazon.SQS;
using Amazon.SQS.Model;
using DotNetCloud.SqsToolbox.Core;
using DotNetCloud.SqsToolbox.Core.Abstractions;
using DotNetCloud.SqsToolbox.Core.Receive;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DotNetCloud.SqsToolbox
{
    internal sealed class SqsPollingQueueReaderFactory : ISqsPollingQueueReaderFactory, ISqsMessageChannelFactory
    {
        private readonly ILogger _logger;
        private readonly IServiceProvider _services;
        private readonly IOptionsMonitor<SqsPollingQueueReaderFactoryOptions> _optionsMonitor;
        private readonly IExceptionHandler _exceptionHandler;

        internal readonly ConcurrentDictionary<string, Lazy<SqsPollingQueueReader>> _pollingReaders;
        internal readonly ConcurrentDictionary<string, Lazy<Channel<Message>>> _channels;

        public SqsPollingQueueReaderFactory(
            IServiceProvider services,
            ILoggerFactory loggerFactory,
            IOptionsMonitor<SqsPollingQueueReaderFactoryOptions> optionsMonitor,
            IExceptionHandler exceptionHandler = null)
        {
            _ = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));

            _services = services ?? throw new ArgumentNullException(nameof(services));
            _optionsMonitor = optionsMonitor ?? throw new ArgumentNullException(nameof(optionsMonitor));
            _exceptionHandler = exceptionHandler;

            _logger = loggerFactory.CreateLogger<SqsPollingQueueReaderFactory>();

            // case-sensitive because named options is.
            _pollingReaders = new ConcurrentDictionary<string, Lazy<SqsPollingQueueReader>>(StringComparer.Ordinal);
            _channels = new ConcurrentDictionary<string, Lazy<Channel<Message>>>(StringComparer.Ordinal);
        }

        public ISqsPollingQueueReader GetOrCreateReader(string name)
        {
            _ = name ?? throw new ArgumentNullException(nameof(name));

            var channel = GetOrCreateChannel(name);

            var options = _optionsMonitor.Get(name);

            var sqs = _services.GetRequiredService<IAmazonSQS>();
            var delayCalculator = new SqsReceiveDelayCalculator(options.Options);

            var exceptionHandler =
                options.ExceptionHandlerType is object type ? _services.GetService((Type)type) as IExceptionHandler : null;

            exceptionHandler ??= _exceptionHandler ?? DefaultExceptionHandler.Instance;

            var queueReader = new Lazy<SqsPollingQueueReader>(() => new SqsPollingQueueReader(options.Options, sqs, delayCalculator, exceptionHandler, channel), LazyThreadSafetyMode.ExecutionAndPublication).Value;

            return queueReader;
        }

        public Channel<Message> GetOrCreateChannel(string name)
        {
            _ = name ?? throw new ArgumentNullException(nameof(name));

            var options = _optionsMonitor.Get(name);

            var channelEntry = new Lazy<Channel<Message>>(() => options.Channel ?? Channel.CreateBounded<Message>(new BoundedChannelOptions(options.Options.ChannelCapacity)), LazyThreadSafetyMode.ExecutionAndPublication);

            var channel = _channels.GetOrAdd(name, channelEntry).Value;

            return channel;
        }
    }
}
