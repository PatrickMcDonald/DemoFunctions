var builder = DistributedApplication.CreateBuilder(args);

builder.AddAzureFunctionsProject<Projects.DemoFunctions>("demofunctions");

builder.Build().Run();
