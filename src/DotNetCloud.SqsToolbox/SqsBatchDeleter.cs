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
    public class SqsBatchDeleter : ISqsBatchDeleter, IDisposable
    {
        private readonly SqsBatchDeleterOptions _sqsBatchDeleterOptions;
        private readonly IAmazonSQS _amazonSqs;
        private readonly Channel<Message> _channel;

        private CancellationTokenSource _cancellationTokenSource;

        private Task _batchingTask;
        private bool _disposed;
        private bool _isStarted;

        private readonly Dictionary<string, string> _currentBatch;
        private readonly DeleteMessageBatchRequest _deleteMessageBatchRequest;

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

            _isStarted = true;
            
            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            _batchingTask = Task.Run(BatchAsync, cancellationToken);
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
            await _channel.Writer.WriteAsync(message, cancellationToken).ConfigureAwait(false);
        }

        public async Task AddMessagesAsync(IList<Message> messages, CancellationToken cancellationToken = default)
        {
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
                while (_currentBatch.Count < 10)
                {
                    await _channel.Reader.WaitToReadAsync(cancellationToken).ConfigureAwait(false);
                    
                    var exitBatchCreation = !_channel.Reader.TryRead(out var message) || cancellationToken.IsCancellationRequested;
                    
                    if (exitBatchCreation) continue;

                    _currentBatch[message.MessageId] = message.ReceiptHandle; // only add each message ID once, using latest receipt handle
                }
            }
            catch (OperationCanceledException)
            {
                // swallow
            }

            Console.WriteLine($"Created batch with {_currentBatch.Count} items, in {sw.ElapsedMilliseconds} milliseconds");
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
