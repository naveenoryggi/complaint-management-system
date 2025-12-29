using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace ComplaintManagement.API.Middleware;

/// <summary>
/// Middleware for comprehensive error logging to file and console
/// </summary>
public class ErrorLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorLoggingMiddleware> _logger;
    private readonly string _logDirectory;

    public ErrorLoggingMiddleware(RequestDelegate next, ILogger<ErrorLoggingMiddleware> logger, IWebHostEnvironment env, IConfiguration configuration)
    {
        _next = next;
        _logger = logger;

        // Use configurable log path, fallback to ProgramData which has write permissions
        var configuredPath = configuration["Logging:LogPath"];
        if (!string.IsNullOrEmpty(configuredPath))
        {
            _logDirectory = configuredPath;
        }
        else
        {
            // Default to ProgramData which has write permissions for all users
            _logDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "ComplaintManagement", "Logs");
        }

        // Ensure log directory exists
        try
        {
            if (!Directory.Exists(_logDirectory))
            {
                Directory.CreateDirectory(_logDirectory);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not create log directory at {LogDirectory}, falling back to temp", _logDirectory);
            _logDirectory = Path.Combine(Path.GetTempPath(), "ComplaintManagement", "Logs");
            if (!Directory.Exists(_logDirectory))
            {
                Directory.CreateDirectory(_logDirectory);
            }
        }

        _logger.LogInformation("Logging to directory: {LogDirectory}", _logDirectory);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestId = Guid.NewGuid().ToString();

        // Log request details
        await LogRequestAsync(context, requestId);

        try
        {
            await _next(context);
            stopwatch.Stop();

            // Log response details for non-success responses
            if (context.Response.StatusCode >= 400)
            {
                await LogErrorResponseAsync(context, requestId, stopwatch.ElapsedMilliseconds);
            }
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            await LogExceptionAsync(context, ex, requestId, stopwatch.ElapsedMilliseconds);
            throw; // Re-throw to let the global exception handler deal with it
        }
    }

    private async Task LogRequestAsync(HttpContext context, string requestId)
    {
        try
        {
            var request = context.Request;

            // Read request body
            request.EnableBuffering();
            var body = await new StreamReader(request.Body).ReadToEndAsync();
            request.Body.Position = 0;

            var logEntry = new
            {
                Timestamp = DateTime.UtcNow,
                RequestId = requestId,
                Type = "REQUEST",
                Method = request.Method,
                Path = request.Path,
                QueryString = request.QueryString.ToString(),
                Body = SanitizeRequestBody(body),
                Headers = request.Headers.Where(h => !h.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase))
                    .ToDictionary(h => h.Key, h => h.Value.ToString()),
                UserAgent = request.Headers["User-Agent"].ToString(),
                RemoteIP = context.Connection.RemoteIpAddress?.ToString()
            };

            var logMessage = JsonSerializer.Serialize(logEntry, new JsonSerializerOptions { WriteIndented = true });
            await WriteToLogFileAsync("requests", logMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging request");
        }
    }

    private async Task LogErrorResponseAsync(HttpContext context, string requestId, long elapsedMs)
    {
        try
        {
            var logEntry = new
            {
                Timestamp = DateTime.UtcNow,
                RequestId = requestId,
                Type = "ERROR_RESPONSE",
                StatusCode = context.Response.StatusCode,
                Method = context.Request.Method,
                Path = context.Request.Path,
                ElapsedMs = elapsedMs,
                User = context.User.Identity?.Name ?? "Anonymous"
            };

            var logMessage = JsonSerializer.Serialize(logEntry, new JsonSerializerOptions { WriteIndented = true });
            await WriteToLogFileAsync("errors", logMessage);

            _logger.LogWarning("HTTP {StatusCode} - {Method} {Path} - {ElapsedMs}ms [RequestId: {RequestId}]",
                context.Response.StatusCode,
                context.Request.Method,
                context.Request.Path,
                elapsedMs,
                requestId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging error response");
        }
    }

    private async Task LogExceptionAsync(HttpContext context, Exception exception, string requestId, long elapsedMs)
    {
        try
        {
            var logEntry = new
            {
                Timestamp = DateTime.UtcNow,
                RequestId = requestId,
                Type = "EXCEPTION",
                Method = context.Request.Method,
                Path = context.Request.Path,
                QueryString = context.Request.QueryString.ToString(),
                StatusCode = context.Response.StatusCode,
                ElapsedMs = elapsedMs,
                Exception = new
                {
                    Message = exception.Message,
                    StackTrace = exception.StackTrace,
                    InnerException = exception.InnerException?.Message,
                    InnerStackTrace = exception.InnerException?.StackTrace,
                    Type = exception.GetType().Name
                },
                User = context.User.Identity?.Name ?? "Anonymous",
                UserClaims = context.User.Claims.Select(c => new { c.Type, c.Value }).ToList()
            };

            var logMessage = JsonSerializer.Serialize(logEntry, new JsonSerializerOptions { WriteIndented = true });
            await WriteToLogFileAsync("exceptions", logMessage);

            _logger.LogError(exception,
                "EXCEPTION - {Method} {Path} - {ElapsedMs}ms [RequestId: {RequestId}]",
                context.Request.Method,
                context.Request.Path,
                elapsedMs,
                requestId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging exception");
        }
    }

    private async Task WriteToLogFileAsync(string logType, string message)
    {
        try
        {
            var fileName = $"{logType}_{DateTime.UtcNow:yyyyMMdd}.log";
            var filePath = Path.Combine(_logDirectory, fileName);

            var logLine = $"{message}{Environment.NewLine}{new string('=', 100)}{Environment.NewLine}";
            await File.AppendAllTextAsync(filePath, logLine);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write to log file");
        }
    }

    private string SanitizeRequestBody(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
            return body;

        try
        {
            // Try to parse as JSON and remove sensitive fields
            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            var sensitiveFields = new[] { "password", "token", "secret", "key", "authorization" };

            // For simplicity, just mask if we detect sensitive fields
            foreach (var field in sensitiveFields)
            {
                if (body.Contains(field, StringComparison.OrdinalIgnoreCase))
                {
                    return "[CONTAINS SENSITIVE DATA - REDACTED]";
                }
            }

            return body;
        }
        catch
        {
            return body;
        }
    }
}

/// <summary>
/// Extension method to register the error logging middleware
/// </summary>
public static class ErrorLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseErrorLogging(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ErrorLoggingMiddleware>();
    }
}
