using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.MasterData;
using MediatR;

namespace ComplaintManagement.Application.Features.MasterData.Commands;

public class CreateComplaintStatusMasterCommand : IRequest<Result<ComplaintStatusMasterDto>>
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public string? ColorCode { get; set; }
    public string? IconClass { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsFinal { get; set; } = false;
    public Guid? CompanyId { get; set; }
}
