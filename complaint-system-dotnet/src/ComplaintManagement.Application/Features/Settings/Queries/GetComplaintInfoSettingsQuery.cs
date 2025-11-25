using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.Settings;
using MediatR;

namespace ComplaintManagement.Application.Features.Settings.Queries;

/// <summary>
/// Query to get complaint information settings for a company
/// </summary>
public class GetComplaintInfoSettingsQuery : IRequest<Result<ComplaintInformationSettingsDto>>
{
    public Guid CompanyId { get; set; }
}
