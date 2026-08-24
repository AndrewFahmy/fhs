var builder = DistributedApplication.CreateBuilder(args);

var postgresCluster = builder
    .AddPostgres("postgres")
    .WithContainerName("fhs-db-server")
    .WithDataBindMount("../../temp/db/")
    .WithPgAdmin(config =>
    {
        config
            .WithBindMount("../../temp/pg-admin", "/var/lib/pgadmin")
            .WithContainerName("fhs-pg-admin");
    });

var database = postgresCluster.AddDatabase("fhs-db");

builder.AddProject<Projects.FHS_Api>("api").WithReference(database).WaitFor(database);

builder.Build().Run();
