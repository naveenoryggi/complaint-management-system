using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.MasterData;
using MediatR;

namespace ComplaintManagement.Application.Features.MasterData.Queries;

public class GetComplaintPriorityMastersQuery : IRequest<Result<List<ComplaintPriorityMasterDto>>>
{
    public Guid? CompanyId { get; set; }
    public bool? IsActive { get; set; }
    public bool IncludeSystem { get; set; } = true;
}
