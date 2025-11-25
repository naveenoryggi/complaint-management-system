using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.MasterData;
using MediatR;

namespace ComplaintManagement.Application.Features.MasterData.Commands;

public class CreateComplaintPriorityMasterCommand : IRequest<Result<ComplaintPriorityMasterDto>>
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public int Level { get; set; }
    public string? ColorCode { get; set; }
    public string? IconClass { get; set; }
    public int? SlaResponseHours { get; set; }
    public int? SlaResolutionHours { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid? CompanyId { get; set; }
}
