using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SharedKernel.Data;
using SharedKernel.Repository;
using SharedKernel.Services;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authentication;
using System;
using Nest;
using Elasticsearch.Net;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel
builder.WebHost.UseKestrel(options =>
{
    options.ListenAnyIP(80);
});

// Add detailed connection string logging
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var approvalWorkflowConnectionString = builder.Configuration.GetConnectionString("ApprovalWorkflowDb");

Console.WriteLine("=== CONNECTION STRING DEBUGGING ===");
Console.WriteLine($"Environment: {builder.Environment.EnvironmentName}");
Console.WriteLine($"DefaultConnection: {connectionString}");
Console.WriteLine($"ApprovalWorkflowDb: {approvalWorkflowConnectionString}");

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
Console.WriteLine($"  ConnectionStrings__DefaultConnection: {Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")}");
Console.WriteLine($"  ConnectionStrings__ApprovalWorkflowDb: {Environment.GetEnvironmentVariable("ConnectionStrings__ApprovalWorkflowDb")}");

// Add services to the container.
builder.Services.AddControllers();

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

// Configure DbContext with logging
var finalConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
Console.WriteLine($"Final connection string being used: {finalConnectionString}");

builder.Services.AddDbContext<ApprovalWorkflowDbContext>(options =>
{
    options.UseNpgsql(finalConnectionString);
    
    // Add detailed logging
    options.LogTo(message => Console.WriteLine($"[EF Core] {message}"))
           .EnableSensitiveDataLogging()
           .EnableDetailedErrors();
});

// Configure JWT Authentication
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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured")))
        };
    });

// Register Elasticsearch client
var elasticSettings = new ConnectionSettings(new Uri(builder.Configuration.GetValue<string>("ElasticsearchUrl") ?? "http://localhost:9200"))
    .DefaultIndex("auditlogs");
var elasticClient = new ElasticClient(elasticSettings);
builder.Services.AddSingleton<IElasticClient>(elasticClient);
builder.Services.AddSingleton(elasticClient); // Register concrete type as well

// Register services
builder.Services.AddScoped<IApprovalWorkflowRepository, ApprovalWorkflowRepository>();
builder.Services.AddScoped<IApprovalWorkflowService, SharedKernel.Services.ApprovalWorkflowService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<IEmailService, EmailService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Approval Workflow API", Version = "v1" });
    
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
    options.SwaggerEndpoint("/approval/swagger/v1/swagger.json", "Approval Workflow Service API V1");
    options.RoutePrefix = "swagger";
    options.DocumentTitle = "Approval Workflow Service API Documentation";
});

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Add health check endpoint
app.MapHealthChecks("/health");

app.Run(); 