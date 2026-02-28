var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.ApplicationAuth>("applicationauth");

builder.Build().Run();
