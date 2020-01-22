using DotNetCloud.SqsToolbox.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DotNetCloud.SqsWorkerService
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
                    var queueUrl = hostContext.Configuration.GetSection("SQS")["ProcessingQueueUrl"];

                    services.AddPollingSqsBackgroundServiceWithProcessor<MessageProcessingService>(opt =>
                    {
                        opt.QueueUrl = queueUrl;
                    });

                    services.AddSqsToolboxDiagnosticsMonitoring<DiagnosticsMonitorService>();

                    services.AddSqsBatchDeletion(opt =>
                        {
                            opt.QueueUrl = queueUrl;
                        })
                        .WithBackgroundService();
                });
    }
}
