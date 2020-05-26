using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SQS.Model;
using DotNetCloud.SqsToolbox.Core.Diagnostics;
using DotNetCloud.SqsToolbox.Core.Receive;
using Microsoft.Extensions.Hosting;

namespace DotNetCloud.SqsToolbox.Diagnostics
{
    public abstract class DiagnosticsMonitoringService : IHostedService
    {
        private IDisposable _allListenersSubscription;
        private readonly ConcurrentBag<IDisposable> _subscriptions = new ConcurrentBag<IDisposable>();

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _allListenersSubscription = DiagnosticListener.AllListeners.Do(source =>
            {
                if (source.Name == SqsPollingQueueReader.DiagnosticListenerName)
                {
                    _subscriptions.Add(source.Do(pair =>
                    {
                        switch (pair.Key)
                        {
                            case DiagnosticEvents.ReceiveMessagesBeginRequest:
                            {
                                if (pair.Value is BeginReceiveRequestPayload payload)
                                {
                                    OnBegin(payload.QueueUrl);
                                }

                                break;
                            }
                            case DiagnosticEvents.ReceiveMessagesRequestComplete:
                            {
                                if (pair.Value is EndReceiveRequestPayload payload)
                                {
                                    OnReceived(payload.QueueUrl, payload.MessageCount);
                                }

                                break;
                            }
                            //case DiagnosticEvents.DeletionBatchCreated:
                            //{
                            //    if (pair.Value is DeletionBatchCreatedPayload payload)
                            //    {
                            //        OnDeleteBatchCreated(payload.MessageCount, payload.MillisecondsTaken);
                            //    }

                            //    break;
                            //}
                            //case DiagnosticEvents.DeleteBatchRequestComplete:
                            //{
                            //    if (pair.Value is EndDeletionBatchPayload payload)
                            //    {
                            //        OnBatchDeleted(payload.DeleteMessageBatchResponse, payload.MillisecondsTaken);
                            //    }

                            //    break;
                            //}
                            case DiagnosticEvents.OverLimitException:
                            {
                                if (pair.Value is ExceptionPayload payload)
                                {
                                    OnOverLimit(payload.Exception, payload.Request);
                                }

                                break;
                            }
                            case DiagnosticEvents.AmazonSqsException:
                            {
                                if (pair.Value is ExceptionPayload payload)
                                {
                                    OnSqsException(payload.Exception, payload.Request);
                                }

                                break;
                            }
                            case DiagnosticEvents.Exception:
                            {
                                if (pair.Value is ExceptionPayload payload)
                                {
                                    OnException(payload.Exception, payload.Request);
                                }

                                break;
                            }
                        }
                    })
                    .Subscribe());
                }

                //if (source.Name == SqsBatchDeleter.DiagnosticListenerName)
                //{
                //    _subscriptions.Add(source.Do(pair =>
                //    {
                //        switch (pair.Key)
                //        {
                //          case DiagnosticEvents.DeletionBatchCreated:
                //                {
                //                    if (pair.Value is DeletionBatchCreatedPayload payload)
                //                    {
                //                        OnDeleteBatchCreated(payload.MessageCount, payload.MillisecondsTaken);
                //                    }

                //                    break;
                //                }
                //            case DiagnosticEvents.DeleteBatchRequestComplete:
                //                {
                //                    if (pair.Value is EndDeletionBatchPayload payload)
                //                    {
                //                        OnBatchDeleted(payload.DeleteMessageBatchResponse, payload.MillisecondsTaken);
                //                    }

                //                    break;
                //                }
                //            case DiagnosticEvents.OverLimitException:
                //                {
                //                    if (pair.Value is ExceptionPayload payload)
                //                    {
                //                        OnOverLimit(payload.Exception, payload.Request);
                //                    }

                //                    break;
                //                }
                //            case DiagnosticEvents.AmazonSqsException:
                //                {
                //                    if (pair.Value is ExceptionPayload payload)
                //                    {
                //                        OnSqsException(payload.Exception, payload.Request);
                //                    }

                //                    break;
                //                }
                //            case DiagnosticEvents.Exception:
                //                {
                //                    if (pair.Value is ExceptionPayload payload)
                //                    {
                //                        OnException(payload.Exception, payload.Request);
                //                    }

                //                    break;
                //                }
                //        }
                //    })
                //    .Subscribe());
                //}

            }).Subscribe();

            return Task.CompletedTask;
        }
        
        public Task StopAsync(CancellationToken cancellationToken)
        {
            _allListenersSubscription?.Dispose();

            foreach (var subscription in _subscriptions)
            {
                subscription.Dispose();
            }

            return Task.CompletedTask;
        }

        public virtual void On(string queueUrl)
        {
        }

        public virtual void OnBegin(string queueUrl)
        {
        }

        public virtual void OnReceived(string queueUrl, in int messageCount)
        {
        }

        public virtual void OnOverLimit(Exception ex, ReceiveMessageRequest request)
        {
        }

        public virtual void OnSqsException(Exception ex, ReceiveMessageRequest request)
        {
        }

        public virtual void OnException(Exception ex, ReceiveMessageRequest request)
        {
        }

        public virtual void OnDeleteBatchCreated(in int messageCount, in long millisecondsTaken)
        {
        }

        public virtual void OnBatchDeleted(DeleteMessageBatchResponse deleteMessageBatchResponse, in long millisecondsTaken)
        {
        }
    }
}
