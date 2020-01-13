using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Amazon;
using Amazon.Runtime.CredentialManagement;
using Amazon.SQS;
using Amazon.SQS.Model;
using DotNetCloud.SqsToolbox;

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
            Console.WriteLine($"Event: {value.Key} ActivityName: {Activity.Current.OperationName} Id: {Activity.Current.Id} Payload: {value.Value}");
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
            var observer = new ExampleDiagnosticObserver();

            DiagnosticListener.AllListeners.Subscribe(observer);

            var f = new SharedCredentialsFile(SharedCredentialsFile.DefaultFilePath);

            f.TryGetProfile("default", out var profile);

            var credentials = profile.GetAWSCredentials(null);
            
            var client = new AmazonSQSClient(credentials, RegionEndpoint.EUWest1);

            var pollingReader = new SqsPollingQueueReader(new SqsPollingQueueReaderOptions{ QueueUrl = "https://sqs.eu-west-1.amazonaws.com/123456789012/test-queue" }, client);

            var readingTask = ReadFromChannel(pollingReader.ChannelReader);

            var cts = new CancellationTokenSource(30000);

            pollingReader.Start(cts.Token);

            await readingTask;
        }

        private static async Task ReadFromChannel(ChannelReader<Message> reader)
        {
            await foreach (var message in reader.ReadAllAsync())
            {
                Console.WriteLine(message.Body);
            }
        }
    }
}
