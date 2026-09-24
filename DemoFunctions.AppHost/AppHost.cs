var builder = DistributedApplication.CreateBuilder(args);

var serviceBus = builder.AddAzureServiceBus("messaging")
    .RunAsEmulator(e => e
        .WithHostPort(56721)
        .WithEndpoint("emulatorhealth", url =>
        {
            url.Port = 53001;
        }));

var topic = serviceBus.AddServiceBusTopic("topic", "az.topic");

var sub1 = topic.AddServiceBusSubscription("sub1", "az.sub1");
var sub2 = topic.AddServiceBusSubscription("sub2", "az.sub2");

builder.AddAzureFunctionsProject<Projects.DemoFunctions>("demofunctions")
    .WaitFor(sub1)
    .WaitFor(sub2)
    .WithEnvironment("ServiceBus:ConnectionString", serviceBus);

await builder.Build().RunAsync();
