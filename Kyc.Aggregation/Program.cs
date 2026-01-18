using Kyc.Aggregation;
using Kyc.Aggregation.Application;
using Kyc.Aggregation.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApi()
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseApiPipeline();

app.Run();
