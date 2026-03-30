using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.OData.Edm;
using Scalar.AspNetCore;
using Serilog;
using Traceon.Api.Endpoints;
using Traceon.Api.Extensions;
using Traceon.Api.Services;
using Traceon.Application;
using Traceon.Application.Interfaces;
using Traceon.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(
                builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? [])
            .AllowAnyHeader()
            .AllowAnyMethod()
            .WithExposedHeaders("X-Total-Count")));

builder.Services.AddSingleton<IEdmModel>(ODataExtensions.BuildTraceonEdmModel());

builder.Services.AddOpenApi();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

await app.Services.ApplyMigrationsAsync();

app.UseSerilogRequestLogging();

var forwardedHeadersOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost
};
// Trust the Docker network proxy (Caddy) — required because Caddy's IP is not 127.0.0.1
forwardedHeadersOptions.KnownIPNetworks.Clear();
forwardedHeadersOptions.KnownProxies.Clear();
app.UseForwardedHeaders(forwardedHeadersOptions);

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseCors();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapGroup("/api/identity")
    .MapIdentityEndpoints()
    .WithTags("Identity");

app.MapTrackedActionEndpoints().RequireAuthorization();
app.MapFieldDefinitionEndpoints().RequireAuthorization();
app.MapActionFieldEndpoints().RequireAuthorization();
app.MapActionEntryEndpoints().RequireAuthorization();
app.MapEntryEndpoints().RequireAuthorization();
app.MapTagEndpoints().RequireAuthorization();
app.MapDataPortabilityEndpoints();
app.MapFeedbackEndpoints();
app.MapTemplateEndpoints();

app.Run();
