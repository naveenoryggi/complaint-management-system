using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.Settings;
using ComplaintManagement.Application.Features.Settings.Queries;
using ComplaintManagement.Application.Interfaces.Repositories;
using ComplaintManagement.Domain.Entities.Settings;
using MediatR;

namespace ComplaintManagement.Application.Features.Settings.Handlers;

/// <summary>
/// Handler for getting complaint information settings
/// </summary>
public class GetComplaintInfoSettingsQueryHandler : IRequestHandler<GetComplaintInfoSettingsQuery, Result<ComplaintInformationSettingsDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetComplaintInfoSettingsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ComplaintInformationSettingsDto>> Handle(GetComplaintInfoSettingsQuery request, CancellationToken cancellationToken)
    {
        // Get existing settings or create default
        var settings = await _unitOfWork.ComplaintInfoSettings.GetByCompanyIdWithDetailsAsync(request.CompanyId, cancellationToken);

        if (settings == null)
        {
            // Create default settings for the company
            settings = new ComplaintInformationSettings
            {
                Id = Guid.NewGuid(),
                CompanyId = request.CompanyId,
                // Handler visibility - default to true (show all)
                ShowEmployeeCodeToHandlers = true,
                ShowEmailToHandlers = true,
                ShowPhoneToHandlers = true,
                ShowBranchToHandlers = true,
                ShowDepartmentToHandlers = true,
                ShowSectionToHandlers = true,
                ShowJobTitleToHandlers = true,
                ShowManagerDetailsToHandlers = true,
                ShowPreviousComplaintsToHandlers = true,
                // Management visibility - default to false (privacy)
                ShowEmployeeAddressToManagement = false,
                ShowEmergencyContactToManagement = false,
                ShowPerformanceMetricsToManagement = false,
                // Privacy settings - default to secure
                MaskPersonalInfoInLogs = true,
                RedactInfoAfterClosure = false,
                DataRetentionDays = 0, // 0 means keep forever
                // Report settings - default to include but mask
                IncludeEmployeeCodeInReports = true,
                IncludeEmailInReports = true,
                IncludePhoneInReports = true,
                MaskEmailInReports = false,
                MaskPhoneInReports = false,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.ComplaintInfoSettings.AddAsync(settings, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Reload with company details
            settings = await _unitOfWork.ComplaintInfoSettings.GetByCompanyIdWithDetailsAsync(request.CompanyId, cancellationToken);
        }

        if (settings == null)
        {
            return Result<ComplaintInformationSettingsDto>.Failure("Failed to retrieve settings", "Settings not found");
        }

        var dto = new ComplaintInformationSettingsDto
        {
            Id = settings.Id,
            CompanyId = settings.CompanyId,
            CompanyName = settings.Company?.Name ?? string.Empty,
            ShowEmployeeCodeToHandlers = settings.ShowEmployeeCodeToHandlers,
            ShowEmailToHandlers = settings.ShowEmailToHandlers,
            ShowPhoneToHandlers = settings.ShowPhoneToHandlers,
            ShowBranchToHandlers = settings.ShowBranchToHandlers,
            ShowDepartmentToHandlers = settings.ShowDepartmentToHandlers,
            ShowSectionToHandlers = settings.ShowSectionToHandlers,
            ShowJobTitleToHandlers = settings.ShowJobTitleToHandlers,
            ShowManagerDetailsToHandlers = settings.ShowManagerDetailsToHandlers,
            ShowPreviousComplaintsToHandlers = settings.ShowPreviousComplaintsToHandlers,
            ShowEmployeeAddressToManagement = settings.ShowEmployeeAddressToManagement,
            ShowEmergencyContactToManagement = settings.ShowEmergencyContactToManagement,
            ShowPerformanceMetricsToManagement = settings.ShowPerformanceMetricsToManagement,
            MaskPersonalInfoInLogs = settings.MaskPersonalInfoInLogs,
            RedactInfoAfterClosure = settings.RedactInfoAfterClosure,
            DataRetentionDays = settings.DataRetentionDays,
            IncludeEmployeeCodeInReports = settings.IncludeEmployeeCodeInReports,
            IncludeEmailInReports = settings.IncludeEmailInReports,
            IncludePhoneInReports = settings.IncludePhoneInReports,
            MaskEmailInReports = settings.MaskEmailInReports,
            MaskPhoneInReports = settings.MaskPhoneInReports
        };

        return Result<ComplaintInformationSettingsDto>.Success(dto, "Settings retrieved successfully");
    }
}
