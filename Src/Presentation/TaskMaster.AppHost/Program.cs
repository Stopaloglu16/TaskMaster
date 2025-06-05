using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache");

var webapi = builder.AddProject<WebApi>("webapi");

var webapiauth = builder.AddProject<WebApiAuth>("webapiauth");


builder.AddProject<WebsiteApp>("websiteapp")
       .WithReference(cache)
       .WaitFor(cache)
       .WithReference(webapiauth)
       .WaitFor(webapiauth)
       .WithReference(webapi)
       .WaitFor(webapi);

//builder.AddDockerComposePublisher();

builder.Build().Run();
