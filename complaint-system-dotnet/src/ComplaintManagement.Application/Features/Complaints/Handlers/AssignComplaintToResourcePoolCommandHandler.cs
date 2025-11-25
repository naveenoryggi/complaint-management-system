using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.Assignment;
using ComplaintManagement.Application.DTOs.Complaints;
using ComplaintManagement.Application.Features.Complaints.Commands;
using ComplaintManagement.Application.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ComplaintManagement.Application.Features.Complaints.Handlers;

/// <summary>
/// Handler for assigning complaints to resource pools
/// </summary>
public class AssignComplaintToResourcePoolCommandHandler : IRequestHandler<AssignComplaintToResourcePoolCommand, Result<ComplaintDto>>
{
    private readonly IAssignmentEngine _assignmentEngine;
    private readonly ILogger<AssignComplaintToResourcePoolCommandHandler> _logger;

    public AssignComplaintToResourcePoolCommandHandler(
        IAssignmentEngine assignmentEngine,
        ILogger<AssignComplaintToResourcePoolCommandHandler> logger)
    {
        _assignmentEngine = assignmentEngine;
        _logger = logger;
    }

    public async Task<Result<ComplaintDto>> Handle(AssignComplaintToResourcePoolCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Handling pool assignment request for complaint {ComplaintId} to pool {PoolId}",
                request.ComplaintId, request.ResourcePoolId);

            // Prepare assignment context
            var context = request.Context;
            context.AssignedByUserId = request.AssignedById;
            context.AssignmentReason = "Manual Pool Assignment";

            // Validate assignment if requested
            if (request.ValidateBeforeAssignment)
            {
                var validationResult = await _assignmentEngine.ValidateAssignmentAsync(
                    request.ComplaintId,
                    request.SpecificUserId,
                    request.ResourcePoolId,
                    context,
                    cancellationToken);

                if (!validationResult.IsValid)
                {
                    var errorMessage = $"Assignment validation failed: {string.Join(", ", validationResult.ValidationErrors)}";
                    _logger.LogWarning("Pool assignment validation failed: {Errors}", errorMessage);
                    return Result<ComplaintDto>.Failure(errorMessage, "VALIDATION_FAILED");
                }

                // Log warnings
                if (validationResult.ValidationWarnings.Any())
                {
                    _logger.LogWarning("Pool assignment warnings: {Warnings}",
                        string.Join(", ", validationResult.ValidationWarnings));
                }
            }

            // Perform the assignment
            var assignmentResult = await _assignmentEngine.AssignComplaintToPoolAsync(
                request.ComplaintId,
                request.ResourcePoolId,
                request.AssignmentMethod,
                request.SpecificUserId,
                context,
                cancellationToken);

            if (!assignmentResult.Success)
            {
                _logger.LogError("Pool assignment failed: {Error}", assignmentResult.Message);
                return Result<ComplaintDto>.Failure(assignmentResult.Message, assignmentResult.ErrorCode);
            }

            _logger.LogInformation("Successfully assigned complaint {ComplaintId} to user {UserId} via pool {PoolId}",
                request.ComplaintId, assignmentResult.AssignedUserId, request.ResourcePoolId);

            // Return updated complaint details
            // Note: In a real implementation, we would fetch and return the updated complaint DTO
            return Result<ComplaintDto>.Success(new ComplaintDto
            {
                Id = request.ComplaintId,
                AssignedToId = assignmentResult.AssignedUserId,
                AssignedToName = assignmentResult.AssignedUserName
            }, assignmentResult.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling pool assignment for complaint {ComplaintId}", request.ComplaintId);
            return Result<ComplaintDto>.Failure($"Pool assignment error: {ex.Message}", "ASSIGNMENT_ERROR");
        }
    }
}

/// <summary>
/// Handler for auto-assigning complaints
/// </summary>
public class AutoAssignComplaintCommandHandler : IRequestHandler<AutoAssignComplaintCommand, Result<ComplaintDto>>
{
    private readonly IAssignmentEngine _assignmentEngine;
    private readonly ILogger<AutoAssignComplaintCommandHandler> _logger;

    public AutoAssignComplaintCommandHandler(
        IAssignmentEngine assignmentEngine,
        ILogger<AutoAssignComplaintCommandHandler> logger)
    {
        _assignmentEngine = assignmentEngine;
        _logger = logger;
    }

    public async Task<Result<ComplaintDto>> Handle(AutoAssignComplaintCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Handling auto-assignment request for complaint {ComplaintId}", request.ComplaintId);

            // Prepare assignment context
            var context = request.Context;
            context.AssignedByUserId = request.TriggeredById;
            context.AssignmentReason = "Auto Assignment";
            context.IsInitialAssignment = true;

            // Perform auto-assignment
            var assignmentResult = await _assignmentEngine.AutoAssignComplaintAsync(
                request.ComplaintId,
                context,
                cancellationToken);

            if (!assignmentResult.Success)
            {
                if (!request.ForceAssignment)
                {
                    _logger.LogError("Auto-assignment failed: {Error}", assignmentResult.Message);
                    return Result<ComplaintDto>.Failure(assignmentResult.Message, assignmentResult.ErrorCode);
                }

                _logger.LogWarning("Auto-assignment failed but forcing assignment: {Error}", assignmentResult.Message);
                // Implement fallback logic for forced assignment
            }

            _logger.LogInformation("Successfully auto-assigned complaint {ComplaintId} to user {UserId}",
                request.ComplaintId, assignmentResult.AssignedUserId);

            // Return updated complaint details
            return Result<ComplaintDto>.Success(new ComplaintDto
            {
                Id = request.ComplaintId,
                AssignedToId = assignmentResult.AssignedUserId,
                AssignedToName = assignmentResult.AssignedUserName
            }, assignmentResult.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling auto-assignment for complaint {ComplaintId}", request.ComplaintId);
            return Result<ComplaintDto>.Failure($"Auto-assignment error: {ex.Message}", "AUTO_ASSIGNMENT_ERROR");
        }
    }
}

/// <summary>
/// Handler for reassigning complaints
/// </summary>
public class ReassignComplaintCommandHandler : IRequestHandler<ReassignComplaintCommand, Result<ComplaintDto>>
{
    private readonly IAssignmentEngine _assignmentEngine;
    private readonly ILogger<ReassignComplaintCommandHandler> _logger;

    public ReassignComplaintCommandHandler(
        IAssignmentEngine assignmentEngine,
        ILogger<ReassignComplaintCommandHandler> logger)
    {
        _assignmentEngine = assignmentEngine;
        _logger = logger;
    }

    public async Task<Result<ComplaintDto>> Handle(ReassignComplaintCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Handling reassignment request for complaint {ComplaintId} from user {CurrentUserId} to new assignment",
                request.ComplaintId, request.CurrentAssignedUserId);

            // Prepare assignment context
            var context = request.Context;
            context.AssignedByUserId = request.ReassignedById;
            context.AssignmentReason = $"Reassignment: {request.ReassignmentReason}";
            context.IsInitialAssignment = false;

            AssignmentResult assignmentResult;

            if (request.NewAssignedUserId.HasValue)
            {
                // Direct user reassignment
                assignmentResult = await _assignmentEngine.AssignComplaintToPoolAsync(
                    request.ComplaintId,
                    request.NewResourcePoolId ?? Guid.Empty, // Would need to find user's pool
                    request.AssignmentMethod,
                    request.NewAssignedUserId,
                    context,
                    cancellationToken);
            }
            else if (request.NewResourcePoolId.HasValue)
            {
                // Pool-based reassignment
                assignmentResult = await _assignmentEngine.AssignComplaintToPoolAsync(
                    request.ComplaintId,
                    request.NewResourcePoolId.Value,
                    request.AssignmentMethod,
                    null,
                    context,
                    cancellationToken);
            }
            else
            {
                return Result<ComplaintDto>.Failure("Either new user ID or resource pool ID must be specified", "INVALID_REASSIGNMENT_REQUEST");
            }

            if (!assignmentResult.Success)
            {
                _logger.LogError("Reassignment failed: {Error}", assignmentResult.Message);
                return Result<ComplaintDto>.Failure(assignmentResult.Message, assignmentResult.ErrorCode);
            }

            _logger.LogInformation("Successfully reassigned complaint {ComplaintId} to user {UserId}",
                request.ComplaintId, assignmentResult.AssignedUserId);

            // Return updated complaint details
            return Result<ComplaintDto>.Success(new ComplaintDto
            {
                Id = request.ComplaintId,
                AssignedToId = assignmentResult.AssignedUserId,
                AssignedToName = assignmentResult.AssignedUserName
            }, $"Complaint reassigned: {request.ReassignmentReason}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling reassignment for complaint {ComplaintId}", request.ComplaintId);
            return Result<ComplaintDto>.Failure($"Reassignment error: {ex.Message}", "REASSIGNMENT_ERROR");
        }
    }
}

/// <summary>
/// Handler for getting assignment candidates
/// </summary>
public class GetAssignmentCandidatesQueryHandler : IRequestHandler<GetAssignmentCandidatesQuery, Result<List<ResourcePoolCandidate>>>
{
    private readonly IAssignmentEngine _assignmentEngine;
    private readonly ILogger<GetAssignmentCandidatesQueryHandler> _logger;

    public GetAssignmentCandidatesQueryHandler(
        IAssignmentEngine assignmentEngine,
        ILogger<GetAssignmentCandidatesQueryHandler> logger)
    {
        _assignmentEngine = assignmentEngine;
        _logger = logger;
    }

    public async Task<Result<List<ResourcePoolCandidate>>> Handle(GetAssignmentCandidatesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting assignment candidates for complaint {ComplaintId}", request.ComplaintId);

            var candidates = await _assignmentEngine.GetAssignmentCandidatesAsync(
                request.ComplaintId,
                request.Criteria,
                request.IncludeUserDetails,
                request.CalculateSkillScores,
                cancellationToken);

            // Limit results to requested maximum
            var limitedCandidates = candidates.Take(request.MaxCandidates).ToList();

            _logger.LogInformation("Found {Count} assignment candidates for complaint {ComplaintId}",
                limitedCandidates.Count, request.ComplaintId);

            return Result<List<ResourcePoolCandidate>>.Success(limitedCandidates, $"Found {limitedCandidates.Count} candidates");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting assignment candidates for complaint {ComplaintId}", request.ComplaintId);
            return Result<List<ResourcePoolCandidate>>.Failure($"Error getting candidates: {ex.Message}", "CANDIDATES_ERROR");
        }
    }
}

/// <summary>
/// Handler for validating assignments
/// </summary>
public class ValidateAssignmentQueryHandler : IRequestHandler<ValidateAssignmentQuery, Result<AssignmentValidationResult>>
{
    private readonly IAssignmentEngine _assignmentEngine;
    private readonly ILogger<ValidateAssignmentQueryHandler> _logger;

    public ValidateAssignmentQueryHandler(
        IAssignmentEngine assignmentEngine,
        ILogger<ValidateAssignmentQueryHandler> logger)
    {
        _assignmentEngine = assignmentEngine;
        _logger = logger;
    }

    public async Task<Result<AssignmentValidationResult>> Handle(ValidateAssignmentQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Validating assignment for complaint {ComplaintId}", request.ComplaintId);

            var validationResult = await _assignmentEngine.ValidateAssignmentAsync(
                request.ComplaintId,
                request.UserId,
                request.ResourcePoolId,
                request.Context,
                cancellationToken);

            var message = validationResult.IsValid
                ? "Assignment is valid"
                : $"Assignment validation failed: {string.Join(", ", validationResult.ValidationErrors)}";

            _logger.LogInformation("Assignment validation completed for complaint {ComplaintId}. Valid: {IsValid}",
                request.ComplaintId, validationResult.IsValid);

            return Result<AssignmentValidationResult>.Success(validationResult, message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating assignment for complaint {ComplaintId}", request.ComplaintId);
            return Result<AssignmentValidationResult>.Failure($"Validation error: {ex.Message}", "VALIDATION_ERROR");
        }
    }
}