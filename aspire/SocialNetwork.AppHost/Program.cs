var builder = DistributedApplication.CreateBuilder(args);

var ui = builder.AddBunApp("ui", "../../SocialNetwork/ClientApp", "dev")
    .WithHttpEndpoint(targetPort: 5999, env: "DEVELOP_PORT")
    ;
builder.AddProject<Projects.SocialNetwork>("socialnetwork")
    .WithReference(ui.GetEndpoint("http")).WaitFor(ui)
    ;

builder.Build().Run();
