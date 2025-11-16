using Projects;
using Scalar.Aspire;

var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache");

//var username = builder.AddParameter("username", secret: true);
//var password = builder.AddParameter("password", secret: true);

//var rabbitmq = builder.AddRabbitMQ("messaging", username, password)
//                                                      .WithManagementPlugin();


var webapi = builder.AddProject<WebApi>("webapi")
    .WithReference(cache)
    .WaitFor(cache);
  //.WithReference(rabbitmq)
  //.WaitFor(rabbitmq);

// Add Scalar API Reference
var scalar = builder.AddScalarApiReference();

var webapiauth = builder.AddProject<WebApiAuth>("webapiauth");

// Register services with the API Reference
scalar.WithApiReference(webapi);

builder.AddProject<WebsiteApp>("websiteapp")
       .WithReference(webapiauth)
       .WaitFor(webapiauth)
       .WithReference(webapi)
       .WaitFor(webapi);


builder.AddProject<WorkerServiceProcess>("workerserviceprocess").WithExplicitStart();


builder.Build().Run();
