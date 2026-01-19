using Kyc.Aggregation.Api;
using Kyc.Aggregation.Api.Extensions;
using Kyc.Aggregation.Application;
using Kyc.Aggregation.Infrastructure;
using Kyc.Aggregation.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Register services from each layer
builder.Services
    .AddApi()
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Initialize database on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<KycDbContext>();
    dbContext.Database.Migrate();
}

// Configure pipeline
app.UseApiPipeline();

app.Run();
