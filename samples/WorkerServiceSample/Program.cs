using System;
using DotNetCloud.SqsToolbox.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace WorkerServiceSample
{
    public class Program
    {
        public static void Main(string[] args) => CreateHostBuilder(args).Build().Run();

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton<ReceiveQueueChannel>();

                    services.AddPollingSqsFromConfiguration(hostContext.Configuration)
                        .Configure(opt => opt.DelayWhenOverLimit = TimeSpan.FromMinutes(20))
                        .WithChannelSource<CustomSqsQueueReaderChannelSource>()
                        .WithBackgroundService()
                        .WithMessageProcessor<Worker>()
                        .WithDefaultExceptionHandler();

                    services.AddSqsBatchDeletion(hostContext.Configuration)
                        .Configure(opt =>
                        {
                            opt.DrainOnStop = true;
                            opt.MaxWaitForFullBatch = TimeSpan.FromSeconds(10);
                        })
                        .WithBackgroundService();

                    services.AddSqsToolboxDiagnosticsMonitoring<DiagnosticsMonitorService>();
                });
    }
}
