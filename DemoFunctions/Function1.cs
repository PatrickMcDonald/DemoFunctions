using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace DemoFunctions;

public class Function1(ILogger<Function1> logger, ServiceBusClient serviceBusClient)
{
    [Function("Function1")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
    {
        logger.LogInformation("C# HTTP trigger function processed a request.");

        var payload = new { id = Guid.NewGuid() };
        var messageBody = JsonSerializer.Serialize(payload);

        await using var sender = serviceBusClient.CreateSender("az.topic");
        await sender.SendMessageAsync(new ServiceBusMessage(messageBody));

        return new OkObjectResult("Welcome to Azure Functions!");
    }
}
