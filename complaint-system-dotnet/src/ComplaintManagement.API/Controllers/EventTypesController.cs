using ComplaintManagement.Domain.Entities.Events;
using ComplaintManagement.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ComplaintManagement.API.Controllers;

[ApiController]
[Route("api/event-types")]
[Authorize]
public class EventTypesController : ControllerBase
{
    private readonly ComplaintDbContext _context;
    private readonly ILogger<EventTypesController> _logger;

    public EventTypesController(ComplaintDbContext context, ILogger<EventTypesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    [ResponseCache(Duration = 600, VaryByQueryKeys = new[] { "includeInactive", "entityType", "category", "companyId" }, VaryByHeader = "Authorization")]
    public async Task<IActionResult> GetAll(
        [FromQuery] bool includeInactive = false,
        [FromQuery] string? entityType = null,
        [FromQuery] string? category = null,
        [FromQuery] Guid? companyId = null)
    {
        try
        {
            var query = _context.EventTypes.AsQueryable();
            if (!includeInactive) query = query.Where(e => e.IsActive);
            if (!string.IsNullOrEmpty(entityType)) query = query.Where(e => e.EntityType == entityType);
            if (!string.IsNullOrEmpty(category)) query = query.Where(e => e.Category == category);
            if (companyId.HasValue) query = query.Where(e => e.CompanyId == null || e.CompanyId == companyId.Value);

            var eventTypes = await query.OrderBy(e => e.Category).ThenBy(e => e.Name).ToListAsync();
            return Ok(eventTypes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving event types");
            return StatusCode(500, new { message = "Failed to retrieve event types" });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var eventType = await _context.EventTypes.FindAsync(id);
            if (eventType == null || eventType.IsDeleted)
                return NotFound(new { message = "Event type not found" });

            return Ok(eventType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving event type {Id}", id);
            return StatusCode(500, new { message = "Failed to retrieve event type" });
        }
    }

    [HttpGet("by-code/{code}")]
    public async Task<IActionResult> GetByCode(string code, [FromQuery] Guid? companyId = null)
    {
        try
        {
            var query = _context.EventTypes.Where(e => e.Code == code && !e.IsDeleted);
            if (companyId.HasValue)
                query = query.Where(e => e.CompanyId == null || e.CompanyId == companyId.Value)
                    .OrderByDescending(e => e.CompanyId);

            var eventType = await query.FirstOrDefaultAsync();
            if (eventType == null)
                return NotFound(new { message = $"Event type with code '{code}' not found" });

            return Ok(eventType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving event type by code {Code}", code);
            return StatusCode(500, new { message = "Failed to retrieve event type" });
        }
    }

    [HttpGet("{id}/rules")]
    public async Task<IActionResult> GetRulesByEventType(Guid id, [FromQuery] bool includeInactive = false)
    {
        try
        {
            var eventType = await _context.EventTypes.FindAsync(id);
            if (eventType == null || eventType.IsDeleted)
                return NotFound(new { message = "Event type not found" });

            var query = _context.EventCommunicationRules.Include(r => r.Template)
                .Where(r => r.EventTypeId == id && !r.IsDeleted);
            if (!includeInactive) query = query.Where(r => r.IsActive);

            var rules = await query.OrderBy(r => r.Priority).ToListAsync();
            return Ok(rules);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving rules for event type {Id}", id);
            return StatusCode(500, new { message = "Failed to retrieve event type rules" });
        }
    }

    [HttpGet("entity-types")]
    public async Task<IActionResult> GetEntityTypes()
    {
        try
        {
            var entityTypes = await _context.EventTypes.Where(e => !e.IsDeleted && e.IsActive)
                .Select(e => e.EntityType).Distinct().OrderBy(e => e).ToListAsync();
            return Ok(new { entityTypes });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving entity types");
            return StatusCode(500, new { message = "Failed to retrieve entity types" });
        }
    }

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
    {
        try
        {
            var categories = await _context.EventTypes.Where(e => !e.IsDeleted && e.IsActive && e.Category != null)
                .Select(e => e.Category).Distinct().OrderBy(c => c).ToListAsync();
            return Ok(new { categories });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving event categories");
            return StatusCode(500, new { message = "Failed to retrieve event categories" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] EventType eventType)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Validate required fields
            if (string.IsNullOrEmpty(eventType.Name))
            {
                return BadRequest(new { message = "Name is required" });
            }

            if (string.IsNullOrEmpty(eventType.Code))
            {
                return BadRequest(new { message = "Code is required" });
            }

            if (string.IsNullOrEmpty(eventType.EntityType))
            {
                return BadRequest(new { message = "EntityType is required" });
            }

            eventType.Id = Guid.NewGuid();
            eventType.CreatedAt = DateTime.UtcNow;
            eventType.IsDeleted = false;

            _context.EventTypes.Add(eventType);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = eventType.Id }, eventType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating event type");
            return StatusCode(500, new { message = "Failed to create event type" });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] EventType eventType)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existing = await _context.EventTypes.FindAsync(id);
            if (existing == null || existing.IsDeleted)
            {
                return NotFound(new { message = "Event type not found" });
            }

            // Prevent editing system event types (optional - remove if not needed)
            // if (existing.IsSystem)
            // {
            //     return BadRequest(new { message = "Cannot modify system event types" });
            // }

            // Validate required fields
            if (string.IsNullOrEmpty(eventType.Name))
            {
                return BadRequest(new { message = "Name is required" });
            }

            if (string.IsNullOrEmpty(eventType.Code))
            {
                return BadRequest(new { message = "Code is required" });
            }

            if (string.IsNullOrEmpty(eventType.EntityType))
            {
                return BadRequest(new { message = "EntityType is required" });
            }

            // Update properties
            existing.Name = eventType.Name;
            existing.Code = eventType.Code;
            existing.Description = eventType.Description;
            existing.EntityType = eventType.EntityType;
            existing.Category = eventType.Category;
            existing.IsActive = eventType.IsActive;
            existing.AvailableFields = eventType.AvailableFields;
            existing.IconClass = eventType.IconClass;
            existing.UpdatedAt = DateTime.UtcNow;

            _context.EventTypes.Update(existing);
            await _context.SaveChangesAsync();

            return Ok(existing);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating event type {Id}", id);
            return StatusCode(500, new { message = "Failed to update event type" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var eventType = await _context.EventTypes.FindAsync(id);
            if (eventType == null || eventType.IsDeleted)
            {
                return NotFound(new { message = "Event type not found" });
            }

            // Prevent deleting system event types
            if (eventType.IsSystem)
            {
                return BadRequest(new { message = "Cannot delete system event types" });
            }

            // Soft delete
            eventType.IsDeleted = true;
            eventType.UpdatedAt = DateTime.UtcNow;

            _context.EventTypes.Update(eventType);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting event type {Id}", id);
            return StatusCode(500, new { message = "Failed to delete event type" });
        }
    }
}
