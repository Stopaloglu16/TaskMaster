using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var webapi = builder.AddProject<WebApi>("webapi");

var webapiauth = builder.AddProject<WebApiAuth>("webapiauth");


builder.AddProject<WebsiteApp>("websiteapp")
       .WithReference(webapiauth)
       .WithReference(webapi);

builder.AddDockerComposePublisher();

builder.Build().Run();
