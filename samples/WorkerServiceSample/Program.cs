using System;
using DotNetCloud.SqsToolbox.Extensions.DependencyInjection;
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
                    services.AddPollingSqs(opt =>
                    {
                        opt.QueueUrl = "https://sqs.eu-west-2.amazonaws.com/123456789012/test-queue";
                        opt.ChannelCapacity = 150;
                    })
                    .WithBackgroundService()
                    .WithMessageProcessor<Worker>()
                    .WithExceptionHandler<CustomExceptionHandler>();

                    services.AddSqsBatchDeletion(opt =>
                    {
                        opt.QueueUrl = "https://sqs.eu-west-2.amazonaws.com/123456789012/test-queue";
                        opt.DrainOnStop = true;
                        opt.MaxWaitForFullBatch = TimeSpan.FromSeconds(10);
                    })
                    .WithBackgroundService();

                    services.AddSqsToolboxDiagnosticsMonitoring<DiagnosticsMonitorService>();
                });
    }
}
