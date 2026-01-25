using Projects;
using Scalar.Aspire;

var builder = DistributedApplication.CreateBuilder(args);

var redis = builder.AddRedis("redis");

//var username = builder.AddParameter("username", secret: true);
//var password = builder.AddParameter("password", secret: true);

//var rabbitmq = builder.AddRabbitMQ("messaging", username, password)
//                                                      .WithManagementPlugin();

var papercut = builder.AddPapercutSmtp("papercut", 80, 25);



var webapi = builder.AddProject<WebApi>("webapi")
    .WithReference(redis)
    .WaitFor(redis)
    .WithReference(papercut)
    .WaitFor(papercut);
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


builder.AddProject<Projects.WebApiEmailService>("webapiemailservice");


builder.Build().Run();
