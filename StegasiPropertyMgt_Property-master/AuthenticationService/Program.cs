// AuthenticationService/Program.cs
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using AuthenticationService.Repository;
using AuthenticationService.Services;
using SharedKernel.Utilities;
using SharedKernel.Services;
using SharedKernel.Middleware;
using Microsoft.EntityFrameworkCore;
using Nest;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using AuthenticationService.Configuration;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;
using AuthenticationService.Middleware;
using Microsoft.OpenApi.Models;
using System.Reflection;
using Swashbuckle.AspNetCore.SwaggerUI;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.OpenApi.Models;
using System;
using Elasticsearch.Net;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel
builder.WebHost.UseKestrel(options =>
{
    options.ListenAnyIP(80);
});

// Add detailed connection string logging
var connectionString = builder.Configuration.GetConnectionString("AuthDb");
var defaultConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");

Console.WriteLine("=== AUTHENTICATION SERVICE CONNECTION STRING DEBUGGING ===");
Console.WriteLine($"Environment: {builder.Environment.EnvironmentName}");
Console.WriteLine($"AuthDb: {connectionString}");
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
Console.WriteLine($"  ConnectionStrings__AuthDb: {Environment.GetEnvironmentVariable("ConnectionStrings__AuthDb")}");
Console.WriteLine($"  ConnectionStrings__DefaultConnection: {Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")}");

// Add services to the container
builder.Services.AddControllers();

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
builder.Services.AddHealthChecks();

// Add API versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new HeaderApiVersionReader("x-api-version"),
        new MediaTypeApiVersionReader("x-api-version")
    );
});

// Add API explorer for versioned API documentation
builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Swagger options
builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();

builder.Services.AddHttpContextAccessor();

// Add database context with logging
var finalConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
Console.WriteLine($"Final connection string being used: {finalConnectionString}");

builder.Services.AddDbContext<AuthDbContext>(options =>
{
    options.UseNpgsql(finalConnectionString);
    
    // Add detailed logging
    options.LogTo(message => Console.WriteLine($"[EF Core] {message}"))
           .EnableSensitiveDataLogging()
           .EnableDetailedErrors();
});

// Add services
builder.Services.AddScoped<IAuthDbContext, AuthDbContext>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IRoleAuditService, RoleAuditService>();
builder.Services.AddScoped<IUserService, AuthenticationService.Services.UserService>();
builder.Services.AddScoped<IUserRegistrationService, AuthenticationService.Services.UserService>();

// Configure RoleConfiguration
builder.Services.Configure<RoleConfiguration>(builder.Configuration.GetSection("RoleConfiguration"));
builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<RoleConfiguration>>().Value);

// Add JWT authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured")));
        key.KeyId = "stegasi-auth-key"; // Add a key ID

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            RequireSignedTokens = true,
            RequireExpirationTime = true,
            ClockSkew = TimeSpan.Zero
        };
        options.RequireHttpsMetadata = false; // Set to true in production
    });

// Add authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
    options.AddPolicy("RequireUserRole", policy => policy.RequireRole("User"));
});

// Add Elasticsearch client
var elasticSettings = new ConnectionSettings(new Uri(builder.Configuration["Elasticsearch:Url"] ?? "http://localhost:9200"))
    .DefaultIndex("auth-service-metrics");
var elasticClient = new ElasticClient(elasticSettings);
builder.Services.AddSingleton<IElasticClient>(elasticClient);
builder.Services.AddSingleton(elasticClient); // Register concrete type as well

// Add authentication service
builder.Services.AddScoped<IAuthService, AuthService>();

// Add role service
builder.Services.AddScoped<IRoleService, RoleService>();

// Add role audit service
builder.Services.AddScoped<IRoleAuditService, RoleAuditService>();

// Register IRoleHierarchyService and RoleHierarchyService
builder.Services.AddScoped<IRoleHierarchyService, RoleHierarchyService>();

// Register IEmailService
builder.Services.AddSingleton<IEmailService>(provider =>
{
    return new EmailService(
        smtpHost: builder.Configuration["Smtp:Host"] ?? "smtp.example.com",
        smtpPort: int.Parse(builder.Configuration["Smtp:Port"] ?? "587"),
        smtpUsername: builder.Configuration["Smtp:Username"] ?? "your-email@example.com",
        smtpPassword: builder.Configuration["Smtp:Password"] ?? "your-password"
    );
});

// Register INotificationService
builder.Services.AddSingleton<INotificationService, NotificationService>();

// Register IUserRepository
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Register AuditLogService with Elasticsearch
builder.Services.AddHttpClient<IAuditLogService, AuditLogService>();
builder.Services.AddHttpClient<INotificationService, NotificationService>();

builder.Services.AddPerformanceMonitoring(options =>
{
    options.SlowRequestThresholdMs = 1000; // 1 second
    options.HighMemoryThresholdBytes = 100 * 1024 * 1024; // 100 MB
});

builder.Logging.ClearProviders();
builder.Logging.AddConsole(); // Add console logging
builder.Logging.AddDebug(); 
// Build the application
var app = builder.Build();

// Configure PathBase for reverse proxy support using X-Forwarded-Prefix (MOST SCALABLE APPROACH)
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

// Add performance monitoring middleware early in the pipeline
app.UsePerformanceMonitoring();
app.UseCorrelationId();
app.UseRequestResponseLogging();
app.UseExceptionHandling();

// Get the API version description provider
var apiVersionDescriptionProvider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

// Configure the HTTP request pipeline
app.Logger.LogInformation("Configuring Swagger for environment: {Environment}", app.Environment.EnvironmentName);

// Add static file serving BEFORE Swagger
app.UseStaticFiles();

app.UseSwagger();

app.Logger.LogInformation("Swagger middleware configured");

app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/auth/swagger/v1/swagger.json", "Authentication Service API v1");
    options.RoutePrefix = "swagger";
    options.DocumentTitle = "Stegasi Property Management - Authentication API";
    options.DefaultModelsExpandDepth(-1); // Hide models section by default
    options.DocExpansion(DocExpansion.None); // Collapse all endpoints by default
    options.EnableDeepLinking(); // Enable deep linking
    options.DisplayRequestDuration(); // Show request duration
    options.EnableValidator(); // Enable swagger validator
    options.InjectStylesheet("/auth/swagger-ui/custom.css"); // Custom CSS
    options.InjectJavascript("/auth/swagger-ui/custom.js"); // Custom JS
    
    // Add OAuth2 configuration if needed
    options.OAuthClientId("swagger-ui");
    options.OAuthAppName("Swagger UI");
    options.OAuthUsePkce();
});

app.Logger.LogInformation("SwaggerUI middleware configured");

// Configure middleware
app.UseHttpsRedirection();

// Add authorization error handling middleware
app.UseAuthorizationErrorHandling();

app.UseAuthentication(); // Ensure this is added
app.UseAuthorization();   // Ensure this is added
app.MapControllers();

// Add health check endpoint
app.MapHealthChecks("/health");

// Apply migrations
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    db.Database.Migrate();
}

// Add middleware in the correct order
app.UseCorrelationId();
app.UseRoleLogging(); // Add role logging middleware
app.UseExceptionHandling();
app.UseRequestResponseLogging();

app.Run();