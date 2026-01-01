using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.Portal;

namespace ComplaintManagement.Application.Interfaces.Services;

/// <summary>
/// Service interface for customer portal operations
/// </summary>
public interface IPortalService
{
    #region Authentication

    /// <summary>
    /// Authenticates a portal user
    /// </summary>
    Task<PortalLoginResponse> LoginAsync(PortalLoginRequest request, string? ipAddress = null);

    /// <summary>
    /// Refreshes portal access token
    /// </summary>
    Task<PortalLoginResponse> RefreshTokenAsync(PortalRefreshTokenRequest request, string? ipAddress = null);

    /// <summary>
    /// Logs out a portal user
    /// </summary>
    Task<Result> LogoutAsync(Guid contactId, string? refreshToken = null);

    /// <summary>
    /// Changes portal user password
    /// </summary>
    Task<Result> ChangePasswordAsync(Guid contactId, PortalChangePasswordRequest request);

    /// <summary>
    /// Initiates forgot password flow
    /// </summary>
    Task<Result> ForgotPasswordAsync(PortalForgotPasswordRequest request);

    /// <summary>
    /// Resets password using token
    /// </summary>
    Task<Result> ResetPasswordAsync(PortalResetPasswordRequest request);

    /// <summary>
    /// Gets current portal user profile
    /// </summary>
    Task<PortalUserDto?> GetProfileAsync(Guid contactId);

    /// <summary>
    /// Updates portal user profile
    /// </summary>
    Task<Result<PortalUserDto>> UpdateProfileAsync(Guid contactId, UpdatePortalProfileRequest request);

    #endregion

    #region Dashboard

    /// <summary>
    /// Gets portal dashboard data
    /// </summary>
    Task<PortalDashboardDto> GetDashboardAsync(Guid contactId);

    #endregion

    #region Tickets

    /// <summary>
    /// Gets paginated list of tickets visible to the portal user
    /// </summary>
    Task<PagedResult<PortalTicketSummaryDto>> GetTicketsAsync(Guid contactId, PortalTicketListQuery query);

    /// <summary>
    /// Gets ticket details
    /// </summary>
    Task<PortalTicketDetailDto?> GetTicketByIdAsync(Guid contactId, Guid ticketId);

    /// <summary>
    /// Gets ticket by complaint number
    /// </summary>
    Task<PortalTicketDetailDto?> GetTicketByNumberAsync(Guid contactId, string complaintNumber);

    /// <summary>
    /// Creates a new ticket from portal
    /// </summary>
    Task<Result<PortalTicketDetailDto>> CreateTicketAsync(Guid contactId, PortalCreateTicketRequest request);

    /// <summary>
    /// Adds a comment to a ticket
    /// </summary>
    Task<Result<PortalTicketCommentDto>> AddCommentAsync(Guid contactId, Guid ticketId, PortalAddCommentRequest request);

    /// <summary>
    /// Rates a resolved/closed ticket
    /// </summary>
    Task<Result> RateTicketAsync(Guid contactId, Guid ticketId, PortalRateTicketRequest request);

    /// <summary>
    /// Reopens a closed ticket
    /// </summary>
    Task<Result<PortalTicketDetailDto>> ReopenTicketAsync(Guid contactId, Guid ticketId, PortalReopenTicketRequest request);

    #endregion

    #region Assets

    /// <summary>
    /// Gets assets visible to the portal user
    /// </summary>
    Task<PagedResult<PortalAssetSummaryDto>> GetAssetsAsync(Guid contactId, int page = 1, int pageSize = 20, string? searchTerm = null);

    /// <summary>
    /// Gets asset details
    /// </summary>
    Task<PortalAssetDetailDto?> GetAssetByIdAsync(Guid contactId, Guid assetId);

    /// <summary>
    /// Gets assets at a specific location
    /// </summary>
    Task<List<PortalAssetSummaryDto>> GetAssetsByLocationAsync(Guid contactId, Guid locationId);

    #endregion

    #region Contracts

    /// <summary>
    /// Gets contracts visible to the portal user
    /// </summary>
    Task<PagedResult<PortalContractSummaryDto>> GetContractsAsync(Guid contactId, int page = 1, int pageSize = 20, bool? activeOnly = true);

    /// <summary>
    /// Gets contract details
    /// </summary>
    Task<PortalContractDetailDto?> GetContractByIdAsync(Guid contactId, Guid contractId);

    #endregion

    #region Locations

    /// <summary>
    /// Gets locations visible to the portal user
    /// </summary>
    Task<List<PortalLocationSummaryDto>> GetLocationsAsync(Guid contactId);

    #endregion

    #region Lookup Data

    /// <summary>
    /// Gets categories for ticket creation
    /// </summary>
    Task<List<PortalCategoryDto>> GetCategoriesAsync(Guid contactId);

    /// <summary>
    /// Gets priorities for ticket creation
    /// </summary>
    Task<List<PortalPriorityDto>> GetPrioritiesAsync(Guid contactId);

    /// <summary>
    /// Gets statuses for filtering
    /// </summary>
    Task<List<PortalStatusDto>> GetStatusesAsync(Guid contactId);

    /// <summary>
    /// Gets assets for ticket creation (dropdown)
    /// </summary>
    Task<List<PortalAssetSummaryDto>> GetAssetLookupsAsync(Guid contactId, Guid? locationId = null);

    /// <summary>
    /// Gets product categories for ticket creation (dropdown)
    /// </summary>
    Task<List<PortalProductCategoryDto>> GetProductCategoriesAsync(Guid contactId, Guid? parentId = null);

    /// <summary>
    /// Gets products for ticket creation (dropdown)
    /// </summary>
    Task<List<PortalProductDto>> GetProductsAsync(Guid contactId, Guid? categoryId = null, Guid? brandId = null);

    /// <summary>
    /// Gets brands (OEM) for ticket creation (dropdown)
    /// </summary>
    Task<List<PortalBrandDto>> GetBrandsAsync(Guid contactId);

    #endregion

    #region Validation

    /// <summary>
    /// Checks if contact has access to view the ticket
    /// </summary>
    Task<bool> CanAccessTicketAsync(Guid contactId, Guid ticketId);

    /// <summary>
    /// Checks if contact has access to view the asset
    /// </summary>
    Task<bool> CanAccessAssetAsync(Guid contactId, Guid assetId);

    /// <summary>
    /// Checks if contact has access to view the contract
    /// </summary>
    Task<bool> CanAccessContractAsync(Guid contactId, Guid contractId);

    #endregion
}
