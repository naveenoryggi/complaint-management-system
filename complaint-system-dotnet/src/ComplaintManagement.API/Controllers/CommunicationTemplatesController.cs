using ComplaintManagement.Application.Interfaces.Services;
using ComplaintManagement.Domain.Entities.Communication;
using ComplaintManagement.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ComplaintManagement.API.Controllers;

[ApiController]
[Route("api/communication-templates")]
[Authorize]
public class CommunicationTemplatesController : ControllerBase
{
    private readonly ComplaintDbContext _context;
    private readonly ITemplateService _templateService;
    private readonly ILogger<CommunicationTemplatesController> _logger;

    public CommunicationTemplatesController(
        ComplaintDbContext context,
        ITemplateService templateService,
        ILogger<CommunicationTemplatesController> logger)
    {
        _context = context;
        _templateService = templateService;
        _logger = logger;
    }

    [HttpGet]
    [ResponseCache(Duration = 600, VaryByQueryKeys = new[] { "includeInactive", "channel", "companyId" }, VaryByHeader = "Authorization")]
    public async Task<IActionResult> GetAll(
        [FromQuery] bool includeInactive = false,
        [FromQuery] string? channel = null,
        [FromQuery] Guid? companyId = null)
    {
        try
        {
            var query = _context.CommunicationTemplates.AsQueryable();
            if (!includeInactive) query = query.Where(t => t.IsActive);

            if (!string.IsNullOrEmpty(channel) && Enum.TryParse<Domain.Enums.CommunicationChannel>(channel, out var channelEnum))
                query = query.Where(t => t.Channel == channelEnum);

            if (companyId.HasValue)
                query = query.Where(t => t.CompanyId == null || t.CompanyId == companyId.Value);

            var templates = await query.OrderBy(t => t.Category).ThenBy(t => t.Name).ToListAsync();
            return Ok(templates);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving communication templates");
            return StatusCode(500, new { message = "Failed to retrieve communication templates" });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var template = await _context.CommunicationTemplates.FindAsync(id);
            if (template == null || template.IsDeleted)
                return NotFound(new { message = "Template not found" });

            return Ok(template);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving template {Id}", id);
            return StatusCode(500, new { message = "Failed to retrieve template" });
        }
    }

    [HttpGet("by-code/{code}")]
    public async Task<IActionResult> GetByCode(string code, [FromQuery] Guid? companyId = null)
    {
        try
        {
            var query = _context.CommunicationTemplates.Where(t => t.Code == code && !t.IsDeleted);
            if (companyId.HasValue)
                query = query.Where(t => t.CompanyId == null || t.CompanyId == companyId.Value)
                    .OrderByDescending(t => t.CompanyId);

            var template = await query.FirstOrDefaultAsync();
            if (template == null)
                return NotFound(new { message = $"Template with code '{code}' not found" });

            return Ok(template);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving template by code {Code}", code);
            return StatusCode(500, new { message = "Failed to retrieve template" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CommunicationTemplate template)
    {
        try
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var existingTemplate = await _context.CommunicationTemplates
                .Where(t => t.Code == template.Code && !t.IsDeleted).FirstOrDefaultAsync();
            if (existingTemplate != null)
                return BadRequest(new { message = $"Template with code '{template.Code}' already exists" });

            template.Id = Guid.NewGuid();
            template.CreatedAt = DateTime.UtcNow;
            _context.CommunicationTemplates.Add(template);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = template.Id }, template);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating communication template");
            return StatusCode(500, new { message = "Failed to create communication template" });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CommunicationTemplate template)
    {
        try
        {
            if (id != template.Id) return BadRequest(new { message = "ID mismatch" });
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var existing = await _context.CommunicationTemplates.FindAsync(id);
            if (existing == null || existing.IsDeleted)
                return NotFound(new { message = "Template not found" });

            if (existing.IsSystem)
                return BadRequest(new { message = "Cannot modify system templates" });

            existing.Name = template.Name;
            existing.Description = template.Description;
            existing.Subject = template.Subject;
            existing.Body = template.Body;
            existing.HtmlBody = template.HtmlBody;
            existing.Channel = template.Channel;
            // Only update Category, Language, and AvailablePlaceholders if they are provided
            if (template.Category != null) existing.Category = template.Category;
            if (template.Language != null) existing.Language = template.Language;
            if (template.AvailablePlaceholders != null) existing.AvailablePlaceholders = template.AvailablePlaceholders;
            existing.IsActive = template.IsActive;
            existing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(existing);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating communication template {Id}", id);
            return StatusCode(500, new { message = "Failed to update communication template" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var template = await _context.CommunicationTemplates.FindAsync(id);
            if (template == null || template.IsDeleted)
                return NotFound(new { message = "Template not found" });

            if (template.IsSystem)
                return BadRequest(new { message = "Cannot delete system templates" });

            template.IsDeleted = true;
            template.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Template deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting communication template {Id}", id);
            return StatusCode(500, new { message = "Failed to delete communication template" });
        }
    }

    [HttpPost("validate")]
    public IActionResult ValidateTemplate([FromBody] ValidateTemplateRequest request)
    {
        try
        {
            var result = _templateService.ValidateTemplate(request.TemplateContent);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating template");
            return StatusCode(500, new { message = "Failed to validate template" });
        }
    }

    [HttpPost("extract-placeholders")]
    public IActionResult ExtractPlaceholders([FromBody] ValidateTemplateRequest request)
    {
        try
        {
            var placeholders = _templateService.ExtractPlaceholders(request.TemplateContent);
            return Ok(new { placeholders });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting placeholders");
            return StatusCode(500, new { message = "Failed to extract placeholders" });
        }
    }
}

public class ValidateTemplateRequest
{
    public string TemplateContent { get; set; } = string.Empty;
}
