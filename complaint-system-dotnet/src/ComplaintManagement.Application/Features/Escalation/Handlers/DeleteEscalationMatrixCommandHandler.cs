using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.Features.Escalation.Commands;
using ComplaintManagement.Application.Interfaces.Services;
using MediatR;

namespace ComplaintManagement.Application.Features.Escalation.Handlers;

public class DeleteEscalationMatrixCommandHandler : IRequestHandler<DeleteEscalationMatrixCommand, Result>
{
    private readonly IEscalationService _escalationService;

    public DeleteEscalationMatrixCommandHandler(IEscalationService escalationService)
    {
        _escalationService = escalationService;
    }

    public async Task<Result> Handle(DeleteEscalationMatrixCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await _escalationService.DeleteMatrixAsync(request.MatrixId, request.DeletedBy);
            return Result.Success("Escalation matrix deleted successfully");
        }
        catch (KeyNotFoundException)
        {
            return Result.Failure("Escalation matrix not found", "Not found");
        }
        catch (Exception ex)
        {
            return Result.Failure($"Error deleting escalation matrix: {ex.Message}", "Deletion failed");
        }
    }
}
