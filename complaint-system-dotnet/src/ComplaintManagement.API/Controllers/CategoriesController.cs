using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.Categories;
using ComplaintManagement.Application.Features.Categories.Commands;
using ComplaintManagement.Application.Features.Categories.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ComplaintManagement.API.Controllers;

[ApiController]
[Route("api/categories")]
[Authorize]
public class CategoriesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<CategoriesController> _logger;

    public CategoriesController(IMediator mediator, ILogger<CategoriesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all complaint categories
    /// </summary>
    /// <param name="activeOnly">Only return active categories (default: true)</param>
    /// <returns>List of categories</returns>
    [HttpGet]
    [ResponseCache(Duration = 300, VaryByQueryKeys = new[] { "activeOnly" }, VaryByHeader = "Authorization")]
    [ProducesResponseType(typeof(Result<List<CategoryDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCategories([FromQuery] bool activeOnly = true)
    {
        try
        {
            var query = new GetCategoriesQuery { ActiveOnly = activeOnly };
            var result = await _mediator.Send(query);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving categories");
            return StatusCode(500, new { message = "An error occurred while retrieving categories" });
        }
    }

    /// <summary>
    /// Create a new complaint category
    /// </summary>
    /// <param name="dto">Category creation data</param>
    /// <returns>Created category</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Result<CategoryDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var command = new CreateCategoryCommand
            {
                Name = dto.Name,
                Code = dto.Code,
                Description = dto.Description,
                ParentCategoryId = dto.ParentCategoryId,
                DefaultPriority = dto.DefaultPriority,
                DefaultSlaHours = dto.DefaultSlaHours,
                IsActive = dto.IsActive,
                DisplayOrder = dto.DisplayOrder
            };

            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return CreatedAtAction(nameof(GetCategories), new { }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating category");
            return StatusCode(500, new { message = "An error occurred while creating category" });
        }
    }

    /// <summary>
    /// Update an existing complaint category
    /// </summary>
    /// <param name="id">Category ID</param>
    /// <param name="dto">Category update data</param>
    /// <returns>Updated category</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(Result<CategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] UpdateCategoryDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != dto.Id)
            {
                return BadRequest(Result<CategoryDto>.Failure("Category ID mismatch", "Invalid ID"));
            }

            var command = new UpdateCategoryCommand
            {
                Id = dto.Id,
                Name = dto.Name,
                Code = dto.Code,
                Description = dto.Description,
                ParentCategoryId = dto.ParentCategoryId,
                DefaultPriority = dto.DefaultPriority,
                DefaultSlaHours = dto.DefaultSlaHours,
                IsActive = dto.IsActive,
                DisplayOrder = dto.DisplayOrder
            };

            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating category {Id}", id);
            return StatusCode(500, new { message = "An error occurred while updating category" });
        }
    }

    /// <summary>
    /// Delete a complaint category
    /// </summary>
    /// <param name="id">Category ID</param>
    /// <returns>Success status</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteCategory(Guid id)
    {
        try
        {
            var command = new DeleteCategoryCommand(id);
            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting category {Id}", id);
            return StatusCode(500, new { message = "An error occurred while deleting category" });
        }
    }
}
