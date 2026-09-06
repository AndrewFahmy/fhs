var builder = DistributedApplication.CreateBuilder(args);

var postgresCluster = builder
    .AddPostgres("postgres")
    .WithContainerName("fhs-db-server")
    .WithDataBindMount("../../temp/db/")
    .WithPgAdmin(config =>
    {
        config.WithBindMount("../../temp/pg-admin", "/var/lib/pgadmin").WithContainerName("fhs-pg-admin");
    });

var database = postgresCluster.AddDatabase("fhs-db");

var keycloak = builder
    .AddKeycloak("keycloak", 18080)
    .WithDataBindMount("../../temp/keycloak")
    .WithRealmImport("../Keycloak/fhs-realm.json");

var api = builder
    .AddProject<Projects.FHS_Api>("api")
    .WithUrlForEndpoint("http", url => url.Url = "/scalar")
    .WithUrlForEndpoint("https", url => url.Url = "/scalar")
    .WithReference(database, connectionName: "DbConnection")
    .WaitFor(database)
    .WithEnvironment(
        "Authentication__Authority",
        ReferenceExpression.Create($"{keycloak.GetEndpoint("http")}/realms/fhs")
    )
    .WaitFor(keycloak);

builder
    .AddViteApp("web", "../../frontend")
    .WithBun()
    .WithEnvironment("VITE_API_URL", api.GetEndpoint("http"))
    .WaitFor(api);

builder.Build().Run();
