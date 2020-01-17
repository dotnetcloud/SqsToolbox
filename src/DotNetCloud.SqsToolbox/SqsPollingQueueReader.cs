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
using DotNetCloud.SqsToolbox.Diagnostics;

namespace DotNetCloud.SqsToolbox
{
    public class SqsPollingQueueReader : ISqsPollingQueueReader, IDisposable
    {
        public const string DiagnosticListenerName = "DotNetCloud.SqsToolbox.SqsSqsPollingQueueReader";

        private readonly SqsPollingQueueReaderOptions _queueReaderOptions;
        private readonly IAmazonSQS _amazonSqs;
        private readonly ISqsPollingDelayer _pollingDelayer;
        private readonly Channel<Message> _channel;
        private readonly ReceiveMessageRequest _receiveMessageRequest;

        private CancellationTokenSource _cancellationTokenSource;
        private Task _pollingTask;
        private bool _disposed;
        private bool _isStarted;
        
        private static readonly DiagnosticListener _diagnostics = new DiagnosticListener(DiagnosticListenerName);

        public SqsPollingQueueReader(SqsPollingQueueReaderOptions queueReaderOptions, IAmazonSQS amazonSqs, ISqsPollingDelayer pollingDelayer)
        {
            _queueReaderOptions = queueReaderOptions ?? throw new ArgumentNullException(nameof(queueReaderOptions));
            _amazonSqs = amazonSqs ?? throw new ArgumentNullException(nameof(amazonSqs));
            _pollingDelayer = pollingDelayer;

            _channel = Channel.CreateBounded<Message>(new BoundedChannelOptions(queueReaderOptions.ChannelCapacity)
            {
                SingleWriter = true
            });

            if (queueReaderOptions.ReceiveMessageRequest is object)
            {
                _receiveMessageRequest = queueReaderOptions.ReceiveMessageRequest;
            }
            else
            {
                _receiveMessageRequest = new ReceiveMessageRequest
                {
                    QueueUrl = queueReaderOptions.QueueUrl ?? throw new ArgumentNullException(nameof(queueReaderOptions), "A queue URL is required for the polling queue reader to be created"),
                    MaxNumberOfMessages = queueReaderOptions.MaxMessages,
                    WaitTimeSeconds = queueReaderOptions.PollTimeInSeconds
                };
            }
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
                    var activity = StartActivity();

                    ReceiveMessageResponse response = null;

                    try
                    {
                        DiagnosticsStart();

                        response = await _amazonSqs.ReceiveMessageAsync(_receiveMessageRequest, _cancellationTokenSource.Token);

                        DiagnosticsEnd(response);
                    }
                    catch (OverLimitException ex) // May be the case if the maximum number of in-flight messages is reached
                    {
                        DiagnosticsOverLimit(ex, activity);

                        await Task.Delay(_queueReaderOptions.DelayWhenOverLimit);

                        continue;
                    }
                    catch (AmazonSQSException ex)
                    {
                        DiagnosticsSqsException(ex, activity);

                        break;
                    }
                    catch (Exception ex)
                    {
                        DiagnosticsException(ex, activity);

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

                    var delayTimeSpan = _pollingDelayer.CalculateSecondsToDelay(response.Messages);

                    await Task.Delay(delayTimeSpan);
                }
            }
            finally
            {
                writer.TryComplete();
            }
        }

        private void DiagnosticsException(Exception ex, Activity activity)
        {
            if (_diagnostics.IsEnabled(Diagnostics.Diagnostics.Exception))
                _diagnostics.Write(Diagnostics.Diagnostics.Exception, new ExceptionData(ex, _receiveMessageRequest));

            activity?.AddTag("error", "true");
        }

        private void DiagnosticsSqsException(AmazonSQSException ex, Activity activity)
        {
            if (_diagnostics.IsEnabled(Diagnostics.Diagnostics.AmazonSqsException))
                _diagnostics.Write(Diagnostics.Diagnostics.AmazonSqsException, new ExceptionData(ex, _receiveMessageRequest));

            activity?.AddTag("error", "true");
        }

        private void DiagnosticsOverLimit(OverLimitException ex, Activity activity)
        {
            if (_diagnostics.IsEnabled(Diagnostics.Diagnostics.OverLimitException))
                _diagnostics.Write(Diagnostics.Diagnostics.OverLimitException, new ExceptionData(ex, _receiveMessageRequest));

            activity?.AddTag("error", "true");
        }

        private Activity StartActivity()
        {
            Activity activity = null;

            if (_diagnostics.IsEnabled() && _diagnostics.IsEnabled(Diagnostics.Diagnostics.PollingForMessages))
            {
                activity = new Activity(Diagnostics.Diagnostics.PollingForMessages);

                if (_diagnostics.IsEnabled(Diagnostics.Diagnostics.PollingForMessagesStart))
                {
                    _diagnostics.StartActivity(activity, new {_receiveMessageRequest});
                }
                else
                {
                    activity.Start();
                }
            }

            return activity;
        }

        private void DiagnosticsEnd(ReceiveMessageResponse response)
        {
            if (_diagnostics.IsEnabled(Diagnostics.Diagnostics.ReceiveMessagesRequestComplete))
                _diagnostics.Write(Diagnostics.Diagnostics.ReceiveMessagesRequestComplete,
                    new EndRequestPayload {QueueUrl = _queueReaderOptions.QueueUrl, MessageCount = response.Messages.Count});
        }

        private void DiagnosticsStart()
        {
            if (_diagnostics.IsEnabled(Diagnostics.Diagnostics.ReceiveMessagesBeginRequest))
                _diagnostics.Write(Diagnostics.Diagnostics.ReceiveMessagesBeginRequest,
                    new BeginRequestPayload(_queueReaderOptions.QueueUrl));
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
