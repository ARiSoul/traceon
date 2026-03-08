using Scalar.AspNetCore;
using Serilog;
using Traceon.Api.Endpoints;
using Traceon.Api.Services;
using Traceon.Application;
using Traceon.Application.Interfaces;
using Traceon.Infrastructure;
using Traceon.Infrastructure.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

builder.Services.AddOpenApi();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration.GetConnectionString("TraceonDb")
    ?? throw new InvalidOperationException("Connection string 'TraceonDb' is not configured."));

var app = builder.Build();

await app.Services.ApplyMigrationsAsync();

app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapGroup("/api/identity")
    .MapIdentityApi<ApplicationUser>()
    .WithTags("Identity");

app.MapTrackedActionEndpoints().RequireAuthorization();
app.MapFieldDefinitionEndpoints().RequireAuthorization();
app.MapActionFieldEndpoints().RequireAuthorization();
app.MapActionEntryEndpoints().RequireAuthorization();
app.MapTagEndpoints().RequireAuthorization();

app.Run();
