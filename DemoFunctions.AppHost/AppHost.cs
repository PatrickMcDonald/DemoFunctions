using SolidCode.Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var serviceBus = builder.AddAzureServiceBus("messaging")
    .RunAsEmulator(e => e
        .WithLifetime(ContainerLifetime.Persistent)
        .WithHostPort(61575)
        .WithSqlServer(sql => sql.WithDataVolume())
        .WithEndpoint("emulatorhealth", url =>
        {
            url.Port = 61576;
        }));

builder.AddServiceBusViewer("servicebusviewer")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithServiceBusEmulatorReference(serviceBus);

var topic = serviceBus.AddServiceBusTopic("topic", "az.topic");

var sub1 = topic.AddServiceBusSubscription("sub1", "az.sub1");
var sub2 = topic.AddServiceBusSubscription("sub2", "az.sub2");

builder.AddAzureFunctionsProject("demofunctions", @"..\DemoFunctions\DemoFunctions.csproj")
    .WaitFor(sub1)
    .WaitFor(sub2)
    .WithEnvironment("ServiceBus:ConnectionString", serviceBus);

await builder.Build().RunAsync();
