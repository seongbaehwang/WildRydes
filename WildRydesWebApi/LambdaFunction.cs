using Amazon.Lambda.AspNetCoreServer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Formatting.Json;

namespace WildRydesWebApi
{
    public class LambdaFunction : APIGatewayProxyFunction<Startup>
    {
        protected override void Init(IWebHostBuilder builder)
        {
            builder
                .ConfigureLogging(logging => logging.ClearProviders())
                .UseSerilog((hostingContext, loggerConfiguration) =>
                {
                    loggerConfiguration
                        // Serilog.Settings.Configuration is required
                        .ReadFrom.Configuration(hostingContext.Configuration)
                        .MinimumLevel.Information()
                        .WriteTo.Console(new JsonFormatter(renderMessage: true));
                });
        }

    }
}