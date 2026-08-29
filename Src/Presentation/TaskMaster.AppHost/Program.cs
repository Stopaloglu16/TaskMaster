using Projects;
using Scalar.Aspire;

var builder = DistributedApplication.CreateBuilder(args);

var redis = builder.AddRedis("redis");

var postgres = builder.AddPostgres("postgres")
                      .WithDataVolume()
                      .WithPgAdmin();

var taskmasterdb = postgres.AddDatabase("taskmasterdb");

// WithDataVolume keeps queued messages across restarts; the management plugin (:15672 via the
// dashboard) is how you inspect queue depth.
var rabbitmq = builder.AddRabbitMQ("rabbitmq")
                      .WithDataVolume()
                      .WithManagementPlugin();

// The host port is pinned so the OIDC issuer URL stays stable across runs — a moving port
// invalidates already-issued token cookies. Admin user is "admin"; the generated password lives in
// the AppHost user-secrets store under Parameters:keycloak-password.
var keycloak = builder.AddKeycloak("keycloak", 8080)
                      .WithDataVolume();

var papercut = builder.AddPapercutSmtp("papercut", 80, 25);



// WebApiAuth owns the migrations + seeding, so it goes first and everything
// else that touches the database waits on it.
var webapiauth = builder.AddProject<WebApiAuth>("webapiauth")
    .WithReference(taskmasterdb)
    .WaitFor(taskmasterdb);

var webapi = builder.AddProject<WebApi>("webapi")
    .WithReference(taskmasterdb)
    .WaitFor(taskmasterdb)
    .WaitFor(webapiauth)
    .WithReference(redis)
    .WaitFor(redis)
    .WithReference(papercut)
    .WaitFor(papercut)
    .WithReference(rabbitmq)
    .WaitFor(rabbitmq)
    .WithReference(keycloak)
    .WaitFor(keycloak);

// Add Scalar API Reference
var scalar = builder.AddScalarApiReference();

// Register services with the API Reference
scalar.WithApiReference(webapi);

builder.AddProject<WebsiteApp>("websiteapp")
       .WithReference(webapiauth)
       .WaitFor(webapiauth)
       .WithReference(webapi)
       .WaitFor(webapi)
       .WithReference(keycloak)
       .WaitFor(keycloak);


// The FileJob saga host: outbox relay + the two consumer groups. No WithExplicitStart() any more —
// it used to mean nothing consumed the pipeline unless someone pressed Start in the dashboard.
builder.AddProject<WorkerServiceProcess>("workerserviceprocess")
       .WithReference(taskmasterdb)
       .WaitFor(taskmasterdb)
       .WithReference(rabbitmq)
       .WaitFor(rabbitmq)
       .WaitFor(webapiauth);


builder.AddProject<Projects.WebApiEmailService>("webapiemailservice");


builder.Build().Run();
