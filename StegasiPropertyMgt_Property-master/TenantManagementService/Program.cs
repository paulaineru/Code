using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using TenantManagementService.Repository;
using TenantManagementService.Services;
using SharedKernel.Services;
using SharedKernel.Models;
using SharedKernel.Utilities;
using SharedKernel.Middleware;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Nest;
using Polly;
using Polly.Extensions.Http;
using System.Net.Http;
using Polly.Retry;
using Polly.CircuitBreaker;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Authentication;
using System;
using Elasticsearch.Net;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel
builder.WebHost.UseKestrel(options =>
{
    options.ListenAnyIP(80);
});

// Add detailed connection string logging
var connectionString = builder.Configuration.GetConnectionString("TenantDb");
var defaultConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");

Console.WriteLine("=== TENANT MANAGEMENT SERVICE CONNECTION STRING DEBUGGING ===");
Console.WriteLine($"Environment: {builder.Environment.EnvironmentName}");
Console.WriteLine($"TenantDb: {connectionString}");
Console.WriteLine($"DefaultConnection: {defaultConnectionString}");

// Log all connection strings from configuration
var allConnectionStrings = builder.Configuration.GetSection("ConnectionStrings").GetChildren();
Console.WriteLine("All Connection Strings in Configuration:");
foreach (var connStr in allConnectionStrings)
{
    var sanitizedValue = connStr.Value?.Contains("Password=") == true 
        ? connStr.Value.Replace(connStr.Value.Split("Password=")[1], "Password=***") 
        : connStr.Value;
    Console.WriteLine($"  {connStr.Key}: {sanitizedValue}");
}

// Log environment variables
Console.WriteLine("Environment Variables:");
Console.WriteLine($"  ConnectionStrings__TenantDb: {Environment.GetEnvironmentVariable("ConnectionStrings__TenantDb")}");
Console.WriteLine($"  ConnectionStrings__DefaultConnection: {Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")}");

// Add services to the container
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.TypeNameHandling = TypeNameHandling.Auto; // Include type info for polymorphism
        options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore; // Handle circular references
    });

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Add health checks
builder.Services.AddHealthChecks();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Tenant Management API", Version = "v1" });
    
    // Add JWT authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// Configure DbContext with logging
var finalConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
Console.WriteLine($"Final connection string being used: {finalConnectionString}");

builder.Services.AddDbContext<TenantDbContext>(options =>
{
    options.UseNpgsql(finalConnectionString);
    
    // Add detailed logging
    options.LogTo(message => Console.WriteLine($"[EF Core] {message}"))
           .EnableSensitiveDataLogging()
           .EnableDetailedErrors();
});

// Register Repositories
builder.Services.AddHttpClient<IPropertyService, PropertyHttpClientService>(client =>
{
    client.BaseAddress = new Uri("http://propertymanagementservice:80/api/Property/"); // Updated for Docker networking
});

builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ILeaseService, LeaseService>();
builder.Services.AddScoped<ILeaseRepository, LeaseRepository>();
builder.Services.AddScoped<ITenantRepository, TenantRepository>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<IRenewalRequestRepository, RenewalRequestRepository>();
builder.Services.AddScoped<ITerminationProcessRepository, TerminationProcessRepository>();

builder.Services.AddHttpClient<IBillingClient, BillingHttpClient>(client =>
{
    client.BaseAddress = new Uri("http://billingservice:80/api"); // Updated for Docker networking
});

builder.Services.AddHttpClient<IUserService, TenantManagementService.Services.RemoteUserService>(client =>
{
    client.BaseAddress = new Uri("http://authenticationservice:80/api/v1/Auth"); // Updated for Docker networking
});

// Register Services
builder.Services.AddSingleton<IEmailService>(provider =>
{
    return new EmailService(
        smtpHost: builder.Configuration["Smtp:Host"] ?? "smtp.example.com",
        smtpPort: int.Parse(builder.Configuration["Smtp:Port"] ?? "587"),
        smtpUsername: builder.Configuration["Smtp:Username"] ?? "your-email@example.com",
        smtpPassword: builder.Configuration["Smtp:Password"] ?? "your-password"
    );
});

// Configure Elasticsearch for notifications
var notificationElasticUri = new Uri(builder.Configuration["ELASTICSEARCH_URL"] ?? "http://localhost:9200");
var notificationSettings = new ConnectionSettings(notificationElasticUri).DefaultIndex("notifications");
var notificationElasticClient = new ElasticClient(notificationSettings);

// Register notification services
builder.Services.AddSingleton<IElasticClient>(notificationElasticClient);
builder.Services.AddSingleton(notificationElasticClient);
builder.Services.AddScoped<INotificationService, SharedKernel.Services.NotificationService>();
builder.Services.AddScoped<ITenantNotificationManager, TenantNotificationManager>();

// Register Audit Log Service - Fixed registration
builder.Services.AddScoped<IAuditLogService, AuditLogService>();

// Configure JWT authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

var app = builder.Build();

// Configure PathBase for reverse proxy support
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedPrefix
});

// Add path base middleware for reverse proxy support
var pathBase = Environment.GetEnvironmentVariable("ASPNETCORE_PATHBASE");
if (!string.IsNullOrEmpty(pathBase))
{
    app.UsePathBase(pathBase);
    app.Logger.LogInformation("Using path base: {PathBase}", pathBase);
}

// Add CORS middleware
app.UseCors();

// Configure Swagger for all environments
app.UseSwagger();

app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/tenant/swagger/v1/swagger.json", "Tenant Management Service API V1");
    options.RoutePrefix = "swagger";
    options.DocumentTitle = "Tenant Management Service API Documentation";
});

app.UseHttpsRedirection();

// Add authorization error handling middleware
app.UseAuthorizationErrorHandling();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Add health check endpoint
app.MapHealthChecks("/health");

app.Run();
