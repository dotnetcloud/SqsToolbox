using Amazon.SQS;
using DotNetCloud.SqsToolbox.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace WorkerServiceSample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddAWSService<IAmazonSQS>();
                    services.AddPollingSqs(opt => opt.QueueUrl = "https://sqs.eu-west-1.amazonaws.com/123456789012/sq-test-queue-reader");
                    services.AddHostedService<Worker>();
                });
    }
}
