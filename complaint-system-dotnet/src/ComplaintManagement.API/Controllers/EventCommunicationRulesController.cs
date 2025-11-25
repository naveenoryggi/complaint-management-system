using ComplaintManagement.Domain.Entities.Events;
using ComplaintManagement.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ComplaintManagement.API.Controllers;

[ApiController]
[Route("api/event-communication-rules")]
[Authorize]
public class EventCommunicationRulesController : ControllerBase
{
    private readonly ComplaintDbContext _context;
    private readonly ILogger<EventCommunicationRulesController> _logger;

    public EventCommunicationRulesController(ComplaintDbContext context, ILogger<EventCommunicationRulesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? eventTypeId = null,
        [FromQuery] bool includeInactive = false,
        [FromQuery] Guid? companyId = null)
    {
        try
        {
            var query = _context.EventCommunicationRules.Include(r => r.EventType).Include(r => r.Template).AsQueryable();
            if (!includeInactive) query = query.Where(r => r.IsActive);
            if (eventTypeId.HasValue) query = query.Where(r => r.EventTypeId == eventTypeId.Value);
            if (companyId.HasValue) query = query.Where(r => r.CompanyId == null || r.CompanyId == companyId.Value);

            var rules = await query.OrderBy(r => r.EventType.Name).ThenBy(r => r.Priority).ToListAsync();
            return Ok(new { isSuccess = true, data = rules, message = "Event communication rules retrieved successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving event communication rules");
            return StatusCode(500, new { isSuccess = false, message = "Failed to retrieve event communication rules", errors = new[] { ex.Message } });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var rule = await _context.EventCommunicationRules.Include(r => r.EventType).Include(r => r.Template)
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);
            if (rule == null) return NotFound(new { isSuccess = false, message = "Event communication rule not found" });

            return Ok(new { isSuccess = true, data = rule, message = "Event communication rule retrieved successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving event communication rule {Id}", id);
            return StatusCode(500, new { isSuccess = false, message = "Failed to retrieve event communication rule", errors = new[] { ex.Message } });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] EventCommunicationRule rule)
    {
        try
        {
            if (!ModelState.IsValid) return BadRequest(new { isSuccess = false, message = "Invalid input", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToArray() });

            var eventType = await _context.EventTypes.FindAsync(rule.EventTypeId);
            if (eventType == null || eventType.IsDeleted)
                return BadRequest(new { isSuccess = false, message = "Invalid event type" });

            if (rule.TemplateId.HasValue)
            {
                var template = await _context.CommunicationTemplates.FindAsync(rule.TemplateId.Value);
                if (template == null || template.IsDeleted)
                    return BadRequest(new { isSuccess = false, message = "Invalid template" });
            }

            rule.Id = Guid.NewGuid();
            rule.CreatedAt = DateTime.UtcNow;
            _context.EventCommunicationRules.Add(rule);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = rule.Id }, new { isSuccess = true, data = rule, message = "Event communication rule created successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating event communication rule");
            return StatusCode(500, new { isSuccess = false, message = "Failed to create event communication rule", errors = new[] { ex.Message } });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] EventCommunicationRule rule)
    {
        try
        {
            if (id != rule.Id) return BadRequest(new { isSuccess = false, message = "ID mismatch" });
            if (!ModelState.IsValid) return BadRequest(new { isSuccess = false, message = "Invalid input", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToArray() });

            var existing = await _context.EventCommunicationRules.FindAsync(id);
            if (existing == null || existing.IsDeleted)
                return NotFound(new { isSuccess = false, message = "Event communication rule not found" });

            if (rule.TemplateId.HasValue)
            {
                var template = await _context.CommunicationTemplates.FindAsync(rule.TemplateId.Value);
                if (template == null || template.IsDeleted)
                    return BadRequest(new { isSuccess = false, message = "Invalid template" });
            }

            existing.Name = rule.Name;
            existing.Description = rule.Description;
            existing.TemplateId = rule.TemplateId;
            existing.Channel = rule.Channel;
            existing.RecipientType = rule.RecipientType;
            existing.SpecificEmails = rule.SpecificEmails;
            existing.SpecificUserIds = rule.SpecificUserIds;
            existing.SpecificRoleIds = rule.SpecificRoleIds;
            existing.Conditions = rule.Conditions;
            existing.Priority = rule.Priority;
            existing.DelayMinutes = rule.DelayMinutes;
            existing.IsActive = rule.IsActive;
            existing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(new { isSuccess = true, data = existing, message = "Event communication rule updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating event communication rule {Id}", id);
            return StatusCode(500, new { isSuccess = false, message = "Failed to update event communication rule", errors = new[] { ex.Message } });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var rule = await _context.EventCommunicationRules.FindAsync(id);
            if (rule == null || rule.IsDeleted)
                return NotFound(new { isSuccess = false, message = "Event communication rule not found" });

            rule.IsDeleted = true;
            rule.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { isSuccess = true, message = "Event communication rule deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting event communication rule {Id}", id);
            return StatusCode(500, new { isSuccess = false, message = "Failed to delete event communication rule", errors = new[] { ex.Message } });
        }
    }

    [HttpPost("reorder")]
    public async Task<IActionResult> Reorder([FromBody] ReorderRulesRequest request)
    {
        try
        {
            foreach (var item in request.Rules)
            {
                var rule = await _context.EventCommunicationRules.FindAsync(item.Id);
                if (rule != null && !rule.IsDeleted) rule.Priority = item.Priority;
            }
            await _context.SaveChangesAsync();
            return Ok(new { isSuccess = true, message = "Rules reordered successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reordering event communication rules");
            return StatusCode(500, new { isSuccess = false, message = "Failed to reorder rules", errors = new[] { ex.Message } });
        }
    }
}

public class ReorderRulesRequest
{
    public List<RuleOrder> Rules { get; set; } = new();
}

public class RuleOrder
{
    public Guid Id { get; set; }
    public int Priority { get; set; }
}
