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

        private readonly SqsBatchDeleterOptions _sqsBatchDeleterOptions;
        private readonly IAmazonSQS _amazonSqs;
        private readonly Channel<Message> _channel;

        private CancellationTokenSource _cancellationTokenSource;

        private Task _batchingTask;
        private bool _disposed;
        private bool _isStarted;

        private readonly Dictionary<string, string> _currentBatch;
        private readonly DeleteMessageBatchRequest _deleteMessageBatchRequest;
        private readonly object _startLock = new object();

        private static readonly DiagnosticListener _diagnostics = new DiagnosticListener(DiagnosticListenerName);

        public SqsBatchDeleter(SqsBatchDeleterOptions sqsBatchDeleterOptions, IAmazonSQS amazonSqs)
        {
            _sqsBatchDeleterOptions = sqsBatchDeleterOptions;
            _amazonSqs = amazonSqs ?? throw new ArgumentNullException(nameof(amazonSqs));

            _channel = Channel.CreateBounded<Message>(new BoundedChannelOptions(_sqsBatchDeleterOptions.ChannelCapacity)
            {
                SingleReader = true
            });

            _currentBatch = new Dictionary<string, string>(sqsBatchDeleterOptions.BatchSize);

            _deleteMessageBatchRequest = new DeleteMessageBatchRequest
            {
                QueueUrl = sqsBatchDeleterOptions.QueueUrl
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
            if (!_isStarted) return;

            _channel.Writer.TryComplete(); // nothing more will be written

            if (!_sqsBatchDeleterOptions.DrainOnStop)
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
                while (i < messages.Count && _channel.Writer.TryWrite(messages[i])) i++;
            }
        }

        private async Task BatchAsync()
        {
            var cancellationToken = _cancellationTokenSource.Token;

            while (await _channel.Reader.WaitToReadAsync(cancellationToken).ConfigureAwait(false))
            {
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                linkedCts.CancelAfter(_sqsBatchDeleterOptions.MaxWaitForFullBatch);

                await CreateBatchAsync(linkedCts.Token).ConfigureAwait(false);
                
                _deleteMessageBatchRequest.Entries = _currentBatch.Select(m => new DeleteMessageBatchRequestEntry(m.Key, m.Value)).ToList();
                
                cancellationToken.ThrowIfCancellationRequested();

                var sqsDeleteBatchResponse = await _amazonSqs.DeleteMessageBatchAsync(_deleteMessageBatchRequest, cancellationToken).ConfigureAwait(false);

                if (sqsDeleteBatchResponse.HttpStatusCode == HttpStatusCode.OK)
                {
                    foreach (var entry in sqsDeleteBatchResponse.Successful)
                    {
                        // diagnostics
                        Console.WriteLine($"Deleted {entry.Id}");
                    }

                    foreach (var entry in sqsDeleteBatchResponse.Failed)
                    {
                        // diagnostics
                        // retry handler?
                    }
                }
            }

            Console.WriteLine("Exiting BatchAsync");
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
                    
                    if (exitBatchCreation) continue;

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
