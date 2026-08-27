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

builder
    .AddProject<Projects.FHS_Api>("api")
    .WithReference(database, connectionName: "DbConnection")
    .WaitFor(database)
    .WithEnvironment(
        "Authentication__Authority",
        ReferenceExpression.Create($"{keycloak.GetEndpoint("http")}/realms/fhs")
    )
    .WaitFor(keycloak);

builder.Build().Run();
