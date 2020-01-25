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

namespace DotNetCloud.SqsToolbox.Receive
{
    public class SqsPollingQueueReader : ISqsPollingQueueReader, IDisposable
    {
        public const string DiagnosticListenerName = "DotNetCloud.SqsToolbox.SqsSqsPollingQueueReader";

        private readonly SqsPollingQueueReaderOptions _queueReaderOptions;
        private readonly IAmazonSQS _amazonSqs;
        private readonly ISqsPollingDelayer _pollingDelayer;
        private readonly Channel<Message> _channel;
        private readonly ReceiveMessageRequest _receiveMessageRequest;
        private readonly ISqsPollingExceptionHandler _sqsPollingExceptionHandler;

        private CancellationTokenSource _cancellationTokenSource;
        private Task _pollingTask;
        private bool _disposed;
        private bool _isStarted;

        private readonly object _startLock = new object();
        private static readonly DiagnosticListener _diagnostics = new DiagnosticListener(DiagnosticListenerName);

        public SqsPollingQueueReader(SqsPollingQueueReaderOptions queueReaderOptions, IAmazonSQS amazonSqs, ISqsPollingDelayer pollingDelayer, ISqsPollingExceptionHandler sqsPollingExceptionHandler)
        {
            _queueReaderOptions = queueReaderOptions ?? throw new ArgumentNullException(nameof(queueReaderOptions));
            _amazonSqs = amazonSqs ?? throw new ArgumentNullException(nameof(amazonSqs));
            _pollingDelayer = pollingDelayer;

            _channel = queueReaderOptions.Channel ?? Channel.CreateBounded<Message>(new BoundedChannelOptions(queueReaderOptions.ChannelCapacity)
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

            _sqsPollingExceptionHandler = sqsPollingExceptionHandler ?? DefaultSqsPollingExceptionHandler.Instance;
        }

        public ChannelReader<Message> ChannelReader => _channel.Reader;

        /// <inheritdoc />
        public void Start(CancellationToken cancellationToken = default)
        {
            if (_isStarted)
                throw new InvalidOperationException("The queue reader is already started.");

            lock (_startLock)
            {
                _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

                _pollingTask = Task.Run(PollForMessagesAsync, cancellationToken);

                _isStarted = true;
            }            
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
                while (!_cancellationTokenSource.IsCancellationRequested && await writer.WaitToWriteAsync(_cancellationTokenSource.Token).ConfigureAwait(false))
                {
                    var activity = StartActivity();

                    ReceiveMessageResponse response = null;

                    try
                    {
                        DiagnosticsStart();

                        response = await _amazonSqs.ReceiveMessageAsync(_receiveMessageRequest, _cancellationTokenSource.Token).ConfigureAwait(false);

                        DiagnosticsEnd(response);
                    }
                    catch (OverLimitException ex) // May be the case if the maximum number of in-flight messages is reached
                    {
                        DiagnosticsOverLimit(ex, activity);

                        await Task.Delay(_queueReaderOptions.DelayWhenOverLimit).ConfigureAwait(false);

                        continue;
                    }
                    catch (AmazonSQSException ex)
                    {
                        DiagnosticsSqsException(ex, activity);

                        _sqsPollingExceptionHandler.OnException(ex);

                        break;
                    }
                    catch (Exception ex)
                    {
                        DiagnosticsException(ex, activity);

                        _sqsPollingExceptionHandler.OnException(ex);

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

                    await PublishMessagesAsync(response.Messages).ConfigureAwait(false);

                    var delayTimeSpan = _pollingDelayer.CalculateSecondsToDelay(response.Messages);

                    await Task.Delay(delayTimeSpan).ConfigureAwait(false);
                }
            }
            finally
            {
                writer.TryComplete();
            }
        }

        private void DiagnosticsException(Exception ex, Activity activity)
        {
            if (_diagnostics.IsEnabled(DiagnosticEvents.Exception))
                _diagnostics.Write(DiagnosticEvents.Exception, new ExceptionPayload(ex, _receiveMessageRequest));

            activity?.AddTag("error", "true");
        }

        private void DiagnosticsSqsException(AmazonSQSException ex, Activity activity)
        {
            if (_diagnostics.IsEnabled(DiagnosticEvents.AmazonSqsException))
                _diagnostics.Write(DiagnosticEvents.AmazonSqsException, new ExceptionPayload(ex, _receiveMessageRequest));

            activity?.AddTag("error", "true");
        }

        private void DiagnosticsOverLimit(OverLimitException ex, Activity activity)
        {
            if (_diagnostics.IsEnabled(DiagnosticEvents.OverLimitException))
                _diagnostics.Write(DiagnosticEvents.OverLimitException, new ExceptionPayload(ex, _receiveMessageRequest));

            activity?.AddTag("error", "true");
        }

        private Activity StartActivity()
        {
            Activity activity = null;

            if (_diagnostics.IsEnabled() && _diagnostics.IsEnabled(DiagnosticEvents.PollingForMessages))
            {
                activity = new Activity(DiagnosticEvents.PollingForMessages);

                if (_diagnostics.IsEnabled(DiagnosticEvents.PollingForMessagesStart))
                {
                    _diagnostics.StartActivity(activity, new { _receiveMessageRequest });
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
            if (_diagnostics.IsEnabled(DiagnosticEvents.ReceiveMessagesRequestComplete))
                _diagnostics.Write(DiagnosticEvents.ReceiveMessagesRequestComplete,
                    new EndRequestPayload(_queueReaderOptions.QueueUrl, response.Messages.Count));
        }

        private void DiagnosticsStart()
        {
            if (_diagnostics.IsEnabled(DiagnosticEvents.ReceiveMessagesBeginRequest))
                _diagnostics.Write(DiagnosticEvents.ReceiveMessagesBeginRequest,
                    new BeginRequestPayload(_queueReaderOptions.QueueUrl));
        }

        private async Task PublishMessagesAsync(IReadOnlyList<Message> messages)
        {
            if (!messages.Any())
                return;

            var writer = _channel.Writer;

            var index = 0;

            while (index < messages.Count && await writer.WaitToWriteAsync(_cancellationTokenSource.Token).ConfigureAwait(false))
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
    }
}
