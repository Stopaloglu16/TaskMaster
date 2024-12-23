using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var webapi = builder.AddProject<WebApi>("webapi");

var webapiauth = builder.AddProject<WebApiAuth>("webapiauth");


builder.AddProject<WebApp>("webapp")
    .WithReference(webapiauth)
    .WithReference(webapi);


builder.Build().Run();
