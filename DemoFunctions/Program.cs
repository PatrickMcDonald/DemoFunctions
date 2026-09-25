using Azure.Monitor.OpenTelemetry.Exporter;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Azure.Functions.Worker.OpenTelemetry;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

AppContext.SetSwitch("Azure.Experimental.EnableActivitySource", true);

var builder = FunctionsApplication.CreateBuilder(args);

builder.Logging.SetMinimumLevel(LogLevel.Trace);

builder.AddServiceDefaults();

builder.ConfigureFunctionsWebApplication();

builder.Services.AddSingleton(_ => CreateServiceBusClient(builder.Configuration));

builder.Services.AddOpenTelemetry()
    .UseFunctionsWorkerDefaults();

if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING")))
{
    builder.Services.AddOpenTelemetry()
        .UseAzureMonitorExporter();
}

await builder.Build().RunAsync();

static ServiceBusClient CreateServiceBusClient(IConfiguration configuration)
{
    var connectionString = configuration["ServiceBus:ConnectionString"];
    if (!string.IsNullOrWhiteSpace(connectionString))
    {
        return new ServiceBusClient(connectionString);
    }

    var serviceBusNamespace = configuration["ServiceBus:FullyQualifiedNamespace"];
    if (!string.IsNullOrWhiteSpace(serviceBusNamespace))
    {
        return new ServiceBusClient(serviceBusNamespace);
    }

    throw new InvalidOperationException("Service Bus is not configured. Set ServiceBus:ConnectionString (or ConnectionStrings:messaging), or set ServiceBus:FullyQualifiedNamespace.");
}