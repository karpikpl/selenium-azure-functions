using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace Tests
{
    [SetUpFixture]
    public static class LoggingSetup
    {
        private readonly static ITelemetryChannel _channel = new InMemoryChannel();
        private static IServiceProvider _provider;

        public static ILogger<T> GetLogger<T>()
        {
            if (_provider == null)
            {
                throw new InvalidOperationException("Logging setup did not run - ServiceProvider is null");
            }

            return _provider.GetRequiredService<ILogger<T>>();
        }

        [OneTimeSetUp]
        public static void Setup()
        {
            IServiceCollection services = new ServiceCollection();
            services.Configure<TelemetryConfiguration>(config => config.TelemetryChannel = _channel);
            services.AddLogging(builder =>
            {
                // Only Application Insights is registered as a logger provider
                builder.AddApplicationInsights(

                    configureTelemetryConfiguration: (conf) => conf.ConnectionString = System.Environment.GetEnvironmentVariable("AppInsightsConnectionString"),
                    configureApplicationInsightsLoggerOptions: (options) => { }
                );
                builder.AddConsole();
            });

            _provider = services.BuildServiceProvider();
            ILogger<Program> logger = _provider.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Logger is working...");
        }

        [OneTimeTearDown]
        public static async Task TearDown()
        {
            // Explicitly call Flush() followed by Delay, as required in console apps.
            // This ensures that even if the application terminates, telemetry is sent to the back end.
            _channel.Flush();
            await Task.Delay(TimeSpan.FromMilliseconds(1000));
        }

    }
}
