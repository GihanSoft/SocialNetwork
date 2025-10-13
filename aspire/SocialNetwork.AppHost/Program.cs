var builder = DistributedApplication.CreateBuilder(args);

var msSQL = builder.AddSqlServer("ms-sql").WithImageTag("2022-latest")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent)
    ;

var db = msSQL.AddDatabase("default");

var ui = builder.AddBunApp("ui", "../../SocialNetwork/ClientApp", "dev")
    .WithHttpEndpoint(targetPort: 5999, env: "DEVELOP_PORT")
    ;
builder.AddProject<Projects.SocialNetwork>("socialnetwork")
    .WithReference(db)
    .WithReference(ui.GetEndpoint("http")).WaitFor(ui)
    ;

builder.Build().Run();
