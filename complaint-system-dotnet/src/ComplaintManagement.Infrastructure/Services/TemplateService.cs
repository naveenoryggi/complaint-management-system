using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.Interfaces.Services;
using ComplaintManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace ComplaintManagement.Infrastructure.Services;

/// <summary>
/// Template processing service for replacing placeholders
/// </summary>
public class TemplateService : ITemplateService
{
    private readonly ComplaintDbContext _context;
    private readonly ILogger<TemplateService> _logger;
    private static readonly Regex PlaceholderRegex = new Regex(@"\{\{(\w+)\}\}", RegexOptions.Compiled);

    public TemplateService(ComplaintDbContext context, ILogger<TemplateService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public string ProcessTemplate(string template, Dictionary<string, object> data)
    {
        if (string.IsNullOrEmpty(template))
        {
            return template;
        }

        return PlaceholderRegex.Replace(template, match =>
        {
            var placeholder = match.Groups[1].Value;

            if (data.TryGetValue(placeholder, out var value))
            {
                return value?.ToString() ?? string.Empty;
            }

            // Also try case-insensitive lookup
            var key = data.Keys.FirstOrDefault(k =>
                string.Equals(k, placeholder, StringComparison.OrdinalIgnoreCase));

            if (key != null)
            {
                return data[key]?.ToString() ?? string.Empty;
            }

            _logger.LogWarning("Placeholder {Placeholder} not found in data", placeholder);
            return match.Value; // Return original placeholder if not found
        });
    }

    public async Task<Result<string>> GetAndProcessTemplateAsync(
        string templateCode,
        Dictionary<string, object> data,
        Guid? companyId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Get template by code
            var template = await _context.CommunicationTemplates
                .Where(t => t.Code == templateCode && t.IsActive)
                .Where(t => companyId == null || t.CompanyId == null || t.CompanyId == companyId)
                .OrderByDescending(t => t.CompanyId) // Prefer company-specific template
                .FirstOrDefaultAsync(cancellationToken);

            if (template == null)
            {
                return Result<string>.Failure($"Template with code '{templateCode}' not found");
            }

            // Process the template
            var processedBody = ProcessTemplate(template.Body, data);
            var processedHtmlBody = !string.IsNullOrEmpty(template.HtmlBody)
                ? ProcessTemplate(template.HtmlBody, data)
                : null;

            // Return HTML body if available, otherwise plain text
            var result = processedHtmlBody ?? processedBody;

            return Result<string>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing template {TemplateCode}", templateCode);
            return Result<string>.Failure($"Error processing template: {ex.Message}");
        }
    }

    public Result<List<string>> ValidateTemplate(string template)
    {
        try
        {
            if (string.IsNullOrEmpty(template))
            {
                return Result<List<string>>.Failure("Template is empty");
            }

            var placeholders = ExtractPlaceholders(template);

            // Check for balanced braces
            var openCount = template.Count(c => c == '{');
            var closeCount = template.Count(c => c == '}');

            if (openCount != closeCount)
            {
                return Result<List<string>>.Failure("Unbalanced braces in template");
            }

            return Result<List<string>>.Success(placeholders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating template");
            return Result<List<string>>.Failure($"Template validation error: {ex.Message}");
        }
    }

    public List<string> ExtractPlaceholders(string template)
    {
        if (string.IsNullOrEmpty(template))
        {
            return new List<string>();
        }

        var matches = PlaceholderRegex.Matches(template);
        return matches
            .Select(m => m.Groups[1].Value)
            .Distinct()
            .ToList();
    }
}
