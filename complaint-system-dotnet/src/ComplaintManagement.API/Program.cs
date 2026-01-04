using ComplaintManagement.API.Authorization;
using ComplaintManagement.API.BackgroundServices;
using ComplaintManagement.API.Extensions;
using ComplaintManagement.API.Middleware;
using ComplaintManagement.Application;
using ComplaintManagement.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

// Set the content root to the application directory (required for Windows Service)
// When running as a service, the working directory is C:\Windows\System32
Directory.SetCurrentDirectory(AppContext.BaseDirectory);

// Setup startup logging directory
var startupLogDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "ComplaintManagement", "Logs");
try { Directory.CreateDirectory(startupLogDir); } catch { }

void LogStartup(string message, bool isError = false)
{
    try
    {
        var logFile = Path.Combine(startupLogDir, $"startup_{DateTime.Now:yyyyMMdd}.log");
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        var level = isError ? "ERROR" : "INFO";
        File.AppendAllText(logFile, $"[{timestamp}] [{level}] {message}{Environment.NewLine}");
    }
    catch { }
}

LogStartup($"Application starting... Version: 1.1.1, Directory: {AppContext.BaseDirectory}");

try
{

var builder = WebApplication.CreateBuilder(args);

// Enable Windows Service support
builder.Host.UseWindowsService();

// Configure Kestrel for long-running operations (like sync)
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    // Increase request timeout to 10 minutes for large sync operations
    serverOptions.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(10);
    serverOptions.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(10);
});

// Add services to the container

// Add Application services (MediatR, FluentValidation, AutoMapper)
builder.Services.AddApplication();

// Add Infrastructure services (DbContext, Repositories, Unit of Work, Services)
builder.Services.AddInfrastructure(builder.Configuration);

// Add Background Services
builder.Services.AddHostedService<AutoEscalationWorker>();
builder.Services.AddHostedService<ComplaintManagement.Infrastructure.Services.EmailPollingBackgroundService>();

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"] ?? "ComplaintManagementSystem",
        ValidAudience = jwtSettings["Audience"] ?? "ComplaintManagementAPI",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero
    };

    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
            {
                context.Response.Headers.Add("Token-Expired", "true");
            }
            return Task.CompletedTask;
        }
    };
});

// Configure Authorization with custom permission-based policies
builder.Services.AddAuthorization(options =>
{
    // Add AdminOnly policy - requires ManageUsers permission
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireAssertion(context =>
            context.User.HasClaim(c => c.Type == "Permission" &&
                (c.Value == "ManageUsers" || c.Value == "ManageRoles" || c.Value == "ManageSettings"))));
});
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();

// Configure CORS - dynamically allow localhost and configured hostnames on any port
var allowedHosts = builder.Configuration.GetSection("Cors:AllowedHosts").Get<string[]>()
    ?? new[] { "localhost", "127.0.0.1" };
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.SetIsOriginAllowed(origin =>
              {
                  var uri = new Uri(origin);
                  var host = uri.Host.ToLower();
                  // Allow localhost and 127.0.0.1 on any port
                  if (host == "localhost" || host == "127.0.0.1")
                      return true;
                  // Allow configured hostnames on any port
                  return allowedHosts.Any(h => h.Equals(host, StringComparison.OrdinalIgnoreCase));
              })
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Add response caching services
builder.Services.AddResponseCaching(options =>
{
    options.MaximumBodySize = 1024 * 1024; // 1 MB max
    options.UseCaseSensitivePaths = false;
    options.SizeLimit = 100 * 1024 * 1024; // 100 MB total cache size
});

// Add rate limiting services
builder.Services.AddMemoryCache();
builder.Services.Configure<AspNetCoreRateLimit.IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.Configure<AspNetCoreRateLimit.IpRateLimitPolicies>(builder.Configuration.GetSection("IpRateLimitPolicies"));
builder.Services.Configure<AspNetCoreRateLimit.ClientRateLimitOptions>(builder.Configuration.GetSection("ClientRateLimiting"));
builder.Services.Configure<AspNetCoreRateLimit.ClientRateLimitPolicies>(builder.Configuration.GetSection("ClientRateLimitPolicies"));
builder.Services.AddSingleton<AspNetCoreRateLimit.IIpPolicyStore, AspNetCoreRateLimit.MemoryCacheIpPolicyStore>();
builder.Services.AddSingleton<AspNetCoreRateLimit.IRateLimitCounterStore, AspNetCoreRateLimit.MemoryCacheRateLimitCounterStore>();
builder.Services.AddSingleton<AspNetCoreRateLimit.IClientPolicyStore, AspNetCoreRateLimit.MemoryCacheClientPolicyStore>();
builder.Services.AddSingleton<AspNetCoreRateLimit.IRateLimitConfiguration, AspNetCoreRateLimit.RateLimitConfiguration>();
builder.Services.AddSingleton<AspNetCoreRateLimit.IProcessingStrategy, AspNetCoreRateLimit.AsyncKeyLockProcessingStrategy>();

// Add HttpClient factory for OAuth token exchange
builder.Services.AddHttpClient();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Use numeric enum values for better frontend compatibility
        // Frontend sends and receives enum values as numbers (0, 1, 2, etc.)

        // Handle circular references in entity navigation properties
        // This prevents JsonException: "A possible object cycle was detected"
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Complaint Management API",
        Version = "v1",
        Description = "API for managing complaints with multi-level escalation and role-based access control"
    });

    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
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
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Configure path base for IIS sub-application hosting
// When hosted under IIS at /api, this ensures routes work correctly
var pathBase = Environment.GetEnvironmentVariable("ASPNETCORE_PATHBASE");
if (!string.IsNullOrEmpty(pathBase))
{
    app.UsePathBase(pathBase);
    LogStartup($"Using path base: {pathBase}");
}

// Seed database
await app.SeedDatabaseAsync();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Only use HTTPS redirection in production
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// Use CORS
app.UseCors("AllowAngularApp");

// Use response caching middleware (MUST be before UseAuthentication)
app.UseResponseCaching();

// Use rate limiting middleware (before authentication for IP-based, after for client-based)
app.UseMiddleware<AspNetCoreRateLimit.IpRateLimitMiddleware>();
app.UseMiddleware<AspNetCoreRateLimit.ClientRateLimitMiddleware>();

// Use error logging middleware (before authentication)
app.UseErrorLogging();

// Enable static file serving for uploaded files
app.UseStaticFiles();

// For Windows Service mode: Serve Angular frontend from WWW folder (sibling to API folder)
// This allows the API to serve the frontend when not using IIS
var wwwPath = Path.Combine(AppContext.BaseDirectory, "..", "WWW");
if (Directory.Exists(wwwPath))
{
    LogStartup($"Serving frontend from: {wwwPath}");
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(Path.GetFullPath(wwwPath)),
        RequestPath = ""
    });

    // Serve index.html for SPA routes (Angular routing support)
    app.MapFallbackToFile("index.html", new StaticFileOptions
    {
        FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(Path.GetFullPath(wwwPath))
    });
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

LogStartup("Application configured successfully, starting to listen on port 5000...");
app.Run();
LogStartup("Application stopped normally.");

}
catch (Exception ex)
{
    LogStartup($"FATAL ERROR during startup: {ex.Message}", true);
    LogStartup($"Exception Type: {ex.GetType().FullName}", true);
    LogStartup($"Stack Trace: {ex.StackTrace}", true);
    if (ex.InnerException != null)
    {
        LogStartup($"Inner Exception: {ex.InnerException.Message}", true);
        LogStartup($"Inner Stack Trace: {ex.InnerException.StackTrace}", true);
    }
    throw;
}
