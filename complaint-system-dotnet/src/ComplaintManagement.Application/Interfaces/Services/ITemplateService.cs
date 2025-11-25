using ComplaintManagement.Application.Common.Models;

namespace ComplaintManagement.Application.Interfaces.Services;

/// <summary>
/// Service for processing communication templates
/// </summary>
public interface ITemplateService
{
    /// <summary>
    /// Replaces placeholders in a template with actual values
    /// </summary>
    /// <param name="template">Template text with placeholders like {{name}}, {{email}}</param>
    /// <param name="data">Dictionary of placeholder names and their values</param>
    /// <returns>Processed template with placeholders replaced</returns>
    string ProcessTemplate(string template, Dictionary<string, object> data);

    /// <summary>
    /// Gets a template by code and processes it with data
    /// </summary>
    Task<Result<string>> GetAndProcessTemplateAsync(
        string templateCode,
        Dictionary<string, object> data,
        Guid? companyId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates template syntax and placeholders
    /// </summary>
    Result<List<string>> ValidateTemplate(string template);

    /// <summary>
    /// Extracts all placeholders from a template
    /// </summary>
    List<string> ExtractPlaceholders(string template);
}
