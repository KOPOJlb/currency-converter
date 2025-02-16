using CurrencyConverter.Infrastructure;
using CurrencyConverter.Infrastructure.HealthChecks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using Serilog;


ServiceExtensions.CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.ConfigureSerilog();
    builder.Services.ConfigureOpenTelemetry(builder.Environment.ApplicationName);
    builder.Services.ConfigureRateLimiter(builder.Configuration);
    builder.Services.AddSingleton<GlobalExceptionMiddleware>();
    builder.Services.ConfigureAuthentication(builder.Configuration);

    builder.Services.AddControllers().AddJsonOptions(SerializerSettings.DefaultAction);
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        var securityScheme = new OpenApiSecurityScheme
        {
            Name = "JWT Authentication",
            Description = "Enter JWT Bearer token **_only_**",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Reference = new OpenApiReference
            {
                Id = JwtBearerDefaults.AuthenticationScheme,
                Type = ReferenceType.SecurityScheme
            }
        };
        options.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);
        options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {securityScheme, Array.Empty<string>()}
    });
    });

    builder.Services.AddExchangeRateApplicationService(builder.Configuration);

    builder.Services.AddHealthChecks()
                    .AddCheck<LivenessHealthCheck>(
                        name: nameof(LivenessHealthCheck),
                        tags: [HealthCheckTags.Availability],
                        failureStatus: HealthStatus.Unhealthy)
                    .AddCheck<ReadinessHealthCheck>(
                        name: nameof(ReadinessHealthCheck),
                        tags: [HealthCheckTags.Ready],
                        failureStatus: HealthStatus.Unhealthy);

    var app = builder.Build();
    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "Handled {RequestMethod} {RequestPath} in {ElapsedMilliseconds} ms from {ClientIP} {ClientId}";
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("ClientIP", httpContext.Connection.RemoteIpAddress?.ToString());
            diagnosticContext.Set("ClientId", httpContext.User.FindFirst("client_id")?.Value);
        };
    });
    app.UseMiddleware<GlobalExceptionMiddleware>();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    app.UseAuthentication();
    app.UseAuthorization();

    app.UseRateLimiter();
    app.MapControllers().RequireRateLimiting(Constants.RateLimitPolicies.Fixed);

    app.UseHealthChecks("/health/availability", new HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains(HealthCheckTags.Availability)
    })
    .UseHealthChecks("/health/ready", new HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains(HealthCheckTags.Ready)
    });

    app.Run();
}
catch (Exception e)
{
    Log.Error(e, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}


public partial class Main { }