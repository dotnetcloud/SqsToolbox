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
                    
                    services.AddSqsToolboxDiagnosticsMonitoring<DiagnosticsMonitorService>();
                });

    }
}
