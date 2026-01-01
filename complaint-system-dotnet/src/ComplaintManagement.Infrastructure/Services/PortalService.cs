using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.Portal;
using ComplaintManagement.Application.Interfaces.Services;
using ComplaintManagement.Domain.Entities.CRM;
using ComplaintManagement.Domain.Enums.CRM;
using ComplaintManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ComplaintManagement.Infrastructure.Services;

/// <summary>
/// Service implementation for customer portal operations
/// </summary>
public class PortalService : IPortalService
{
    private readonly ComplaintDbContext _context;
    private readonly IJwtTokenService _jwtService;
    private readonly IPasswordService _passwordService;
    private readonly ILogger<PortalService> _logger;

    // Lockout settings
    private const int MaxFailedAttempts = 5;
    private const int LockoutMinutes = 15;

    public PortalService(
        ComplaintDbContext context,
        IJwtTokenService jwtService,
        IPasswordService passwordService,
        ILogger<PortalService> logger)
    {
        _context = context;
        _jwtService = jwtService;
        _passwordService = passwordService;
        _logger = logger;
    }

    #region Authentication

    public async Task<PortalLoginResponse> LoginAsync(PortalLoginRequest request, string? ipAddress = null)
    {
        try
        {
            // Find contact by email
            var contact = await _context.CustomerContacts
                .Include(c => c.Customer)
                .FirstOrDefaultAsync(c => c.Email == request.Email && !c.IsDeleted);

            if (contact == null)
            {
                _logger.LogWarning("Portal login failed: Email not found - {Email}", request.Email);
                return new PortalLoginResponse
                {
                    IsSuccess = false,
                    Message = "Invalid email or password"
                };
            }

            // Check if portal access is enabled
            if (!contact.IsPortalUser)
            {
                _logger.LogWarning("Portal login failed: Portal access not enabled for {Email}", request.Email);
                return new PortalLoginResponse
                {
                    IsSuccess = false,
                    Message = "Portal access is not enabled for this account"
                };
            }

            // Check if contact is active
            if (!contact.IsActive)
            {
                _logger.LogWarning("Portal login failed: Account inactive - {Email}", request.Email);
                return new PortalLoginResponse
                {
                    IsSuccess = false,
                    Message = "Your account is inactive. Please contact support."
                };
            }

            // Check if customer is active
            if (contact.Customer.Status != CustomerStatus.Active)
            {
                _logger.LogWarning("Portal login failed: Customer inactive - {Email}", request.Email);
                return new PortalLoginResponse
                {
                    IsSuccess = false,
                    Message = "Your organization's account is inactive. Please contact support."
                };
            }

            // Check if account is locked
            if (contact.LockedUntil.HasValue && contact.LockedUntil > DateTime.UtcNow)
            {
                _logger.LogWarning("Portal login failed: Account locked - {Email}", request.Email);
                return new PortalLoginResponse
                {
                    IsSuccess = false,
                    Message = "Your account is temporarily locked due to multiple failed login attempts.",
                    IsAccountLocked = true,
                    LockedUntil = contact.LockedUntil
                };
            }

            // Verify password
            if (string.IsNullOrEmpty(contact.PasswordHash) ||
                !_passwordService.VerifyPassword(request.Password, contact.PasswordHash))
            {
                // Increment failed attempts
                contact.FailedLoginAttempts++;

                if (contact.FailedLoginAttempts >= MaxFailedAttempts)
                {
                    contact.LockedUntil = DateTime.UtcNow.AddMinutes(LockoutMinutes);
                    _logger.LogWarning("Portal account locked due to failed attempts - {Email}", request.Email);
                }

                await _context.SaveChangesAsync();

                _logger.LogWarning("Portal login failed: Invalid password - {Email}", request.Email);
                return new PortalLoginResponse
                {
                    IsSuccess = false,
                    Message = "Invalid email or password"
                };
            }

            // Check password expiry
            var mustChangePassword = contact.MustChangePassword ||
                (contact.PasswordExpiresAt.HasValue && contact.PasswordExpiresAt <= DateTime.UtcNow);

            // Generate tokens
            var tokenResult = _jwtService.GeneratePortalAccessToken(contact, contact.Customer);
            var refreshToken = _jwtService.GenerateRefreshToken();

            // Update contact
            contact.FailedLoginAttempts = 0;
            contact.LockedUntil = null;
            contact.LastLoginAt = DateTime.UtcNow;

            // Save refresh token (you might want to store this in a separate table)
            // For now, we'll just return it

            await _context.SaveChangesAsync();

            _logger.LogInformation("Portal login successful - {Email}", request.Email);

            return new PortalLoginResponse
            {
                IsSuccess = true,
                Message = "Login successful",
                Token = tokenResult.Token,
                RefreshToken = refreshToken,
                ExpiresAt = tokenResult.ExpiresAt,
                MustChangePassword = mustChangePassword,
                User = MapToPortalUserDto(contact, contact.Customer)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during portal login for {Email}", request.Email);
            return new PortalLoginResponse
            {
                IsSuccess = false,
                Message = "An error occurred during login. Please try again."
            };
        }
    }

    public async Task<PortalLoginResponse> RefreshTokenAsync(PortalRefreshTokenRequest request, string? ipAddress = null)
    {
        try
        {
            var contactId = _jwtService.GetPortalContactIdFromToken(request.Token);
            if (!contactId.HasValue)
            {
                return new PortalLoginResponse
                {
                    IsSuccess = false,
                    Message = "Invalid token"
                };
            }

            var contact = await _context.CustomerContacts
                .Include(c => c.Customer)
                .FirstOrDefaultAsync(c => c.Id == contactId.Value && !c.IsDeleted);

            if (contact == null || !contact.IsPortalUser || !contact.IsActive)
            {
                return new PortalLoginResponse
                {
                    IsSuccess = false,
                    Message = "Invalid token"
                };
            }

            // Generate new tokens
            var tokenResult = _jwtService.GeneratePortalAccessToken(contact, contact.Customer);
            var newRefreshToken = _jwtService.GenerateRefreshToken();

            return new PortalLoginResponse
            {
                IsSuccess = true,
                Message = "Token refreshed",
                Token = tokenResult.Token,
                RefreshToken = newRefreshToken,
                ExpiresAt = tokenResult.ExpiresAt,
                User = MapToPortalUserDto(contact, contact.Customer)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing portal token");
            return new PortalLoginResponse
            {
                IsSuccess = false,
                Message = "Failed to refresh token"
            };
        }
    }

    public async Task<Result> LogoutAsync(Guid contactId, string? refreshToken = null)
    {
        // In a full implementation, you would revoke the refresh token here
        return Result.Success("Logged out successfully");
    }

    public async Task<Result> ChangePasswordAsync(Guid contactId, PortalChangePasswordRequest request)
    {
        try
        {
            if (request.NewPassword != request.ConfirmPassword)
            {
                return Result.Failure("New password and confirmation do not match");
            }

            var contact = await _context.CustomerContacts
                .FirstOrDefaultAsync(c => c.Id == contactId && !c.IsDeleted);

            if (contact == null)
            {
                return Result.Failure("Contact not found");
            }

            // Verify current password
            if (!_passwordService.VerifyPassword(request.CurrentPassword, contact.PasswordHash ?? ""))
            {
                return Result.Failure("Current password is incorrect");
            }

            // Hash and save new password
            contact.PasswordHash = _passwordService.HashPassword(request.NewPassword);
            contact.LastPasswordChangeAt = DateTime.UtcNow;
            contact.MustChangePassword = false;
            contact.PasswordExpiresAt = DateTime.UtcNow.AddDays(90); // 90-day expiry

            await _context.SaveChangesAsync();

            _logger.LogInformation("Portal password changed for contact {ContactId}", contactId);
            return Result.Success("Password changed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing portal password for contact {ContactId}", contactId);
            return Result.Failure("Failed to change password");
        }
    }

    public async Task<Result> ForgotPasswordAsync(PortalForgotPasswordRequest request)
    {
        // In a full implementation, this would:
        // 1. Find the contact by email
        // 2. Generate a password reset token
        // 3. Send email with reset link
        // For now, return success to not reveal if email exists
        _logger.LogInformation("Portal forgot password requested for {Email}", request.Email);
        return Result.Success("If an account exists with this email, you will receive password reset instructions.");
    }

    public async Task<Result> ResetPasswordAsync(PortalResetPasswordRequest request)
    {
        // In a full implementation, this would:
        // 1. Validate the reset token
        // 2. Update the password
        return Result.Failure("Password reset functionality not yet implemented");
    }

    public async Task<PortalUserDto?> GetProfileAsync(Guid contactId)
    {
        var contact = await _context.CustomerContacts
            .Include(c => c.Customer)
            .Include(c => c.Location)
            .FirstOrDefaultAsync(c => c.Id == contactId && !c.IsDeleted);

        if (contact == null)
            return null;

        return MapToPortalUserDto(contact, contact.Customer);
    }

    public async Task<Result<PortalUserDto>> UpdateProfileAsync(Guid contactId, UpdatePortalProfileRequest request)
    {
        try
        {
            var contact = await _context.CustomerContacts
                .Include(c => c.Customer)
                .FirstOrDefaultAsync(c => c.Id == contactId && !c.IsDeleted);

            if (contact == null)
            {
                return Result<PortalUserDto>.Failure("Contact not found");
            }

            // Update allowed fields
            contact.Phone = request.Phone;
            contact.Mobile = request.Mobile;
            contact.PreferredLanguage = request.PreferredLanguage;
            contact.PreferredTimeZone = request.PreferredTimeZone;
            contact.ReceiveEmailNotifications = request.ReceiveEmailNotifications;
            contact.ReceiveSmsNotifications = request.ReceiveSmsNotifications;
            contact.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Result<PortalUserDto>.Success(MapToPortalUserDto(contact, contact.Customer));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating portal profile for contact {ContactId}", contactId);
            return Result<PortalUserDto>.Failure("Failed to update profile");
        }
    }

    #endregion

    #region Dashboard

    public async Task<PortalDashboardDto> GetDashboardAsync(Guid contactId)
    {
        var contact = await _context.CustomerContacts
            .Include(c => c.Customer)
            .FirstOrDefaultAsync(c => c.Id == contactId && !c.IsDeleted);

        if (contact == null)
        {
            return new PortalDashboardDto();
        }

        var customerId = contact.CustomerId;
        var companyId = contact.Customer.CompanyId;

        // Build query for tickets based on permissions
        var ticketsQuery = _context.Complaints
            .Where(c => c.CustomerId == customerId && !c.IsDeleted);

        // Apply location filter if user doesn't have all-customer access
        if (!contact.CanViewAllCustomerTickets && !contact.CanViewAllLocationTickets && contact.LocationId.HasValue)
        {
            ticketsQuery = ticketsQuery.Where(c => c.CustomerLocationId == contact.LocationId);
        }
        else if (!contact.CanViewAllCustomerTickets && contact.CanViewAllLocationTickets && contact.LocationId.HasValue)
        {
            ticketsQuery = ticketsQuery.Where(c => c.CustomerLocationId == contact.LocationId);
        }

        // Get ticket counts
        var allTickets = await ticketsQuery.ToListAsync();

        var dashboard = new PortalDashboardDto
        {
            TotalTickets = allTickets.Count,
            OpenTickets = allTickets.Count(t => !IsClosedStatus(t.StatusMasterId)),
            InProgressTickets = allTickets.Count(t => IsInProgressStatus(t.StatusMasterId)),
            ResolvedTickets = allTickets.Count(t => t.ResolvedAt.HasValue),
            ClosedTickets = allTickets.Count(t => IsClosedStatus(t.StatusMasterId)),
            OverdueTickets = allTickets.Count(t => t.DueDate.HasValue && t.DueDate < DateTime.UtcNow && !t.ResolvedAt.HasValue),
            TicketsThisMonth = allTickets.Count(t => t.CreatedAt >= DateTime.UtcNow.AddDays(-30)),
            TicketsAwaitingResponse = allTickets.Count(t => t.HasCustomerResponse)
        };

        // Get recent tickets
        dashboard.RecentTickets = await ticketsQuery
            .OrderByDescending(t => t.CreatedAt)
            .Take(5)
            .Select(t => new PortalTicketSummaryDto
            {
                Id = t.Id,
                ComplaintNumber = t.ComplaintNumber,
                Title = t.Title,
                Description = t.Description,
                CategoryName = t.Category != null ? t.Category.Name : "",
                StatusName = t.StatusMaster != null ? t.StatusMaster.Name : "",
                StatusColor = t.StatusMaster != null ? t.StatusMaster.ColorCode ?? "" : "",
                PriorityName = t.PriorityMaster != null ? t.PriorityMaster.Name : "",
                PriorityColor = t.PriorityMaster != null ? t.PriorityMaster.ColorCode ?? "" : "",
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt
            })
            .ToListAsync();

        // Get asset stats if permission
        if (contact.CanViewAssets)
        {
            var assetsQuery = _context.Assets
                .Where(a => a.CustomerId == customerId && !a.IsDeleted);

            if (contact.LocationId.HasValue && !contact.CanViewAllCustomerTickets)
            {
                assetsQuery = assetsQuery.Where(a => a.LocationId == contact.LocationId);
            }

            var assets = await assetsQuery.ToListAsync();
            dashboard.TotalAssets = assets.Count;
            dashboard.ActiveAssets = assets.Count(a => a.Status == Domain.Enums.Service.AssetStatus.Deployed);
            dashboard.AssetsNeedingService = assets.Count(a => a.NextServiceDate.HasValue && a.NextServiceDate <= DateTime.UtcNow.AddDays(7));
        }

        // Get contract stats if permission
        if (contact.CanViewContracts)
        {
            var contracts = await _context.Contracts
                .Where(c => c.CustomerId == customerId && !c.IsDeleted)
                .ToListAsync();

            dashboard.ActiveContracts = contracts.Count(c => c.Status == Domain.Enums.Service.ContractStatus.Active);
            dashboard.ContractsExpiringSoon = contracts.Count(c =>
                c.Status == Domain.Enums.Service.ContractStatus.Active &&
                c.EndDate <= DateTime.UtcNow.AddDays(30));
        }

        return dashboard;
    }

    private bool IsClosedStatus(Guid statusId)
    {
        // This should be enhanced to check the actual status configuration
        return false;
    }

    private bool IsInProgressStatus(Guid statusId)
    {
        // This should be enhanced to check the actual status configuration
        return false;
    }

    #endregion

    #region Tickets

    public async Task<PagedResult<PortalTicketSummaryDto>> GetTicketsAsync(Guid contactId, PortalTicketListQuery query)
    {
        var contact = await _context.CustomerContacts
            .FirstOrDefaultAsync(c => c.Id == contactId && !c.IsDeleted);

        if (contact == null)
        {
            return new PagedResult<PortalTicketSummaryDto>(new List<PortalTicketSummaryDto>(), query.Page, query.PageSize, 0);
        }

        var ticketsQuery = _context.Complaints
            .Include(c => c.Category)
            .Include(c => c.StatusMaster)
            .Include(c => c.PriorityMaster)
            .Include(c => c.AssignedTo)
            .Where(c => c.CustomerId == contact.CustomerId && !c.IsDeleted);

        // Apply location filter
        if (!contact.CanViewAllCustomerTickets && contact.LocationId.HasValue)
        {
            ticketsQuery = ticketsQuery.Where(c => c.CustomerLocationId == contact.LocationId);
        }

        // Apply filters
        if (!string.IsNullOrEmpty(query.SearchTerm))
        {
            var searchTerm = query.SearchTerm.ToLower();
            ticketsQuery = ticketsQuery.Where(c =>
                c.ComplaintNumber.ToLower().Contains(searchTerm) ||
                c.Title.ToLower().Contains(searchTerm) ||
                (c.Description != null && c.Description.ToLower().Contains(searchTerm)));
        }

        if (query.CategoryId.HasValue)
            ticketsQuery = ticketsQuery.Where(c => c.CategoryId == query.CategoryId.Value);

        if (query.StatusId.HasValue)
            ticketsQuery = ticketsQuery.Where(c => c.StatusMasterId == query.StatusId.Value);

        if (query.PriorityId.HasValue)
            ticketsQuery = ticketsQuery.Where(c => c.PriorityMasterId == query.PriorityId.Value);

        if (query.LocationId.HasValue)
            ticketsQuery = ticketsQuery.Where(c => c.CustomerLocationId == query.LocationId.Value);

        if (query.AssetId.HasValue)
            ticketsQuery = ticketsQuery.Where(c => c.AssetId == query.AssetId.Value);

        if (query.FromDate.HasValue)
            ticketsQuery = ticketsQuery.Where(c => c.CreatedAt >= query.FromDate.Value);

        if (query.ToDate.HasValue)
            ticketsQuery = ticketsQuery.Where(c => c.CreatedAt <= query.ToDate.Value);

        // Get total count
        var totalCount = await ticketsQuery.CountAsync();

        // Apply sorting
        ticketsQuery = query.SortBy?.ToLower() switch
        {
            "createdat" => query.SortDescending ? ticketsQuery.OrderByDescending(c => c.CreatedAt) : ticketsQuery.OrderBy(c => c.CreatedAt),
            "updatedat" => query.SortDescending ? ticketsQuery.OrderByDescending(c => c.UpdatedAt) : ticketsQuery.OrderBy(c => c.UpdatedAt),
            "priority" => query.SortDescending ? ticketsQuery.OrderByDescending(c => c.PriorityMaster!.Level) : ticketsQuery.OrderBy(c => c.PriorityMaster!.Level),
            "status" => query.SortDescending ? ticketsQuery.OrderByDescending(c => c.StatusMaster!.DisplayOrder) : ticketsQuery.OrderBy(c => c.StatusMaster!.DisplayOrder),
            _ => ticketsQuery.OrderByDescending(c => c.CreatedAt)
        };

        // Apply pagination
        var items = await ticketsQuery
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(c => new PortalTicketSummaryDto
            {
                Id = c.Id,
                ComplaintNumber = c.ComplaintNumber,
                Title = c.Title,
                Description = c.Description,
                CategoryName = c.Category != null ? c.Category.Name : "",
                StatusName = c.StatusMaster != null ? c.StatusMaster.Name : "",
                StatusColor = c.StatusMaster != null ? c.StatusMaster.ColorCode ?? "" : "",
                PriorityName = c.PriorityMaster != null ? c.PriorityMaster.Name : "",
                PriorityColor = c.PriorityMaster != null ? c.PriorityMaster.ColorCode ?? "" : "",
                AssignedToName = c.AssignedTo != null ? c.AssignedTo.FullName : null,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                ResolvedAt = c.ResolvedAt,
                ClosedAt = c.ClosedAt,
                HasCustomerResponse = c.HasCustomerResponse,
                IsOverdue = c.DueDate.HasValue && c.DueDate < DateTime.UtcNow && !c.ResolvedAt.HasValue
            })
            .ToListAsync();

        return new PagedResult<PortalTicketSummaryDto>(items, query.Page, query.PageSize, totalCount);
    }

    public async Task<PortalTicketDetailDto?> GetTicketByIdAsync(Guid contactId, Guid ticketId)
    {
        if (!await CanAccessTicketAsync(contactId, ticketId))
            return null;

        var ticket = await _context.Complaints
            .Include(c => c.Category)
            .Include(c => c.StatusMaster)
            .Include(c => c.PriorityMaster)
            .Include(c => c.AssignedTo)
            .Include(c => c.CustomerLocation)
            .Include(c => c.Asset)
            .Include(c => c.Contract)
            .Include(c => c.Comments.Where(cm => !cm.IsDeleted))
                .ThenInclude(cm => cm.Commenter)
            .Include(c => c.Attachments.Where(a => !a.IsDeleted))
            .FirstOrDefaultAsync(c => c.Id == ticketId && !c.IsDeleted);

        if (ticket == null)
            return null;

        return new PortalTicketDetailDto
        {
            Id = ticket.Id,
            ComplaintNumber = ticket.ComplaintNumber,
            Title = ticket.Title,
            Description = ticket.Description,
            CategoryId = ticket.CategoryId,
            CategoryName = ticket.Category?.Name ?? "",
            StatusId = ticket.StatusMasterId,
            StatusName = ticket.StatusMaster?.Name ?? "",
            StatusColor = ticket.StatusMaster?.ColorCode ?? "",
            PriorityId = ticket.PriorityMasterId,
            PriorityName = ticket.PriorityMaster?.Name ?? "",
            PriorityColor = ticket.PriorityMaster?.ColorCode ?? "",
            AssignedToName = ticket.AssignedTo?.FullName,
            AssignedToEmail = ticket.AssignedTo?.Email,
            LocationId = ticket.CustomerLocationId,
            LocationName = ticket.CustomerLocation?.Name,
            AssetId = ticket.AssetId,
            AssetName = ticket.Asset?.Name,
            AssetTag = ticket.Asset?.AssetTag,
            AssetSerialNumber = ticket.Asset?.SerialNumber,
            ContractId = ticket.ContractId,
            ContractNumber = ticket.Contract?.ContractNumber,
            IsUnderWarranty = ticket.IsUnderWarranty,
            IsUnderContract = ticket.IsUnderContract,
            CreatedAt = ticket.CreatedAt,
            UpdatedAt = ticket.UpdatedAt,
            ResolvedAt = ticket.ResolvedAt,
            ClosedAt = ticket.ClosedAt,
            ResponseDueDate = ticket.DueDate,
            ResolutionDueDate = ticket.DueDate,
            IsOverdue = ticket.DueDate.HasValue && ticket.DueDate < DateTime.UtcNow && !ticket.ResolvedAt.HasValue,
            CustomerRating = ticket.CustomerSatisfactionRating,
            CustomerFeedback = ticket.CustomerFeedback,
            CanRate = ticket.ResolvedAt.HasValue && !ticket.CustomerSatisfactionRating.HasValue,
            CanAddComment = true,
            CanReopen = ticket.ClosedAt.HasValue,
            Comments = ticket.Comments
                .Where(cm => !cm.IsInternal)
                .Select(cm => new PortalTicketCommentDto
                {
                    Id = cm.Id,
                    Content = cm.CommentText,
                    IsInternal = cm.IsInternal,
                    IsFromCustomer = false, // Would need to check if CommentedBy is a CustomerContact
                    AuthorName = cm.Commenter?.FullName ?? "Customer",
                    CreatedAt = cm.CommentedAt
                }).OrderBy(cm => cm.CreatedAt).ToList(),
            Attachments = ticket.Attachments.Select(a => new PortalTicketAttachmentDto
            {
                Id = a.Id,
                FileName = a.FileName,
                ContentType = a.ContentType,
                FileSize = a.FileSize,
                UploadedAt = a.UploadedAt,
                DownloadUrl = $"/api/portal/tickets/{ticketId}/attachments/{a.Id}"
            }).ToList()
        };
    }

    public async Task<PortalTicketDetailDto?> GetTicketByNumberAsync(Guid contactId, string complaintNumber)
    {
        var ticket = await _context.Complaints
            .FirstOrDefaultAsync(c => c.ComplaintNumber == complaintNumber && !c.IsDeleted);

        if (ticket == null)
            return null;

        return await GetTicketByIdAsync(contactId, ticket.Id);
    }

    public async Task<Result<PortalTicketDetailDto>> CreateTicketAsync(Guid contactId, PortalCreateTicketRequest request)
    {
        try
        {
            // Get contact with customer
            var contact = await _context.CustomerContacts
                .Include(c => c.Customer)
                .FirstOrDefaultAsync(c => c.Id == contactId && !c.IsDeleted);

            if (contact == null)
            {
                return Result<PortalTicketDetailDto>.Failure("Contact not found");
            }

            // Validate category if provided
            Guid categoryId;
            if (request.CategoryId != Guid.Empty)
            {
                categoryId = request.CategoryId;
            }
            else
            {
                // Get default category
                var defaultCategory = await _context.ComplaintCategories
                    .Where(c => !c.IsDeleted)
                    .OrderBy(c => c.DisplayOrder)
                    .FirstOrDefaultAsync();

                if (defaultCategory == null)
                {
                    return Result<PortalTicketDetailDto>.Failure("No category found. Please configure categories first.");
                }
                categoryId = defaultCategory.Id;
            }

            // Get default open status (first active status by display order)
            var defaultStatus = await _context.ComplaintStatusMasters
                .Where(s => !s.IsDeleted && s.IsActive && !s.IsFinal)
                .OrderBy(s => s.DisplayOrder)
                .FirstOrDefaultAsync();

            if (defaultStatus == null)
            {
                // Fallback to any active status
                defaultStatus = await _context.ComplaintStatusMasters
                    .Where(s => !s.IsDeleted && s.IsActive)
                    .OrderBy(s => s.DisplayOrder)
                    .FirstOrDefaultAsync();
            }

            if (defaultStatus == null)
            {
                return Result<PortalTicketDetailDto>.Failure("No status configured. Please configure status masters first.");
            }

            // Get priority
            Guid priorityId;
            if (request.PriorityId.HasValue && request.PriorityId.Value != Guid.Empty)
            {
                priorityId = request.PriorityId.Value;
            }
            else
            {
                // Get default priority (first active by level)
                var defaultPriority = await _context.ComplaintPriorityMasters
                    .Where(p => !p.IsDeleted && p.IsActive)
                    .OrderBy(p => p.Level)
                    .FirstOrDefaultAsync();

                if (defaultPriority == null)
                {
                    return Result<PortalTicketDetailDto>.Failure("No priority configured. Please configure priority masters first.");
                }
                priorityId = defaultPriority.Id;
            }

            // Generate complaint number using company ID
            var companyId = contact.Customer.CompanyId;
            var complaintNumber = await GenerateComplaintNumberAsync(companyId);

            // Create the complaint
            var complaint = new Domain.Entities.Complaints.Complaint
            {
                Id = Guid.NewGuid(),
                ComplaintNumber = complaintNumber,
                Title = request.Title,
                Description = request.Description ?? string.Empty,
                CategoryId = categoryId,
                ComplainantId = contact.Id, // Using contact ID as complainant
                CompanyId = contact.Customer.CompanyId,
                StatusMasterId = defaultStatus.Id,
                PriorityMasterId = priorityId,
                SubmittedAt = DateTime.UtcNow,
                CustomerId = contact.CustomerId,
                CustomerContactId = contactId,
                CustomerLocationId = request.LocationId,
                AssetId = request.AssetId,
                ProductId = request.ProductId,
                ContactEmail = contact.Email,
                ContactPhone = request.ContactPhone ?? contact.Phone,
                Source = Domain.Enums.TicketSource.Portal,
                HasCustomerResponse = false,
                CurrentEscalationLevel = 0,
                IsAnonymous = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Complaints.Add(complaint);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Portal ticket created: {ComplaintNumber} by contact {ContactId}", complaintNumber, contactId);

            // Return the created ticket details
            var result = await GetTicketByIdAsync(contactId, complaint.Id);
            if (result == null)
            {
                return Result<PortalTicketDetailDto>.Failure("Ticket created but failed to retrieve details");
            }

            return Result<PortalTicketDetailDto>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating portal ticket for contact {ContactId}", contactId);
            return Result<PortalTicketDetailDto>.Failure("An error occurred while creating the ticket");
        }
    }

    private async Task<string> GenerateComplaintNumberAsync(Guid companyId)
    {
        // Get current date parts
        var now = DateTime.UtcNow;
        var year = now.Year.ToString().Substring(2); // Last 2 digits
        var month = now.Month.ToString("D2");

        // Get count for this month
        var prefix = $"TKT{year}{month}";
        var countThisMonth = await _context.Complaints
            .Where(c => c.CompanyId == companyId && c.ComplaintNumber.StartsWith(prefix))
            .CountAsync();

        // Generate sequential number
        var sequence = (countThisMonth + 1).ToString("D4");
        return $"{prefix}{sequence}";
    }

    public async Task<Result<PortalTicketCommentDto>> AddCommentAsync(Guid contactId, Guid ticketId, PortalAddCommentRequest request)
    {
        if (!await CanAccessTicketAsync(contactId, ticketId))
        {
            return Result<PortalTicketCommentDto>.Failure("Ticket not found");
        }

        var contact = await _context.CustomerContacts
            .FirstOrDefaultAsync(c => c.Id == contactId && !c.IsDeleted);

        if (contact == null)
        {
            return Result<PortalTicketCommentDto>.Failure("Contact not found");
        }

        var ticket = await _context.Complaints
            .FirstOrDefaultAsync(c => c.Id == ticketId && !c.IsDeleted);

        if (ticket == null)
        {
            return Result<PortalTicketCommentDto>.Failure("Ticket not found");
        }

        // Create comment
        var comment = new Domain.Entities.Complaints.ComplaintComment
        {
            Id = Guid.NewGuid(),
            ComplaintId = ticketId,
            CommentText = request.Content,
            IsInternal = false,
            CommentedAt = DateTime.UtcNow,
            CommentedBy = contactId, // Note: This may need adjustment as CommentedBy expects a User ID, not CustomerContact ID
            CreatedAt = DateTime.UtcNow
        };

        _context.ComplaintComments.Add(comment);

        // Update ticket to mark customer response
        ticket.HasCustomerResponse = true;
        ticket.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Result<PortalTicketCommentDto>.Success(new PortalTicketCommentDto
        {
            Id = comment.Id,
            Content = comment.CommentText,
            IsInternal = false,
            IsFromCustomer = true,
            AuthorName = contact.FullName,
            CreatedAt = comment.CommentedAt
        });
    }

    public async Task<Result> RateTicketAsync(Guid contactId, Guid ticketId, PortalRateTicketRequest request)
    {
        if (!await CanAccessTicketAsync(contactId, ticketId))
        {
            return Result.Failure("Ticket not found");
        }

        var ticket = await _context.Complaints
            .FirstOrDefaultAsync(c => c.Id == ticketId && !c.IsDeleted);

        if (ticket == null)
        {
            return Result.Failure("Ticket not found");
        }

        if (!ticket.ResolvedAt.HasValue)
        {
            return Result.Failure("Cannot rate an unresolved ticket");
        }

        if (request.Rating < 1 || request.Rating > 5)
        {
            return Result.Failure("Rating must be between 1 and 5");
        }

        ticket.CustomerSatisfactionRating = request.Rating;
        ticket.CustomerFeedback = request.Feedback;
        ticket.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Result.Success("Thank you for your feedback!");
    }

    public async Task<Result<PortalTicketDetailDto>> ReopenTicketAsync(Guid contactId, Guid ticketId, PortalReopenTicketRequest request)
    {
        // This would reopen a closed ticket
        return Result<PortalTicketDetailDto>.Failure("Ticket reopening through portal not yet implemented");
    }

    #endregion

    #region Assets

    public async Task<PagedResult<PortalAssetSummaryDto>> GetAssetsAsync(Guid contactId, int page = 1, int pageSize = 20, string? searchTerm = null)
    {
        var contact = await _context.CustomerContacts
            .FirstOrDefaultAsync(c => c.Id == contactId && !c.IsDeleted);

        if (contact == null || !contact.CanViewAssets)
        {
            return new PagedResult<PortalAssetSummaryDto>(new List<PortalAssetSummaryDto>(), page, pageSize, 0);
        }

        var query = _context.Assets
            .Include(a => a.Location)
            .Where(a => a.CustomerId == contact.CustomerId && !a.IsDeleted);

        if (contact.LocationId.HasValue && !contact.CanViewAllCustomerTickets)
        {
            query = query.Where(a => a.LocationId == contact.LocationId);
        }

        if (!string.IsNullOrEmpty(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(a =>
                a.AssetTag.ToLower().Contains(term) ||
                a.Name.ToLower().Contains(term) ||
                (a.SerialNumber != null && a.SerialNumber.ToLower().Contains(term)));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(a => a.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new PortalAssetSummaryDto
            {
                Id = a.Id,
                AssetTag = a.AssetTag,
                Name = a.Name,
                SerialNumber = a.SerialNumber,
                ProductName = a.ProductName,
                Manufacturer = a.Manufacturer,
                Model = a.Model,
                TypeName = a.Type.ToString(),
                StatusName = a.Status.ToString(),
                ConditionName = a.Condition.ToString(),
                LocationName = a.Location != null ? a.Location.Name : null,
                IsUnderWarranty = a.WarrantyEndDate.HasValue && a.WarrantyEndDate > DateTime.UtcNow,
                IsUnderContract = a.ContractId.HasValue,
                WarrantyEndDate = a.WarrantyEndDate,
                LastServiceDate = a.LastServiceDate,
                NextServiceDate = a.NextServiceDate
            })
            .ToListAsync();

        return new PagedResult<PortalAssetSummaryDto>(items, page, pageSize, totalCount);
    }

    public async Task<PortalAssetDetailDto?> GetAssetByIdAsync(Guid contactId, Guid assetId)
    {
        if (!await CanAccessAssetAsync(contactId, assetId))
            return null;

        var asset = await _context.Assets
            .Include(a => a.Location)
            .Include(a => a.Contract)
            .Include(a => a.ServiceHistory.Where(sh => !sh.IsDeleted).OrderByDescending(sh => sh.ServiceStartDate).Take(5))
            .FirstOrDefaultAsync(a => a.Id == assetId && !a.IsDeleted);

        if (asset == null)
            return null;

        return new PortalAssetDetailDto
        {
            Id = asset.Id,
            AssetTag = asset.AssetTag,
            Name = asset.Name,
            Description = asset.Description,
            SerialNumber = asset.SerialNumber,
            TypeName = asset.Type.ToString(),
            StatusName = asset.Status.ToString(),
            ConditionName = asset.Condition.ToString(),
            ProductName = asset.ProductName,
            ProductCode = asset.ProductCode,
            Manufacturer = asset.Manufacturer,
            Model = asset.Model,
            Version = asset.Version,
            LocationName = asset.Location?.Name,
            Building = asset.Building,
            Floor = asset.Floor,
            Room = asset.Room,
            IsUnderWarranty = asset.WarrantyEndDate.HasValue && asset.WarrantyEndDate > DateTime.UtcNow,
            WarrantyStartDate = asset.WarrantyStartDate,
            WarrantyEndDate = asset.WarrantyEndDate,
            IsUnderContract = asset.ContractId.HasValue,
            ContractNumber = asset.Contract?.ContractNumber,
            ContractEndDate = asset.Contract?.EndDate,
            InstallationDate = asset.InstallationDate,
            LastServiceDate = asset.LastServiceDate,
            NextServiceDate = asset.NextServiceDate,
            ServiceCallCount = asset.ServiceCallCount,
            CanCreateTicket = true,
            RecentServiceHistory = asset.ServiceHistory.Select(sh => new PortalServiceHistorySummaryDto
            {
                Id = sh.Id,
                ServiceNumber = sh.ServiceNumber,
                ServiceTypeName = sh.ServiceType.ToString(),
                Description = sh.Description,
                ResultName = sh.Result.ToString(),
                ServiceDate = sh.ServiceStartDate
            }).ToList()
        };
    }

    public async Task<List<PortalAssetSummaryDto>> GetAssetsByLocationAsync(Guid contactId, Guid locationId)
    {
        var result = await GetAssetsAsync(contactId, 1, 1000);
        return result.Items.Where(a => a.LocationName != null).ToList();
    }

    #endregion

    #region Contracts

    public async Task<PagedResult<PortalContractSummaryDto>> GetContractsAsync(Guid contactId, int page = 1, int pageSize = 20, bool? activeOnly = true)
    {
        var contact = await _context.CustomerContacts
            .FirstOrDefaultAsync(c => c.Id == contactId && !c.IsDeleted);

        if (contact == null || !contact.CanViewContracts)
        {
            return new PagedResult<PortalContractSummaryDto>(new List<PortalContractSummaryDto>(), page, pageSize, 0);
        }

        var query = _context.Contracts
            .Include(c => c.Items)
            .Where(c => c.CustomerId == contact.CustomerId && !c.IsDeleted);

        if (activeOnly == true)
        {
            query = query.Where(c => c.Status == Domain.Enums.Service.ContractStatus.Active);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(c => c.StartDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new PortalContractSummaryDto
            {
                Id = c.Id,
                ContractNumber = c.ContractNumber,
                Name = c.Name,
                TypeName = c.Type.ToString(),
                StatusName = c.Status.ToString(),
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                IsExpiringSoon = c.EndDate <= DateTime.UtcNow.AddDays(30),
                DaysUntilExpiry = (int)(c.EndDate - DateTime.UtcNow).TotalDays,
                CoveredAssetsCount = c.Items.Count(i => i.AssetId.HasValue),
                SupportHours = c.SupportHoursType != null ? c.SupportHoursType.ToString() : null,
                ResponseTimeHours = c.ResponseTimeHours,
                ResolutionTimeHours = c.ResolutionTimeHours
            })
            .ToListAsync();

        return new PagedResult<PortalContractSummaryDto>(items, page, pageSize, totalCount);
    }

    public async Task<PortalContractDetailDto?> GetContractByIdAsync(Guid contactId, Guid contractId)
    {
        if (!await CanAccessContractAsync(contactId, contractId))
            return null;

        var contract = await _context.Contracts
            .Include(c => c.Items)
                .ThenInclude(i => i.Asset)
            .FirstOrDefaultAsync(c => c.Id == contractId && !c.IsDeleted);

        if (contract == null)
            return null;

        return new PortalContractDetailDto
        {
            Id = contract.Id,
            ContractNumber = contract.ContractNumber,
            Name = contract.Name,
            Description = contract.Description,
            TypeName = contract.Type.ToString(),
            StatusName = contract.Status.ToString(),
            CoverageTypeName = contract.CoverageType.ToString(),
            StartDate = contract.StartDate,
            EndDate = contract.EndDate,
            SignedDate = contract.SignedDate,
            AutoRenew = contract.AutoRenew,
            NextRenewalDate = contract.NextRenewalDate,
            RenewalNoticeDays = contract.RenewalNoticeDays,
            IncludesOnsite = contract.IncludesOnsite,
            IncludesRemote = contract.IncludesRemote,
            IncludedHours = contract.IncludedHours,
            UsedHours = contract.UsedHours,
            SupportHours = contract.SupportHoursType?.ToString(),
            ResponseTimeHours = contract.ResponseTimeHours,
            ResolutionTimeHours = contract.ResolutionTimeHours,
            IsActive = contract.Status == Domain.Enums.Service.ContractStatus.Active,
            IsExpired = contract.EndDate < DateTime.UtcNow,
            IsExpiringSoon = contract.EndDate <= DateTime.UtcNow.AddDays(30),
            DaysUntilExpiry = (int)(contract.EndDate - DateTime.UtcNow).TotalDays,
            CoveredAssets = contract.Items
                .Where(i => i.AssetId.HasValue && i.Asset != null)
                .Select(i => new PortalContractAssetDto
                {
                    AssetId = i.AssetId!.Value,
                    AssetTag = i.Asset!.AssetTag,
                    AssetName = i.Asset.Name,
                    SerialNumber = i.Asset.SerialNumber,
                    ProductName = i.Asset.ProductName,
                    IsActive = i.IsActive
                })
                .ToList()
        };
    }

    #endregion

    #region Locations

    public async Task<List<PortalLocationSummaryDto>> GetLocationsAsync(Guid contactId)
    {
        var contact = await _context.CustomerContacts
            .FirstOrDefaultAsync(c => c.Id == contactId && !c.IsDeleted);

        if (contact == null)
        {
            return new List<PortalLocationSummaryDto>();
        }

        var query = _context.CustomerLocations
            .Where(l => l.CustomerId == contact.CustomerId && l.IsActive && !l.IsDeleted);

        // If user is restricted to a location, only show that location
        if (contact.LocationId.HasValue && !contact.CanViewAllCustomerTickets)
        {
            query = query.Where(l => l.Id == contact.LocationId);
        }

        return await query
            .OrderBy(l => l.Name)
            .Select(l => new PortalLocationSummaryDto
            {
                Id = l.Id,
                Code = l.Code,
                Name = l.Name,
                TypeName = l.Type.ToString(),
                AddressLine1 = l.AddressLine1,
                City = l.City,
                State = l.State,
                Country = l.Country,
                IsPrimary = l.IsPrimary
            })
            .ToListAsync();
    }

    #endregion

    #region Lookup Data

    public async Task<List<PortalCategoryDto>> GetCategoriesAsync(Guid contactId)
    {
        var contact = await _context.CustomerContacts
            .Include(c => c.Customer)
            .FirstOrDefaultAsync(c => c.Id == contactId && !c.IsDeleted);

        if (contact == null)
        {
            return new List<PortalCategoryDto>();
        }

        // Get all active root categories (categories are shared across companies)
        return await _context.ComplaintCategories
            .Where(c => c.IsActive && !c.IsDeleted && c.ParentCategoryId == null)
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .Select(c => new PortalCategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                DisplayOrder = c.DisplayOrder,
                SubCategories = c.SubCategories
                    .Where(sc => sc.IsActive && !sc.IsDeleted)
                    .OrderBy(sc => sc.DisplayOrder)
                    .Select(sc => new PortalCategoryDto
                    {
                        Id = sc.Id,
                        Name = sc.Name,
                        Description = sc.Description,
                        DisplayOrder = sc.DisplayOrder
                    })
                    .ToList()
            })
            .ToListAsync();
    }

    public async Task<List<PortalPriorityDto>> GetPrioritiesAsync(Guid contactId)
    {
        var contact = await _context.CustomerContacts
            .Include(c => c.Customer)
            .FirstOrDefaultAsync(c => c.Id == contactId && !c.IsDeleted);

        if (contact == null)
        {
            return new List<PortalPriorityDto>();
        }

        // Get priorities for the customer's company or global priorities
        return await _context.ComplaintPriorityMasters
            .Where(p => (p.CompanyId == contact.Customer.CompanyId || p.CompanyId == null) && p.IsActive && !p.IsDeleted)
            .OrderBy(p => p.DisplayOrder)
            .Select(p => new PortalPriorityDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Color = p.ColorCode ?? string.Empty,
                DisplayOrder = p.DisplayOrder
            })
            .ToListAsync();
    }

    public async Task<List<PortalStatusDto>> GetStatusesAsync(Guid contactId)
    {
        var contact = await _context.CustomerContacts
            .Include(c => c.Customer)
            .FirstOrDefaultAsync(c => c.Id == contactId && !c.IsDeleted);

        if (contact == null)
        {
            return new List<PortalStatusDto>();
        }

        // Get statuses for the customer's company or global statuses
        return await _context.ComplaintStatusMasters
            .Where(s => (s.CompanyId == contact.Customer.CompanyId || s.CompanyId == null) && s.IsActive && !s.IsDeleted)
            .OrderBy(s => s.DisplayOrder)
            .Select(s => new PortalStatusDto
            {
                Id = s.Id,
                Name = s.Name,
                Color = s.ColorCode ?? string.Empty,
                IsOpenStatus = !s.IsFinal,
                IsClosedStatus = s.IsFinal,
                DisplayOrder = s.DisplayOrder
            })
            .ToListAsync();
    }

    public async Task<List<PortalAssetSummaryDto>> GetAssetLookupsAsync(Guid contactId, Guid? locationId = null)
    {
        var result = await GetAssetsAsync(contactId, 1, 100);
        var assets = result.Items.ToList();

        if (locationId.HasValue)
        {
            // Filter by location - would need to add locationId to PortalAssetSummaryDto
        }

        return assets;
    }

    public async Task<List<PortalProductCategoryDto>> GetProductCategoriesAsync(Guid contactId, Guid? parentId = null)
    {
        var contact = await _context.CustomerContacts
            .Include(c => c.Customer)
            .FirstOrDefaultAsync(c => c.Id == contactId && !c.IsDeleted);

        if (contact?.Customer == null)
            return new List<PortalProductCategoryDto>();

        var companyId = contact.Customer.CompanyId;
        var query = _context.ProductCategories
            .Where(c => c.CompanyId == companyId && c.IsActive && !c.IsDeleted && c.IsPublic);

        if (parentId.HasValue)
        {
            query = query.Where(c => c.ParentCategoryId == parentId);
        }
        else
        {
            // Get root categories only
            query = query.Where(c => c.ParentCategoryId == null);
        }

        return await query
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .Select(c => new PortalProductCategoryDto
            {
                Id = c.Id,
                Code = c.Code,
                Name = c.Name,
                ParentCategoryId = c.ParentCategoryId,
                ParentCategoryName = c.ParentCategory != null ? c.ParentCategory.Name : null,
                Level = c.Level,
                Icon = c.Icon,
                Color = c.Color,
                HasChildren = c.SubCategories.Any(sc => sc.IsActive && !sc.IsDeleted)
            })
            .ToListAsync();
    }

    public async Task<List<PortalProductDto>> GetProductsAsync(Guid contactId, Guid? categoryId = null, Guid? brandId = null)
    {
        var contact = await _context.CustomerContacts
            .Include(c => c.Customer)
            .FirstOrDefaultAsync(c => c.Id == contactId && !c.IsDeleted);

        if (contact?.Customer == null)
            return new List<PortalProductDto>();

        var companyId = contact.Customer.CompanyId;
        var query = _context.Products
            .Include(p => p.Category)
            .Include(p => p.BrandEntity)
            .Where(p => p.CompanyId == companyId && p.Status == Domain.Enums.Product.ProductStatus.Active && !p.IsDeleted && p.IsPublic);

        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId);
        }

        if (brandId.HasValue)
        {
            query = query.Where(p => p.BrandId == brandId);
        }

        return await query
            .OrderBy(p => p.Name)
            .Take(100) // Limit for dropdown performance
            .Select(p => new PortalProductDto
            {
                Id = p.Id,
                ProductCode = p.ProductCode,
                Name = p.Name,
                Description = p.ShortDescription,
                CategoryId = p.CategoryId,
                CategoryName = p.Category != null ? p.Category.Name : null,
                BrandId = p.BrandId,
                BrandName = p.BrandEntity != null ? p.BrandEntity.Name : p.Brand,
                ImageUrl = p.ImageUrl,
                IsUnderWarranty = p.DefaultWarrantyMonths.HasValue && p.DefaultWarrantyMonths > 0
            })
            .ToListAsync();
    }

    public async Task<List<PortalBrandDto>> GetBrandsAsync(Guid contactId)
    {
        var contact = await _context.CustomerContacts
            .Include(c => c.Customer)
            .FirstOrDefaultAsync(c => c.Id == contactId && !c.IsDeleted);

        if (contact?.Customer == null)
            return new List<PortalBrandDto>();

        var companyId = contact.Customer.CompanyId;
        return await _context.Brands
            .Where(b => b.CompanyId == companyId && b.IsActive && !b.IsDeleted)
            .OrderBy(b => b.DisplayOrder)
            .ThenBy(b => b.Name)
            .Select(b => new PortalBrandDto
            {
                Id = b.Id,
                Code = b.Code,
                Name = b.Name,
                LogoUrl = b.LogoUrl,
                Website = b.WebsiteUrl
            })
            .ToListAsync();
    }

    #endregion

    #region Validation

    public async Task<bool> CanAccessTicketAsync(Guid contactId, Guid ticketId)
    {
        var contact = await _context.CustomerContacts
            .FirstOrDefaultAsync(c => c.Id == contactId && !c.IsDeleted);

        if (contact == null)
            return false;

        var ticket = await _context.Complaints
            .FirstOrDefaultAsync(c => c.Id == ticketId && !c.IsDeleted);

        if (ticket == null || ticket.CustomerId != contact.CustomerId)
            return false;

        // Check location restriction
        if (!contact.CanViewAllCustomerTickets && contact.LocationId.HasValue)
        {
            if (ticket.CustomerLocationId != contact.LocationId)
                return false;
        }

        return true;
    }

    public async Task<bool> CanAccessAssetAsync(Guid contactId, Guid assetId)
    {
        var contact = await _context.CustomerContacts
            .FirstOrDefaultAsync(c => c.Id == contactId && !c.IsDeleted);

        if (contact == null || !contact.CanViewAssets)
            return false;

        var asset = await _context.Assets
            .FirstOrDefaultAsync(a => a.Id == assetId && !a.IsDeleted);

        if (asset == null || asset.CustomerId != contact.CustomerId)
            return false;

        // Check location restriction
        if (!contact.CanViewAllCustomerTickets && contact.LocationId.HasValue)
        {
            if (asset.LocationId != contact.LocationId)
                return false;
        }

        return true;
    }

    public async Task<bool> CanAccessContractAsync(Guid contactId, Guid contractId)
    {
        var contact = await _context.CustomerContacts
            .FirstOrDefaultAsync(c => c.Id == contactId && !c.IsDeleted);

        if (contact == null || !contact.CanViewContracts)
            return false;

        var contract = await _context.Contracts
            .FirstOrDefaultAsync(c => c.Id == contractId && !c.IsDeleted);

        if (contract == null || contract.CustomerId != contact.CustomerId)
            return false;

        return true;
    }

    #endregion

    #region Helpers

    private PortalUserDto MapToPortalUserDto(CustomerContact contact, Customer customer)
    {
        return new PortalUserDto
        {
            Id = contact.Id,
            CustomerId = contact.CustomerId,
            CustomerName = customer.Name,
            CustomerCode = customer.Code,
            LocationId = contact.LocationId,
            LocationName = contact.Location?.Name,
            FirstName = contact.FirstName,
            LastName = contact.LastName,
            FullName = contact.FullName,
            Email = contact.Email,
            Phone = contact.Phone,
            JobTitle = contact.JobTitle,
            PortalRole = contact.PortalRole,
            PreferredLanguage = contact.PreferredLanguage,
            PreferredTimeZone = contact.PreferredTimeZone,
            LastLoginAt = contact.LastLoginAt,
            Permissions = new PortalPermissionsDto
            {
                CanCreateTickets = contact.CanCreateTickets,
                CanViewAllLocationTickets = contact.CanViewAllLocationTickets,
                CanViewAllCustomerTickets = contact.CanViewAllCustomerTickets,
                CanManageContacts = contact.CanManageContacts,
                CanViewContracts = contact.CanViewContracts,
                CanViewAssets = contact.CanViewAssets,
                CanApproveEscalations = contact.CanApproveEscalations,
                CanDownloadReports = contact.CanDownloadReports
            }
        };
    }

    #endregion
}
