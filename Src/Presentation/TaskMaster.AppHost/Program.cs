using Projects;
using Scalar.Aspire;

var builder = DistributedApplication.CreateBuilder(args);

var redis = builder.AddRedis("redis");

// The image tag is pinned, not left to the Aspire integration's default: Aspire 13.5.3 moved that
// default to postgres:18.3, and Postgres 18 refuses to start on a data directory written by 17
// ("in 18+, these Docker images are configured to store database data in a format which is
// compatible with pg_ctlcluster"). The container then exits(1), taskmasterdb never resolves a
// connection string, and every project hangs in WaitFor(taskmasterdb) — with the only clue being a
// "Duplicate property 'resource.connectionString'" line from the dashboard. Moving to 18 means
// either a pg_upgrade or dropping the data volume; do that deliberately, not via an Aspire bump.
var postgres = builder.AddPostgres("postgres")
                      .WithImageTag("17")
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
// WithRealmImport is a no-op once the realm exists in the data volume: Keycloak only imports a
// realm it does not already have. Editing taskmaster-realm.json therefore needs the volume wiped
// (see the run-app skill) or you will spend an afternoon debugging a realm that never changed.
var keycloak = builder.AddKeycloak("keycloak", 8080)
                      .WithDataVolume()
                      .WithRealmImport("./Keycloak");

// Must match "secret" on the taskmaster-auth client in taskmaster-realm.json. Dev-only value; in a
// real environment set it in user-secrets under Parameters:keycloak-client-secret and rotate it in
// the realm.
var keycloakClientSecret = builder.AddParameter("keycloak-client-secret", "taskmaster-dev-client-secret", secret: true);

// The issuer baked into a token is whatever URL Keycloak was reached at. The host port is pinned
// above precisely so this stays constant, so every service — issuer and validator alike — uses this
// literal rather than the service-discovery hostname, which would produce a mismatched `iss`.
const string keycloakRealmAuthority = "http://localhost:8080/realms/taskmaster";

var papercut = builder.AddPapercutSmtp("papercut", 80, 25);



// WebApiAuth owns the migrations + seeding, so it goes first and everything
// else that touches the database waits on it.
var webapiauth = builder.AddProject<WebApiAuth>("webapiauth")
    .WithReference(taskmasterdb)
    .WaitFor(taskmasterdb)
    .WithReference(keycloak)
    .WaitFor(keycloak)
    .WithEnvironment("Keycloak__Authority", keycloakRealmAuthority)
    .WithEnvironment("Keycloak__Audience", "taskmaster-api")
    .WithEnvironment("Keycloak__Realm", "taskmaster")
    .WithEnvironment("Keycloak__BaseUrl", "http://localhost:8080")
    .WithEnvironment("Keycloak__ClientId", "taskmaster-auth")
    .WithEnvironment("Keycloak__ClientSecret", keycloakClientSecret);

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
    .WaitFor(keycloak)
    .WithEnvironment("Keycloak__Authority", keycloakRealmAuthority)
    .WithEnvironment("Keycloak__Audience", "taskmaster-api");

// Add Scalar API Reference
var scalar = builder.AddScalarApiReference();

// Register services with the API Reference
scalar.WithApiReference(webapi);

var websiteapp = builder.AddProject<WebsiteApp>("websiteapp")
       .WithReference(webapiauth)
       .WaitFor(webapiauth)
       .WithReference(webapi)
       .WaitFor(webapi)
       .WithReference(keycloak)
       .WaitFor(keycloak)
       .WithEnvironment("Keycloak__Authority", keycloakRealmAuthority)
       .WithEnvironment("Keycloak__Audience", "taskmaster-api");

// Applied after websiteapp exists, because webapiauth is declared first and the reference points the
// other way. WebApiAuth's IEmailSender factory throws at controller-construction time if this is
// missing, which took out every UsersController endpoint — the value had never been set in
// appsettings, user-secrets or here.
webapiauth.WithEnvironment("AppSettings__WebSiteUrl", websiteapp.GetEndpoint("https"));


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
