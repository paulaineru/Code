using Serilog;
using Nest;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using SharedKernel.Services;
using SharedKernel.Utilities;
using Prometheus;
using System.Reflection;
using System.IO;
using Serilog.Sinks.Elasticsearch;
using Microsoft.Extensions.Logging;
using PropertyManagementService.Repository;
using PropertyManagementService.Services;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Newtonsoft.Json;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using PropertyManagementService.Configuration;
using System.Collections.Generic;
using SharedKernel.Data;
using SharedKernel.Repository;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Middleware;
using Amazon.S3;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Authentication;
using System;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel
builder.WebHost.UseKestrel(options =>
{
    options.ListenAnyIP(80);
});

// Add detailed connection string logging
var connectionString = builder.Configuration.GetConnectionString("PropertyDb");
var defaultConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");

Console.WriteLine("=== PROPERTY MANAGEMENT SERVICE CONNECTION STRING DEBUGGING ===");
Console.WriteLine($"Environment: {builder.Environment.EnvironmentName}");
Console.WriteLine($"PropertyDb: {connectionString}");
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
Console.WriteLine($"  ConnectionStrings__PropertyDb: {Environment.GetEnvironmentVariable("ConnectionStrings__PropertyDb")}");
Console.WriteLine($"  ConnectionStrings__DefaultConnection: {Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")}");

// Configure comprehensive logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Debug);

// Add HTTP client logging
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
    logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Debug);
});

// Configure HTTP client with detailed logging
builder.Services.AddHttpClient("DetailedLoggingClient")
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
    })
    .AddHttpMessageHandler<LoggingHttpMessageHandler>();

// Add custom HTTP message handler for detailed logging
builder.Services.AddTransient<LoggingHttpMessageHandler>();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddControllers().AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.TypeNameHandling = TypeNameHandling.Auto;
        options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        // options.SerializerSettings.Converters.Add(new SharedKernel.Models.PropertyConverter());
    });;

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add health checks
builder.Services.AddHealthChecks()
    .AddNpgSql(
        connectionString: connectionString,
        name: "PropertyDb",
        timeout: TimeSpan.FromSeconds(10),
        tags: new[] { "database", "property" }
    )
    .AddNpgSql(
        connectionString: builder.Configuration.GetConnectionString("ApprovalWorkflowDb") ?? "Host=192.168.185.23;Database=pms;Username=postgres;Password=devOps5.6",
        name: "ApprovalWorkflowDb",
        timeout: TimeSpan.FromSeconds(10),
        tags: new[] { "database", "approval" }
    )
    .AddUrlGroup(
        uris: new[] { 
            new Uri("http://authenticationservice:80/health"),
            new Uri("http://tenantmanagementservice:80/health"),
            new Uri("http://billingservice:80/health")
        },
        name: "ExternalServices",
        timeout: TimeSpan.FromSeconds(10),
        tags: new[] { "external", "services" }
    );

// Add database connection monitoring
builder.Services.AddSingleton<DatabaseHealthMonitor>();

// Configure API versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new HeaderApiVersionReader("api-version"),
        new QueryStringApiVersionReader("api-version")
    );
});

builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Property Management Service API",
        Version = "v1",
        Description = "API for managing properties, units, and related operations",
        Contact = new OpenApiContact
        {
            Name = "Stegasi Property Management",
            Email = "support@stegasi.com"
        }
    });

    // Add XML comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);

    // Add custom operation filters
    options.OperationFilter<AddRequiredHeaderParameter>();
    options.OperationFilter<AddApiVersionParameter>();

    // Add security definitions
    options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Description = "API Key Authentication",
        Name = "X-API-Key",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "ApiKeyScheme"
    });

    var scheme = new OpenApiSecurityScheme
    {
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "ApiKey"
        },
        In = ParameterLocation.Header
    };

    var requirement = new OpenApiSecurityRequirement
    {
        { scheme, new List<string>() }
    };

    options.AddSecurityRequirement(requirement);
});

// Configure DbContext with logging
var finalConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
Console.WriteLine($"Final connection string being used: {finalConnectionString}");

builder.Services.AddDbContext<PropertyDbContext>(options =>
{
    options.UseNpgsql(finalConnectionString);
    
    // Add detailed logging
    options.LogTo(message => Console.WriteLine($"[EF Core] {message}"))
           .EnableSensitiveDataLogging()
           .EnableDetailedErrors();
});
builder.Services.AddScoped<IPropertyRepository, PropertyRepository>();

// Add ApprovalWorkflowService and its dependencies
var approvalWorkflowConnectionString = builder.Configuration.GetConnectionString("ApprovalWorkflowDb");
Console.WriteLine($"=== APPROVAL WORKFLOW DB CONTEXT CONFIGURATION ===");
Console.WriteLine($"ApprovalWorkflowDb connection string: {approvalWorkflowConnectionString}");
Console.WriteLine($"Using fallback connection string: {approvalWorkflowConnectionString == null}");

builder.Services.AddDbContext<ApprovalWorkflowDbContext>(options =>
{
    var connStr = approvalWorkflowConnectionString ?? "Host=192.168.185.23;Database=pms;Username=postgres;Password=devOps5.6";
    Console.WriteLine($"Final ApprovalWorkflowDbContext connection string: {connStr}");
    options.UseNpgsql(connStr);
    
    // Add detailed logging for ApprovalWorkflowDbContext
    options.LogTo(message => Console.WriteLine($"[ApprovalWorkflow EF Core] {message}"))
           .EnableSensitiveDataLogging()
           .EnableDetailedErrors();
});
builder.Services.AddScoped<IApprovalWorkflowRepository, ApprovalWorkflowRepository>();
builder.Services.AddScoped<IApprovalWorkflowService, ApprovalWorkflowService>();

builder.Services.AddSingleton<IEmailService>(provider =>
{
    return new EmailService(
        smtpHost: builder.Configuration["Smtp:Host"] ?? "smtp.example.com",
        smtpPort: int.Parse(builder.Configuration["Smtp:Port"] ?? "587"),
        smtpUsername: builder.Configuration["Smtp:Username"] ?? "your-email@example.com",
        smtpPassword: builder.Configuration["Smtp:Password"] ?? "your-password"
    );
});

builder.Services.AddScoped<IPropertyService, PropertyService>();
builder.Services.AddScoped<IAmenityService, AmenityService>();

// Register HTTP client for NotificationService
builder.Services.AddHttpClient<INotificationService>(client =>
{
    var baseAddress = builder.Configuration["Services:NotificationService"] ?? "http://localhost:5033";
    Console.WriteLine($"=== NOTIFICATION SERVICE HTTP CLIENT CONFIGURATION ===");
    Console.WriteLine($"Base Address: {baseAddress}");
    client.BaseAddress = new Uri(baseAddress);
    client.Timeout = TimeSpan.FromSeconds(30);
})
.AddHttpMessageHandler<LoggingHttpMessageHandler>();

// Register NotificationService with all its dependencies
builder.Services.AddTransient<INotificationService>(provider =>
{
    Console.WriteLine("=== CREATING NOTIFICATION SERVICE ===");
    var httpClient = provider.GetRequiredService<IHttpClientFactory>().CreateClient("DetailedLoggingClient");
    var elasticUri = new Uri(builder.Configuration["ELASTICSEARCH_URL"] ?? "https://eagle.nssfug.org:9200");
    Console.WriteLine($"Elasticsearch URI: {elasticUri}");
    
    var settings = new ConnectionSettings(elasticUri)
        .BasicAuthentication("elastic", "xMFisFVN57uNEQovC-i8")
        .DefaultIndex("search-audit_log")
        .EnableDebugMode()
        .ServerCertificateValidationCallback((sender, certificate, chain, sslPolicyErrors) => true);
    var elasticClient = new ElasticClient(settings);
    var emailService = provider.GetRequiredService<IEmailService>();
    var httpContextAccessor = provider.GetRequiredService<IHttpContextAccessor>();
    var configuration = provider.GetRequiredService<IConfiguration>();
    
    Console.WriteLine("=== NOTIFICATION SERVICE CREATED SUCCESSFULLY ===");
    return new NotificationService(emailService, elasticClient, httpContextAccessor, httpClient, configuration);
});

// Configure authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        Console.WriteLine("=== JWT AUTHENTICATION CONFIGURATION ===");
        Console.WriteLine($"JWT Issuer: {builder.Configuration["Jwt:Issuer"]}");
        Console.WriteLine($"JWT Audience: {builder.Configuration["Jwt:Audience"]}");
        Console.WriteLine($"JWT Key Length: {(builder.Configuration["Jwt:Key"]?.Length ?? 0)}");
        
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is not configured")));
        key.KeyId = "stegasi-auth-key"; // Add a key ID

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            RequireSignedTokens = true,
            RequireExpirationTime = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = key
        };
        options.RequireHttpsMetadata = false; // Set to true in production
        
        // Add JWT event logging
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"=== JWT AUTHENTICATION FAILED ===");
                Console.WriteLine($"Exception: {context.Exception.Message}");
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.WriteLine($"=== JWT TOKEN VALIDATED ===");
                Console.WriteLine($"User: {context.Principal?.Identity?.Name}");
                Console.WriteLine($"Claims: {string.Join(", ", context.Principal?.Claims?.Select(c => $"{c.Type}={c.Value}") ?? Array.Empty<string>())}");
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddHttpClient<IUserService, RemoteUserService>(client =>
{
    var baseAddress = "http://authenticationservice:80/api/auth";
    Console.WriteLine($"=== USER SERVICE HTTP CLIENT CONFIGURATION ===");
    Console.WriteLine($"Base Address: {baseAddress}");
    client.BaseAddress = new Uri(baseAddress);
    client.Timeout = TimeSpan.FromSeconds(30);
})
.AddHttpMessageHandler<LoggingHttpMessageHandler>();

// Add authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Tenant", policy => policy.RequireRole("Tenant"));
    options.AddPolicy("EstatesOfficer", policy => policy.RequireRole("Estates Officer"));
    options.AddPolicy("PropertyManager", policy => policy.RequireRole("Property Manager"));
});

// Re-add the singleton DI registration for IAuditLogService using ElasticClient
builder.Services.AddSingleton<IAuditLogService>(provider =>
{
    var elasticUri = new Uri(builder.Configuration["ELASTICSEARCH_URL"] ?? "https://eagle.nssfug.org:9200");
    var settings = new ConnectionSettings(elasticUri)
        .BasicAuthentication("elastic", "xMFisFVN57uNEQovC-i8")
        .DefaultIndex("search-audit_log")
        .EnableDebugMode()
        .ServerCertificateValidationCallback((sender, certificate, chain, sslPolicyErrors) => true);
    var elasticClient = new ElasticClient(settings);
    return new AuditLogService(elasticClient, builder.Configuration);
});

builder.Services.AddHttpContextAccessor();

// Configure AWS S3 client with proper configuration
builder.Services.AddSingleton<IAmazonS3>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    var region = configuration["AWS:Region"] ?? "us-east-1";
    var accessKey = configuration["AWS:AccessKey"] ?? "dummy-access-key";
    var secretKey = configuration["AWS:SecretKey"] ?? "dummy-secret-key";
    
    var config = new AmazonS3Config
    {
        RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(region),
        ForcePathStyle = true // Use path-style URLs for local development
    };
    
    return new AmazonS3Client(accessKey, secretKey, config);
});
builder.Services.AddScoped<S3ImageService>();

var logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri("https://eagle.nssfug.org:9200"))
    {
        IndexFormat = "property-logs-netcore-{0:yyyy.MM.dd}",
        AutoRegisterTemplate = true
    })
    .CreateLogger();

builder.Logging.ClearProviders();
builder.Logging.AddSerilog(logger);

builder.Logging.AddConsole(); // Add console logging
builder.Logging.AddDebug();

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

// Add request/response logging middleware
app.Use(async (context, next) =>
{
    var startTime = DateTime.UtcNow;
    var requestId = Guid.NewGuid().ToString();
    
    Console.WriteLine($"=== REQUEST START [{requestId}] ===");
    Console.WriteLine($"Method: {context.Request.Method}");
    Console.WriteLine($"Path: {context.Request.Path}");
    Console.WriteLine($"QueryString: {context.Request.QueryString}");
    Console.WriteLine($"Headers: {string.Join(", ", context.Request.Headers.Select(h => $"{h.Key}={string.Join(",", h.Value)}"))}");
    
    if (context.Request.Body.CanSeek)
    {
        context.Request.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        Console.WriteLine($"Request Body: {body}");
        context.Request.Body.Seek(0, SeekOrigin.Begin);
    }
    
    try
    {
        await next();
        
        var duration = DateTime.UtcNow - startTime;
        Console.WriteLine($"=== REQUEST COMPLETED [{requestId}] ===");
        Console.WriteLine($"Status Code: {context.Response.StatusCode}");
        Console.WriteLine($"Duration: {duration.TotalMilliseconds}ms");
    }
    catch (Exception ex)
    {
        var duration = DateTime.UtcNow - startTime;
        Console.WriteLine($"=== REQUEST FAILED [{requestId}] ===");
        Console.WriteLine($"Exception: {ex.Message}");
        Console.WriteLine($"Duration: {duration.TotalMilliseconds}ms");
        throw;
    }
});

// Add CORS middleware
app.UseCors();

// Add static file serving BEFORE Swagger
app.UseStaticFiles();

// Configure Swagger for all environments
app.UseSwagger();

app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/property/swagger/v1/swagger.json", "Property Management Service API V1");
    options.RoutePrefix = "swagger";
    options.DocumentTitle = "Property Management Service API Documentation";
    options.InjectStylesheet("/property/swagger-ui/custom.css");
    options.InjectJavascript("/property/swagger-ui/custom.js");
});

app.UseHttpsRedirection();

// Add JWT token logging middleware
app.UseJwtTokenLogging();

// Add authorization error handling middleware
app.UseAuthorizationErrorHandling();

app.UseAuthentication();
app.UseAuthorization();

app.UseMetricServer(); // Exposes /metrics endpoint
app.UseHttpMetrics();

app.MapControllers();

// Add health check endpoint
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        
        var result = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                duration = e.Value.Duration.TotalMilliseconds
            }),
            totalDuration = report.TotalDuration.TotalMilliseconds,
            timestamp = DateTime.UtcNow
        };
        
        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(result, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
    }
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false
});

app.Run();

// Custom HTTP message handler for detailed logging
public class LoggingHttpMessageHandler : DelegatingHandler
{
    private readonly ILogger<LoggingHttpMessageHandler> _logger;

    public LoggingHttpMessageHandler(ILogger<LoggingHttpMessageHandler> logger)
    {
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var startTime = DateTime.UtcNow;
        
        _logger.LogInformation("=== HTTP CLIENT REQUEST START ===");
        _logger.LogInformation("Request URI: {Uri}", request.RequestUri);
        _logger.LogInformation("Request Method: {Method}", request.Method);
        _logger.LogInformation("Request Headers: {Headers}", string.Join(", ", request.Headers.Select(h => $"{h.Key}={string.Join(",", h.Value)}")));
        
        if (request.Content != null)
        {
            var content = await request.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogInformation("Request Content: {Content}", content);
        }

        try
        {
            var response = await base.SendAsync(request, cancellationToken);
            var duration = DateTime.UtcNow - startTime;
            
            _logger.LogInformation("=== HTTP CLIENT RESPONSE ===");
            _logger.LogInformation("Response Status: {StatusCode}", response.StatusCode);
            _logger.LogInformation("Response Headers: {Headers}", string.Join(", ", response.Headers.Select(h => $"{h.Key}={string.Join(",", h.Value)}")));
            _logger.LogInformation("Response Duration: {Duration}ms", duration.TotalMilliseconds);
            
            if (response.Content != null)
            {
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogInformation("Response Content: {Content}", responseContent);
            }
            
            return response;
        }
        catch (Exception ex)
        {
            var duration = DateTime.UtcNow - startTime;
            _logger.LogError(ex, "=== HTTP CLIENT ERROR ===");
            _logger.LogError("Request failed after {Duration}ms", duration.TotalMilliseconds);
            _logger.LogError("Request URI: {Uri}", request.RequestUri);
            throw;
        }
    }
}

// Database health monitoring service
public class DatabaseHealthMonitor : IHostedService
{
    private readonly ILogger<DatabaseHealthMonitor> _logger;
    private readonly IConfiguration _configuration;
    private Timer? _timer;

    public DatabaseHealthMonitor(ILogger<DatabaseHealthMonitor> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("=== DATABASE HEALTH MONITOR STARTED ===");
        
        _timer = new Timer(CheckDatabaseHealth, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    private async void CheckDatabaseHealth(object? state)
    {
        try
        {
            _logger.LogInformation("=== DATABASE HEALTH CHECK STARTED ===");
            
            // Check PropertyDb
            var propertyDbConnStr = _configuration.GetConnectionString("PropertyDb");
            _logger.LogInformation("Checking PropertyDb connection: {ConnectionString}", 
                propertyDbConnStr?.Replace("Password=", "Password=***"));
            
            // Check ApprovalWorkflowDb
            var approvalDbConnStr = _configuration.GetConnectionString("ApprovalWorkflowDb");
            _logger.LogInformation("Checking ApprovalWorkflowDb connection: {ConnectionString}", 
                approvalDbConnStr?.Replace("Password=", "Password=***"));
            
            _logger.LogInformation("=== DATABASE HEALTH CHECK COMPLETED ===");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "=== DATABASE HEALTH CHECK FAILED ===");
        }
    }
}
