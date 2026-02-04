// BillingService/Program.cs
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using BillingService.Services;
using Microsoft.AspNetCore.Http;
using SharedKernel.Services;
using Nest;
using Elasticsearch.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using BillingService.Data;
using Microsoft.AspNetCore.Mvc.NewtonsoftJson;
using BillingService.Repository;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel
builder.WebHost.UseKestrel(options =>
{
    options.ListenAnyIP(80);
});

// Add detailed connection string logging
var connectionString = builder.Configuration.GetConnectionString("BillingDb");
var defaultConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");

Console.WriteLine("=== BILLING SERVICE CONNECTION STRING DEBUGGING ===");
Console.WriteLine($"Environment: {builder.Environment.EnvironmentName}");
Console.WriteLine($"BillingDb: {connectionString}");
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
Console.WriteLine($"  ConnectionStrings__BillingDb: {Environment.GetEnvironmentVariable("ConnectionStrings__BillingDb")}");
Console.WriteLine($"  ConnectionStrings__DefaultConnection: {Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")}");

// Add services to the container
builder.Services.AddControllers()
    .AddNewtonsoftJson(options => options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

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

// Add HttpClient and HttpContextAccessor
builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();

// Add DbContext with logging
var finalConnectionString = builder.Configuration.GetConnectionString("BillingDb");
Console.WriteLine($"Final connection string being used: {finalConnectionString}");

builder.Services.AddDbContext<BillingDbContext>(options =>
{
    options.UseNpgsql(finalConnectionString);
    
    // Add detailed logging
    options.LogTo(message => Console.WriteLine($"[EF Core] {message}"))
           .EnableSensitiveDataLogging()
           .EnableDetailedErrors();
});

// Add services
builder.Services.AddScoped<IBillingService, BillingService.Services.BillingService>();
builder.Services.AddScoped<IBillingRepository, BillingRepository>();

// Authentication
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

builder.Services.AddAuthorization();

builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
});

// Register Swagger
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Billing Service API", Version = "v1" });
    
    // Add JWT authentication to Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
    
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});

// Register Elasticsearch
var elasticSettings = new ConnectionSettings(new Uri(builder.Configuration.GetValue<string>("ElasticsearchUrl") ?? "http://localhost:9200"))
    .DefaultIndex("auditlogs");
var elasticClient = new ElasticClient(elasticSettings);
builder.Services.AddSingleton<IElasticClient>(elasticClient);
builder.Services.AddSingleton(elasticClient);

// Register repositories and services
builder.Services.AddScoped<INotificationService, SharedKernel.Services.NotificationService>();
builder.Services.AddScoped<IAuditLogService, SharedKernel.Services.AuditLogService>();
builder.Services.AddScoped<IUserService, SharedKernel.Services.UserService>();
builder.Services.AddScoped<IEmailService, SharedKernel.Services.EmailService>();
builder.Services.AddScoped<IPropertyClient, PropertyClient>();

// Build the application
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

// Serve Swagger for all environments, after path base and CORS
app.UseSwagger();
app.Logger.LogInformation("Swagger middleware registered");
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("swagger/v1/swagger.json", "Billing Service API V1");
    options.RoutePrefix = string.Empty;
    options.DocumentTitle = "Billing Service API Documentation";
});

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Add health check endpoint
app.MapHealthChecks("/health");

app.Run();
