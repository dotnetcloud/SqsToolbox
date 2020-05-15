using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace WorkerServiceSample
{
    public class Program
    {
        public static void Main() => CreateHostBuilder().Build().Run();

        public static IHostBuilder CreateHostBuilder() =>
            Host.CreateDefaultBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddPollingSqs(hostContext.Configuration.GetSection("TestQueue"))
                        .Configure(c => c.UseExponentialBackoff = true)
                        .WithBackgroundService()
                        .WithMessageProcessor<QueueProcessor>()
                        .WithDefaultExceptionHandler();

                    // the above can be simplified to:
                    services.AddDefaultPollingSqs<QueueProcessor>(hostContext.Configuration.GetSection("TestQueue2")); // This snippet does not call configure, but can do if required.

                    services.AddSqsToolboxDiagnosticsMonitoring<DiagnosticsMonitorService>();
                });
    }
}
