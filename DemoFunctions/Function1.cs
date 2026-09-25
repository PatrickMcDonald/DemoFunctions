using System.Diagnostics;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace DemoFunctions;

public class Function1(ILogger<Function1> logger, ServiceBusClient serviceBusClient)
{
    private static readonly ActivitySource ActivitySource = new(typeof(Function1).Assembly.GetName().Name ?? "DemoFunctions");

    [Function("Function1")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
    {
        logger.LogInformation("C# HTTP trigger function processed a request.");

        using (var activity = ActivitySource.StartActivity("ServiceBus Message", ActivityKind.Internal))
        {
            var payload = new { id = Guid.NewGuid() };
            var messageBody = JsonSerializer.Serialize(payload);

            using (logger.BeginScope(new Dictionary<string, object> { ["processing.step"] = 1 }))
            {
                logger.LogInformation("Sending message to Service Bus topic with ID = {PayloadId}", payload.id);
            }

            await using var sender = serviceBusClient.CreateSender("az.topic");
            await sender.SendMessageAsync(new ServiceBusMessage(messageBody));

            using (logger.BeginScope(new Dictionary<string, object> { ["processing.step"] = 2 }))
            {
                logger.LogInformation("Message sent to Service Bus topic with ID = {PayloadId}", payload.id);
            }
        }

        return new OkObjectResult("Welcome to Azure Functions!");
    }
}
