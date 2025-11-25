using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.Settings;
using ComplaintManagement.Application.Features.Settings.Commands;
using ComplaintManagement.Application.Interfaces.Repositories;
using MediatR;

namespace ComplaintManagement.Application.Features.Settings.Handlers;

/// <summary>
/// Handler for updating complaint information settings
/// </summary>
public class UpdateComplaintInfoSettingsCommandHandler : IRequestHandler<UpdateComplaintInfoSettingsCommand, Result<ComplaintInformationSettingsDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateComplaintInfoSettingsCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ComplaintInformationSettingsDto>> Handle(UpdateComplaintInfoSettingsCommand request, CancellationToken cancellationToken)
    {
        // Get existing settings
        var settings = await _unitOfWork.ComplaintInfoSettings.GetByCompanyIdAsync(request.CompanyId, cancellationToken);

        if (settings == null)
        {
            return Result<ComplaintInformationSettingsDto>.Failure("Settings not found", "No settings found for the specified company");
        }

        // Update settings
        settings.ShowEmployeeCodeToHandlers = request.ShowEmployeeCodeToHandlers;
        settings.ShowEmailToHandlers = request.ShowEmailToHandlers;
        settings.ShowPhoneToHandlers = request.ShowPhoneToHandlers;
        settings.ShowBranchToHandlers = request.ShowBranchToHandlers;
        settings.ShowDepartmentToHandlers = request.ShowDepartmentToHandlers;
        settings.ShowSectionToHandlers = request.ShowSectionToHandlers;
        settings.ShowJobTitleToHandlers = request.ShowJobTitleToHandlers;
        settings.ShowManagerDetailsToHandlers = request.ShowManagerDetailsToHandlers;
        settings.ShowPreviousComplaintsToHandlers = request.ShowPreviousComplaintsToHandlers;
        settings.ShowEmployeeAddressToManagement = request.ShowEmployeeAddressToManagement;
        settings.ShowEmergencyContactToManagement = request.ShowEmergencyContactToManagement;
        settings.ShowPerformanceMetricsToManagement = request.ShowPerformanceMetricsToManagement;
        settings.MaskPersonalInfoInLogs = request.MaskPersonalInfoInLogs;
        settings.RedactInfoAfterClosure = request.RedactInfoAfterClosure;
        settings.DataRetentionDays = request.DataRetentionDays;
        settings.IncludeEmployeeCodeInReports = request.IncludeEmployeeCodeInReports;
        settings.IncludeEmailInReports = request.IncludeEmailInReports;
        settings.IncludePhoneInReports = request.IncludePhoneInReports;
        settings.MaskEmailInReports = request.MaskEmailInReports;
        settings.MaskPhoneInReports = request.MaskPhoneInReports;
        settings.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.ComplaintInfoSettings.Update(settings);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Reload with company details
        var updatedSettings = await _unitOfWork.ComplaintInfoSettings.GetByCompanyIdWithDetailsAsync(request.CompanyId, cancellationToken);

        if (updatedSettings == null)
        {
            return Result<ComplaintInformationSettingsDto>.Failure("Failed to retrieve updated settings", "Database error");
        }

        var dto = new ComplaintInformationSettingsDto
        {
            Id = updatedSettings.Id,
            CompanyId = updatedSettings.CompanyId,
            CompanyName = updatedSettings.Company?.Name ?? string.Empty,
            ShowEmployeeCodeToHandlers = updatedSettings.ShowEmployeeCodeToHandlers,
            ShowEmailToHandlers = updatedSettings.ShowEmailToHandlers,
            ShowPhoneToHandlers = updatedSettings.ShowPhoneToHandlers,
            ShowBranchToHandlers = updatedSettings.ShowBranchToHandlers,
            ShowDepartmentToHandlers = updatedSettings.ShowDepartmentToHandlers,
            ShowSectionToHandlers = updatedSettings.ShowSectionToHandlers,
            ShowJobTitleToHandlers = updatedSettings.ShowJobTitleToHandlers,
            ShowManagerDetailsToHandlers = updatedSettings.ShowManagerDetailsToHandlers,
            ShowPreviousComplaintsToHandlers = updatedSettings.ShowPreviousComplaintsToHandlers,
            ShowEmployeeAddressToManagement = updatedSettings.ShowEmployeeAddressToManagement,
            ShowEmergencyContactToManagement = updatedSettings.ShowEmergencyContactToManagement,
            ShowPerformanceMetricsToManagement = updatedSettings.ShowPerformanceMetricsToManagement,
            MaskPersonalInfoInLogs = updatedSettings.MaskPersonalInfoInLogs,
            RedactInfoAfterClosure = updatedSettings.RedactInfoAfterClosure,
            DataRetentionDays = updatedSettings.DataRetentionDays,
            IncludeEmployeeCodeInReports = updatedSettings.IncludeEmployeeCodeInReports,
            IncludeEmailInReports = updatedSettings.IncludeEmailInReports,
            IncludePhoneInReports = updatedSettings.IncludePhoneInReports,
            MaskEmailInReports = updatedSettings.MaskEmailInReports,
            MaskPhoneInReports = updatedSettings.MaskPhoneInReports
        };

        return Result<ComplaintInformationSettingsDto>.Success(dto, "Settings updated successfully");
    }
}
