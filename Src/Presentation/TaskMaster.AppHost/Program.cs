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

// Add Scalar API Reference Header: Authorization Bearer "Token"
var scalar = builder.AddScalarApiReference(options =>
{
    options.WithTheme(ScalarTheme.BluePlanet);
    //options.AddDocument("asd");
});


var webapiauth = builder.AddProject<WebApiAuth>("webapiauth");

// Register services with the API Reference
scalar.WithApiReference(webapi)
      .WithApiReference(webapiauth);


var papercut = builder.AddPapercutSmtp("papercut");


builder.AddProject<WebsiteApp>("websiteapp")
       .WithReference(webapiauth)
       .WaitFor(webapiauth)
       .WithReference(webapi)
       .WaitFor(webapi)
       .WithReference(papercut)
       .WaitFor(papercut);


builder.AddProject<WorkerServiceProcess>("workerserviceprocess").WithExplicitStart();



builder.Build().Run();
