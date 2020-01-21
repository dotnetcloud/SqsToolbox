using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Amazon;
using Amazon.Runtime.CredentialManagement;
using Amazon.SQS;
using Amazon.SQS.Model;
using DotNetCloud.SqsToolbox;
using DotNetCloud.SqsToolbox.BatchDelete;
using DotNetCloud.SqsToolbox.PollingRead;

namespace ConsoleAppSample
{
    public sealed class ExampleDiagnosticObserver : IObserver<DiagnosticListener>, IObserver<KeyValuePair<string, object>>
    {
        private readonly List<IDisposable> _subscriptions = new List<IDisposable>();

        void IObserver<DiagnosticListener>.OnNext(DiagnosticListener diagnosticListener)
        {
            if (diagnosticListener.Name == SqsPollingQueueReader.DiagnosticListenerName)
            {
                var subscription = diagnosticListener.Subscribe(this);
                _subscriptions.Add(subscription);
            }
        }

        public void OnCompleted()
        {
            throw new NotImplementedException();
        }

        public void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public void OnNext(KeyValuePair<string, object> value)
        {
            var (key, payload) = value;

            Console.WriteLine($"Event: {key} ActivityName: {Activity.Current.OperationName} Id: {Activity.Current.Id} Payload: {payload}");
        }

        void IObserver<DiagnosticListener>.OnError(Exception error)
        {
        }

        void IObserver<DiagnosticListener>.OnCompleted()
        {
            _subscriptions.ForEach(x => x.Dispose());
            _subscriptions.Clear();
        }
    }

    internal class Program
    {
        private static async Task Main(string[] args)
        {
            //var observer = new ExampleDiagnosticObserver();

            //DiagnosticListener.AllListeners.Subscribe(observer);

            var f = new SharedCredentialsFile(SharedCredentialsFile.DefaultFilePath);

            f.TryGetProfile("default", out var profile);

            var credentials = profile.GetAWSCredentials(null);
            
            var client = new AmazonSQSClient(credentials, RegionEndpoint.EUWest2);

            var options = new SqsPollingQueueReaderOptions { QueueUrl = "https://sqs.eu-west-1.amazonaws.com/123456789012/test-queue" };

            var pollingReader = new SqsPollingQueueReader(options, client, new SqsPollingDelayer(options));

            var deleter = new SqsBatchDeleter(new SqsBatchDeleterOptions { MaxWaitForFullBatch = TimeSpan.FromSeconds(10), DrainOnStop = true, QueueUrl = "https://sqs.eu-west-1.amazonaws.com/123456789012/test-queue" }, client);

            var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));

            var readingTask = ReadFromChannelAsync(pollingReader.ChannelReader, deleter, cts.Token);

            deleter.Start(cts.Token);
            pollingReader.Start(cts.Token);

            await readingTask;
            
            //for (var i = 0; i < 26; i++)
            //{
            //    await deleter.AddMessageAsync(new Message{ MessageId = Guid.NewGuid().ToString() }, cts.Token);
            //}

            //deleter.Start(cts.Token);

            //await Task.Delay(TimeSpan.FromSeconds(8), cts.Token);

            //await deleter.AddMessageAsync(new Message { MessageId = Guid.NewGuid().ToString() }, cts.Token);
            
            //await Task.Delay(TimeSpan.FromSeconds(15), cts.Token);

            //for (var i = 0; i < 11; i++)
            //{
            //    await deleter.AddMessageAsync(new Message { MessageId = Guid.NewGuid().ToString() }, cts.Token);
            //}

            //var messages = Enumerable.Range(0, 57).Select(x => new Message {MessageId = Guid.NewGuid().ToString()}).ToArray();

            //await deleter.AddMessagesAsync(messages, cts.Token);

            //await Task.Delay(TimeSpan.FromSeconds(10), cts.Token);

            //for (var i = 0; i < 2; i++)
            //{
            //    await deleter.AddMessageAsync(new Message{ MessageId = "ABC" }, cts.Token);
            //}

            //await Task.Delay(TimeSpan.FromSeconds(10), cts.Token);
            
            await deleter.StopAsync();

            //await Task.Delay(Timeout.Infinite, cts.Token);
        }

        private static async Task ReadFromChannelAsync(ChannelReader<Message> reader, SqsBatchDeleter deleter, CancellationToken cancellationToken)
        {
            await foreach (var message in reader.ReadAllAsync(cancellationToken))
            {
                Console.WriteLine(message.MessageId);

                await deleter.AddMessageAsync(message, cancellationToken);
            }
        }
    }
}
