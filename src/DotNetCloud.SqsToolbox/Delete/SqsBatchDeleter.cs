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

namespace DotNetCloud.SqsToolbox.Delete
{
    public class SqsBatchDeleter : ISqsBatchDeleter, IDisposable
    {
        public const string DiagnosticListenerName = "DotNetCloud.SqsToolbox.SqsBatchDeleter";

        private readonly SqsBatchDeletionOptions _sqsBatchDeletionOptions;
        private readonly IAmazonSQS _amazonSqs;
        private readonly IFailedDeletionEntryHandler _failedDeletionEntryHandler;
        private readonly IExceptionHandler _exceptionHandler;
        private readonly Channel<Message> _channel;

        private CancellationTokenSource _cancellationTokenSource;

        private Task _batchingTask;
        private bool _disposed;
        private bool _isStarted;

        private readonly Dictionary<string, string> _currentBatch;
        private readonly DeleteMessageBatchRequest _deleteMessageBatchRequest;
        private readonly object _startLock = new object();

        private static readonly DiagnosticListener _diagnostics = new DiagnosticListener(DiagnosticListenerName);

        public SqsBatchDeleter(SqsBatchDeletionOptions sqsBatchDeletionOptions, IAmazonSQS amazonSqs, IExceptionHandler exceptionHandler, IFailedDeletionEntryHandler failedDeletionEntryHandler)
            : this(sqsBatchDeletionOptions, amazonSqs, exceptionHandler, failedDeletionEntryHandler, null)
        {
        }

        public SqsBatchDeleter(SqsBatchDeletionOptions sqsBatchDeletionOptions, IAmazonSQS amazonSqs, IExceptionHandler exceptionHandler, IFailedDeletionEntryHandler failedDeletionEntryHandler, Channel<Message> channel)
        {
            _ = sqsBatchDeletionOptions ?? throw new ArgumentNullException(nameof(sqsBatchDeletionOptions));

            _sqsBatchDeletionOptions = sqsBatchDeletionOptions.Clone();
            _amazonSqs = amazonSqs ?? throw new ArgumentNullException(nameof(amazonSqs));
            _failedDeletionEntryHandler = failedDeletionEntryHandler ?? DefaultFailedDeletionEntryHandler.Instance;
            _exceptionHandler = exceptionHandler ?? DefaultExceptionHandler.Instance;

            _channel = channel ?? Channel.CreateBounded<Message>(new BoundedChannelOptions(_sqsBatchDeletionOptions.ChannelCapacity)
            {
                SingleReader = true
            });

            _currentBatch = new Dictionary<string, string>(sqsBatchDeletionOptions.BatchSize);

            _deleteMessageBatchRequest = new DeleteMessageBatchRequest
            {
                QueueUrl = sqsBatchDeletionOptions.QueueUrl
            };
        }

        public void Start(CancellationToken cancellationToken = default)
        {
            if (_isStarted)
                throw new InvalidOperationException("The batch deleter is already started.");

            lock (_startLock)
            {
                if (_isStarted)
                    throw new InvalidOperationException("The batch deleter is already started.");

                _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

                _batchingTask = Task.Run(BatchAsync, cancellationToken);

                _isStarted = true;
            }
        }

        public async Task StopAsync()
        {
            if (!_isStarted)
                return;

            _channel.Writer.TryComplete(); // nothing more will be written

            if (!_sqsBatchDeletionOptions.DrainOnStop)
            {
                _cancellationTokenSource?.Cancel();
            }

            await _batchingTask.ConfigureAwait(false);
        }

        public async Task AddMessageAsync(Message message, CancellationToken cancellationToken = default)
        {
            _ = message ?? throw new ArgumentNullException(nameof(message));

            await _channel.Writer.WriteAsync(message, cancellationToken).ConfigureAwait(false);
        }

        public async Task AddMessagesAsync(IList<Message> messages, CancellationToken cancellationToken = default)
        {
            _ = messages ?? throw new ArgumentNullException(nameof(messages));

            var i = 0;

            while (i < messages.Count && await _channel.Writer.WaitToWriteAsync(cancellationToken).ConfigureAwait(false))
            {
                while (i < messages.Count && _channel.Writer.TryWrite(messages[i]))
                    i++;
            }
        }

        private async Task BatchAsync()
        {
            var cancellationToken = _cancellationTokenSource.Token;

            try
            {
                while (await _channel.Reader.WaitToReadAsync(cancellationToken).ConfigureAwait(false)) // wait until there are messages in the channel before we try to batch
                {
                    using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                    linkedCts.CancelAfter(_sqsBatchDeletionOptions.MaxWaitForFullBatch);

                    await CreateBatchAsync(linkedCts.Token).ConfigureAwait(false);

                    _deleteMessageBatchRequest.Entries = _currentBatch.Select(m => new DeleteMessageBatchRequestEntry(m.Key, m.Value)).ToList();

                    cancellationToken.ThrowIfCancellationRequested();

                    var sw = Stopwatch.StartNew();

                    var sqsDeleteBatchResponse = await _amazonSqs.DeleteMessageBatchAsync(_deleteMessageBatchRequest, cancellationToken).ConfigureAwait(false);

                    sw.Stop();

                    if (sqsDeleteBatchResponse.HttpStatusCode == HttpStatusCode.OK)
                    {
                        BatchRequestCompletedDiagnostics(sqsDeleteBatchResponse, sw);

                        var failureTasks = sqsDeleteBatchResponse.Failed.Select(entry =>
                            _failedDeletionEntryHandler.OnFailureAsync(entry, cancellationToken)).ToArray();

                        await Task.WhenAll(failureTasks);
                    }
                    else
                    {
                        // TODO - Handle non-success status code
                    }
                }
            }
            catch (Exception ex)
            {
                _exceptionHandler.OnException(ex, this);
            }
        }

        private async Task CreateBatchAsync(CancellationToken cancellationToken)
        {
            var sw = Stopwatch.StartNew();

            _currentBatch.Clear();

            try
            {
                while (_currentBatch.Count < 10 && await _channel.Reader.WaitToReadAsync(cancellationToken).ConfigureAwait(false))
                {
                    var exitBatchCreation = !_channel.Reader.TryRead(out var message) || cancellationToken.IsCancellationRequested;

                    if (exitBatchCreation)
                        continue;

                    _currentBatch[message.MessageId] = message.ReceiptHandle; // only add each message ID once, using latest receipt handle
                }
            }
            catch (OperationCanceledException)
            {
                // swallow this as expected when batch is not full within timeout period
            }

            sw.Stop();

            BatchCreatedDiagnostics(_currentBatch.Count, sw);
        }

        private static void BatchCreatedDiagnostics(int messageCount, Stopwatch stopwatch)
        {
            if (_diagnostics.IsEnabled(DiagnosticEvents.DeletionBatchCreated))
                _diagnostics.Write(DiagnosticEvents.DeletionBatchCreated,
                    new DeletionBatchCreatedPayload(messageCount, stopwatch.ElapsedMilliseconds));
        }

        private static void BatchRequestCompletedDiagnostics(DeleteMessageBatchResponse response, Stopwatch stopwatch)
        {
            if (_diagnostics.IsEnabled(DiagnosticEvents.DeleteBatchRequestComplete))
                _diagnostics.Write(DiagnosticEvents.DeleteBatchRequestComplete,
                    new EndDeletionBatchPayload(response, stopwatch.ElapsedMilliseconds));
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _cancellationTokenSource.Dispose();
                _batchingTask.Dispose();
            }

            _disposed = true;
        }
    }
}
