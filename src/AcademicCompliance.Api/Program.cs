using AcademicCompliance.Api.Extensions;
using AcademicCompliance.Application;
using AcademicCompliance.Infrastructure;
using AcademicCompliance.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddApplication();
builder.Services.AddCurrentUser();
builder.Services.AddJwtAuthentication(builder.Configuration);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

builder.Services.AddInfrastructure(connectionString);

var app = builder.Build();

app.UseGlobalExceptionHandling();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await app.Services.SeedIdentityAsync();

app.Run();
