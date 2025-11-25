using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.Comments;
using ComplaintManagement.Application.Features.Comments.Commands;
using ComplaintManagement.Application.Features.Comments.Queries;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ComplaintManagement.API.Controllers;

[ApiController]
[Route("api/complaints/{complaintId}/comments")]
[Authorize]
public class CommentsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<CommentsController> _logger;

    public CommentsController(IMediator mediator, ILogger<CommentsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all comments for a complaint
    /// </summary>
    /// <param name="complaintId">Complaint ID</param>
    /// <param name="includeInternal">Include internal comments (default: true)</param>
    /// <returns>List of comments</returns>
    [HttpGet]
    [ProducesResponseType(typeof(Result<List<CommentDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetComments(Guid complaintId, [FromQuery] bool includeInternal = true)
    {
        try
        {
            var query = new GetComplaintCommentsQuery
            {
                ComplaintId = complaintId,
                IncludeInternal = includeInternal
            };

            var result = await _mediator.Send(query);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving comments for complaint {ComplaintId}", complaintId);
            return StatusCode(500, new { message = "An error occurred while retrieving comments" });
        }
    }

    /// <summary>
    /// Add a comment to a complaint
    /// </summary>
    /// <param name="complaintId">Complaint ID</param>
    /// <param name="request">Comment creation request</param>
    /// <returns>Created comment</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Result<CommentDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AddComment(Guid complaintId, [FromBody] CreateCommentRequest request)
    {
        try
        {
            // Get current user ID from claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized(new { message = "User information not found" });
            }

            var command = new AddCommentCommand
            {
                ComplaintId = complaintId,
                UserId = Guid.Parse(userIdClaim),
                Comment = request.Comment,
                IsInternal = request.IsInternal
            };

            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            _logger.LogInformation("Comment added to complaint {ComplaintId} by user {UserId}", complaintId, userIdClaim);
            return CreatedAtAction(nameof(GetComments), new { complaintId }, result);
        }
        catch (ValidationException vex)
        {
            _logger.LogWarning(vex, "Validation failed while adding comment to complaint {ComplaintId}", complaintId);
            var errors = vex.Errors.Select(e => new { field = e.PropertyName, message = e.ErrorMessage });
            return BadRequest(new { message = "Validation failed", errors });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while adding comment to complaint {ComplaintId}", complaintId);
            return StatusCode(500, new { message = "An error occurred while adding the comment" });
        }
    }
}
