using CurrencyConverter.ApplicationServices;
using CurrencyConverter.Infrastructure.FrankfurterService;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Serilog;
using Serilog.ThrowContext;
using System.Text;
using System.Threading.RateLimiting;


namespace CurrencyConverter.Infrastructure
{
    public static class ServiceExtensions
    {
        public static void ConfigureAuthentication(this IServiceCollection serviceCollection, IConfiguration configuration)
        {
            serviceCollection.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(jwtOptions =>
                {
                    jwtOptions.Audience = configuration["JwtSettings:Audience"];
                    jwtOptions.TokenValidationParameters = new()
                    {
                        ValidIssuer = configuration["JwtSettings:ValidIssuer"],
                        RequireExpirationTime = configuration.GetValue<bool>("JwtSettings:RequireExpirationTime"),
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtSettings:IssuerSigningKey"]!))
                    };
                });
        }
        public static void ConfigureOpenTelemetry(this IServiceCollection serviceCollection, string applicationName)
        {
            serviceCollection.AddOpenTelemetry()
                .ConfigureResource(resource => resource
                    .AddService(serviceName: applicationName))
                .WithMetrics(metrics => metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddConsoleExporter()
                    .AddOtlpExporter())
                .WithTracing(tracing => tracing
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddConsoleExporter()
                    .AddOtlpExporter());
        }
        public static void AddExchangeRateApplicationService(this IServiceCollection serviceCollection, IConfiguration configuration)
        {
            serviceCollection.AddTransient<ExchangeRateApplicationService>();
            serviceCollection.AddSingleton<IValidator<ConvertRequest>, ConvertRequestValidator>();
            serviceCollection.AddSingleton<IValidator<LatestRequest>, LatestRequestValidator>();
            serviceCollection.AddSingleton<IValidator<ListRequest>, ListRequestValidator>();
            serviceCollection.AddKeyedSingleton<ICurrencyProvider, CurrencyProvider>(CurrencyProviders.Default);
            

            serviceCollection.AddFrankfurter(configuration);
        }

        public static void AddFrankfurter(this IServiceCollection serviceCollection, IConfiguration configuration)
        {
            serviceCollection.AddMemoryCache();
            serviceCollection.AddHttpClient(Constants.HttpClientNames.FrankfurterHttpClient);
            serviceCollection.AddKeyedTransient<ICurrencyProvider, Frankfurter>(CurrencyProviders.Frankfurter);
            serviceCollection.Configure<FrankfurterConfig>(configuration.GetSection(nameof(FrankfurterConfig)));
            serviceCollection.AddCircuitBreaker(Constants.ResiliencePolicies.FrankfurterPolicy);
        }

        public static void ConfigureSerilog(this WebApplicationBuilder builder)
        {
            builder.Host.UseSerilog((context, serviceProvider, configuration) => configuration
               .ReadFrom.Configuration(context.Configuration)
               .Enrich.With<ThrowContextEnricher>());
        }

        public static void CreateBootstrapLogger()
        {
            Log.Logger = new LoggerConfiguration()
                .Enrich.With<ThrowContextEnricher>()
                .WriteTo.Console()
                .CreateBootstrapLogger();
        }

        public static void ConfigureRateLimiter(this IServiceCollection serviceCollection, IConfiguration configuration)
        {
            serviceCollection.AddRateLimiter(o => o
                .AddFixedWindowLimiter(policyName: Constants.RateLimitPolicies.Fixed, options =>
                {
                    options.PermitLimit = 10;
                    options.Window = TimeSpan.FromMinutes(1);
                    options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    options.QueueLimit = 2;
                }));
        }

        private static void AddCircuitBreaker(this IServiceCollection sc, string policyKey)
        {
            sc.AddResiliencePipeline(policyKey, (builder, context) =>
            {
                builder
                .AddRetry(new RetryStrategyOptions
                {
                    MaxRetryAttempts = 5,
                    BackoffType = DelayBackoffType.Exponential,
                    ShouldHandle = new PredicateBuilder()
                        .Handle<TaskCanceledException>()
                })
                .AddCircuitBreaker(new CircuitBreakerStrategyOptions
                {
                    BreakDuration = TimeSpan.FromSeconds(60),
                    MinimumThroughput = 3,
                    ShouldHandle = new PredicateBuilder()
                        .Handle<TaskCanceledException>()
                });
            });
        }
    }
}
