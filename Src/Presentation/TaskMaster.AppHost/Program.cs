using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache");

var username = builder.AddParameter("username", secret: true);
var password = builder.AddParameter("password", secret: true);

var rabbitmq = builder.AddRabbitMQ("messaging", username, password)
                                                      .WithManagementPlugin();

var webapi = builder.AddProject<WebApi>("webapi")
       .WithReference(cache)
       .WaitFor(cache)
       .WithReference(rabbitmq)
       .WaitFor(rabbitmq);

var webapiauth = builder.AddProject<WebApiAuth>("webapiauth");

//builder.AddProject<ServiceLayer>("servicelayer");

builder.AddProject<WebsiteApp>("websiteapp")
       .WithReference(webapiauth)
       .WaitFor(webapiauth)
       .WithReference(webapi)
       .WaitFor(webapi);


builder.Build().Run();
