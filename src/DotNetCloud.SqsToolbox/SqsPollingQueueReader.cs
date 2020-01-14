using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Amazon.SQS;
using Amazon.SQS.Model;
using DotNetCloud.SqsToolbox.Abstractions;

namespace DotNetCloud.SqsToolbox
{
    public class SqsSqsPollingQueueReader : ISqsPollingQueueReader, IDisposable
    {
        public const string DiagnosticListenerName = "DotNetCloud.SqsToolbox.SqsSqsPollingQueueReader";

        private readonly SqsPollingQueueReaderOptions _queueReaderOptions;
        private readonly IAmazonSQS _amazonSqs;
        private readonly Channel<Message> _channel;
        private readonly ReceiveMessageRequest _receiveMessageRequest;

        private CancellationTokenSource _cancellationTokenSource;
        private Task _pollingTask;
        private bool _disposed;
        private bool _isStarted;

        private int _emptyResponseCounter;

        private static readonly DiagnosticListener _diagnostics = new DiagnosticListener(DiagnosticListenerName);

        public SqsSqsPollingQueueReader(SqsPollingQueueReaderOptions queueReaderOptions, IAmazonSQS amazonSqs)
        {
            _queueReaderOptions = queueReaderOptions ?? throw new ArgumentNullException(nameof(queueReaderOptions));
            _amazonSqs = amazonSqs ?? throw new ArgumentNullException(nameof(amazonSqs));

            _channel = Channel.CreateBounded<Message>(new BoundedChannelOptions(queueReaderOptions.ChannelCapacity)
            {
                SingleWriter = true
            });

            _receiveMessageRequest = new ReceiveMessageRequest
            {
                QueueUrl = queueReaderOptions.QueueUrl ?? throw new ArgumentNullException(nameof(queueReaderOptions), "A queue URL is required for the polling queue reader to be created"),
                MaxNumberOfMessages = queueReaderOptions.MaxMessages,
                WaitTimeSeconds = queueReaderOptions.PollTimeInSeconds
            };
        }

        public ChannelReader<Message> ChannelReader => _channel.Reader;

        /// <inheritdoc />
        public void Start(CancellationToken cancellationToken = default)
        {
            if (_isStarted)
                throw new InvalidOperationException("The queue reader is already started.");

            _isStarted = true;

            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            _pollingTask = Task.Run(PollForMessagesAsync, cancellationToken);
        }

        /// <inheritdoc />
        public async Task StopAsync()
        {
            if (!_isStarted)
                return;

            _cancellationTokenSource?.Cancel();

            await _pollingTask;
        }

        private async Task PollForMessagesAsync()
        {
            var writer = _channel.Writer;

            try
            {
                while (!_cancellationTokenSource.IsCancellationRequested && await writer.WaitToWriteAsync(_cancellationTokenSource.Token))
                {
                    Activity activity = null;

                    if (_diagnostics.IsEnabled() && _diagnostics.IsEnabled(Diagnostics.PollingForMessages))
                    {
                        activity = new Activity(Diagnostics.PollingForMessages);

                        if (_diagnostics.IsEnabled(Diagnostics.PollingForMessagesStart))
                        {
                            _diagnostics.StartActivity(activity, new { _receiveMessageRequest });
                        }
                        else
                        {
                            activity.Start();
                        }
                    }

                    ReceiveMessageResponse response = null;

                    try
                    {
                        if (_diagnostics.IsEnabled(Diagnostics.ReceiveMessagesBeginRequest))
                            _diagnostics.Write(Diagnostics.ReceiveMessagesBeginRequest, new { _queueReaderOptions.QueueUrl });

                        response = await _amazonSqs.ReceiveMessageAsync(_receiveMessageRequest, _cancellationTokenSource.Token);

                        if (_diagnostics.IsEnabled(Diagnostics.ReceiveMessagesRequestComplete))
                            _diagnostics.Write(Diagnostics.ReceiveMessagesRequestComplete, new { _queueReaderOptions.QueueUrl, MessageCount = response.Messages.Count });
                    }
                    catch (OverLimitException ex) // May be the case if the maximum number of in-flight messages is reached
                    {
                        if (_diagnostics.IsEnabled(Diagnostics.OverLimitException))
                            _diagnostics.Write(Diagnostics.OverLimitException, new ExceptionData(ex, _receiveMessageRequest));

                        activity?.AddTag("error", "true");

                        await Task.Delay(_queueReaderOptions.DelayWhenOverLimit); // Wait for 1 minute before trying to receive from the queue
                    }
                    catch (AmazonSQSException ex)
                    {
                        if (_diagnostics.IsEnabled(Diagnostics.AmazonSqsException))
                            _diagnostics.Write(Diagnostics.AmazonSqsException, new ExceptionData(ex, _receiveMessageRequest));

                        activity?.AddTag("error", "true");

                        break;
                    }
                    catch (Exception ex)
                    {
                        if (_diagnostics.IsEnabled(Diagnostics.Exception))
                            _diagnostics.Write(Diagnostics.Exception, new ExceptionData(ex, _receiveMessageRequest));

                        activity?.AddTag("error", "true");

                        break;
                    }
                    finally
                    {
                        if (activity is object)
                        {
                            _diagnostics.StopActivity(activity, new { response });
                        }
                    }

                    if (response is null || response.HttpStatusCode != HttpStatusCode.OK)
                    {
                        continue; // Something went wrong
                    }

                    // Status code was 200-OK

                    await PublishMessagesAsync(response.Messages);

                    await DelayNextRequestIfRequired(response.Messages);
                }
            }
            finally
            {
                writer.TryComplete();
            }
        }

        private Task DelayNextRequestIfRequired(IEnumerable<Message> messages)
        {
            if (!messages.Any())
            {
                _emptyResponseCounter = 0;

                return Task.CompletedTask;
            }
            
            if (_emptyResponseCounter < 5)
            {
                _emptyResponseCounter++;
            }

            var delaySeconds = _queueReaderOptions.InitialDelayWhenEmpty.Seconds;

            if (_queueReaderOptions.UseExponentialBackoff)
            {
                delaySeconds *= (2 ^ _emptyResponseCounter);
            }

            return Task.Delay(TimeSpan.FromSeconds(delaySeconds));
        }

        private async Task PublishMessagesAsync(IReadOnlyList<Message> messages)
        {
            if (!messages.Any())
                return;

            var writer = _channel.Writer;

            var index = 0;

            while (index < messages.Count && await writer.WaitToWriteAsync(_cancellationTokenSource.Token))
            {
                while (index < messages.Count && writer.TryWrite(messages[index]))
                {
                    index++;
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _cancellationTokenSource.Dispose();
                    _pollingTask.Dispose();
                    _diagnostics.Dispose();
                }

                _disposed = true;
            }
        }

        private sealed class ExceptionData
        {
            internal ExceptionData(Exception exception, ReceiveMessageRequest request)
            {
                Exception = exception;
                Request = request;
            }

            private Exception Exception { get; }
            private ReceiveMessageRequest Request { get; }

            public override string ToString() => $"{{ {nameof(Exception)} = {Exception}, {nameof(Request)} = {Request} }}";
        }
    }
}
