var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.WebApi>("webapi");

builder.AddProject<Projects.WebApiAuth>("webapiauth");

builder.Build().Run();
