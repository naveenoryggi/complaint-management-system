using ComplaintManagement.API.Authorization;
using ComplaintManagement.Application.Interfaces.Repositories;
using ComplaintManagement.Infrastructure.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ComplaintManagement.API.Controllers;

[ApiController]
[Route("api/company")]
[Authorize]
public class CompanyController : ControllerBase
{
    private readonly ICompanyRepository _companyRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<CompanyController> _logger;

    public CompanyController(
        ICompanyRepository companyRepository,
        IUnitOfWork unitOfWork,
        IFileStorageService fileStorageService,
        ILogger<CompanyController> logger)
    {
        _companyRepository = companyRepository;
        _unitOfWork = unitOfWork;
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    /// <summary>
    /// Get all companies
    /// </summary>
    [HttpGet]
    [ResponseCache(Duration = 600, VaryByQueryKeys = new[] { "includeInactive" }, VaryByHeader = "Authorization")]
    public async Task<IActionResult> GetAllCompanies([FromQuery] bool includeInactive = false)
    {
        try
        {
            var companies = await _companyRepository.GetAllAsync();

            if (!includeInactive)
            {
                companies = companies.Where(c => c.IsActive).ToList();
            }

            var companiesData = companies.Select(company => new
            {
                company.Id,
                company.Name,
                company.Code,
                company.Description,
                company.ContactEmail,
                company.ContactPhone,
                company.Address,
                company.LogoUrl,
                company.IsActive,
                company.ManagerId,
                ManagerName = company.Manager != null ? $"{company.Manager.FirstName} {company.Manager.LastName}" : null,
                company.SecondaryManagerId,
                SecondaryManagerName = company.SecondaryManager != null ? $"{company.SecondaryManager.FirstName} {company.SecondaryManager.LastName}" : null,
                company.HrResponsibleId,
                HrResponsibleName = company.HrResponsible != null ? $"{company.HrResponsible.FirstName} {company.HrResponsible.LastName}" : null
            }).ToList();

            return Ok(new
            {
                isSuccess = true,
                data = companiesData,
                count = companiesData.Count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving companies");
            return StatusCode(500, new
            {
                isSuccess = false,
                message = "An error occurred while retrieving companies"
            });
        }
    }

    /// <summary>
    /// Get company details by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCompanyById(Guid id)
    {
        try
        {
            var company = await _companyRepository.GetCompanyWithManagersAsync(id);
            if (company == null)
            {
                return NotFound(new
                {
                    isSuccess = false,
                    message = "Company not found"
                });
            }

            return Ok(new
            {
                isSuccess = true,
                data = new
                {
                    company.Id,
                    company.Name,
                    company.Code,
                    company.Description,
                    company.ContactEmail,
                    company.ContactPhone,
                    company.Address,
                    company.LogoUrl,
                    company.LogoFileName,
                    company.IsActive,
                    company.ManagerId,
                    ManagerName = company.Manager != null ? $"{company.Manager.FirstName} {company.Manager.LastName}" : null,
                    company.SecondaryManagerId,
                    SecondaryManagerName = company.SecondaryManager != null ? $"{company.SecondaryManager.FirstName} {company.SecondaryManager.LastName}" : null,
                    company.HrResponsibleId,
                    HrResponsibleName = company.HrResponsible != null ? $"{company.HrResponsible.FirstName} {company.HrResponsible.LastName}" : null
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving company {CompanyId}", id);
            return StatusCode(500, new
            {
                isSuccess = false,
                message = "An error occurred while retrieving company details"
            });
        }
    }

    /// <summary>
    /// Upload company logo
    /// </summary>
    [HttpPost("{id}/logo")]
    [HasPermission("ManageCompany")]
    [RequestSizeLimit(5242880)] // 5MB
    public async Task<IActionResult> UploadLogo(Guid id, [FromForm] IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new
                {
                    isSuccess = false,
                    message = "File is required"
                });
            }

            // Validate file is an image
            if (!_fileStorageService.IsValidImage(file))
            {
                return BadRequest(new
                {
                    isSuccess = false,
                    message = "Invalid file. Please upload a valid image file (JPG, PNG, GIF, SVG, WEBP) with maximum size of 5MB"
                });
            }

            var company = await _companyRepository.GetByIdAsync(id);
            if (company == null)
            {
                return NotFound(new
                {
                    isSuccess = false,
                    message = "Company not found"
                });
            }

            // Delete old logo if exists
            if (!string.IsNullOrEmpty(company.LogoUrl))
            {
                await _fileStorageService.DeleteFileAsync(company.LogoUrl);
            }

            // Upload new logo
            var logoUrl = await _fileStorageService.UploadFileAsync(file, "company-logos");
            var contentType = _fileStorageService.GetContentType(file.FileName);

            // Update company with new logo
            company.LogoUrl = logoUrl;
            company.LogoFileName = file.FileName;
            company.LogoContentType = contentType;

            _companyRepository.Update(company);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Company logo uploaded successfully for company {CompanyId}", id);

            return Ok(new
            {
                isSuccess = true,
                message = "Company logo uploaded successfully",
                data = new
                {
                    logoUrl,
                    fileName = file.FileName,
                    contentType
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading company logo for company {CompanyId}", id);
            return StatusCode(500, new
            {
                isSuccess = false,
                message = "An error occurred while uploading the logo"
            });
        }
    }

    /// <summary>
    /// Delete company logo
    /// </summary>
    [HttpDelete("{id}/logo")]
    [HasPermission("ManageCompany")]
    public async Task<IActionResult> DeleteLogo(Guid id)
    {
        try
        {
            var company = await _companyRepository.GetByIdAsync(id);
            if (company == null)
            {
                return NotFound(new
                {
                    isSuccess = false,
                    message = "Company not found"
                });
            }

            if (string.IsNullOrEmpty(company.LogoUrl))
            {
                return BadRequest(new
                {
                    isSuccess = false,
                    message = "Company has no logo to delete"
                });
            }

            // Delete logo file
            await _fileStorageService.DeleteFileAsync(company.LogoUrl);

            // Update company - remove logo
            company.LogoUrl = null;
            company.LogoFileName = null;
            company.LogoContentType = null;

            _companyRepository.Update(company);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Company logo deleted successfully for company {CompanyId}", id);

            return Ok(new
            {
                isSuccess = true,
                message = "Company logo deleted successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting company logo for company {CompanyId}", id);
            return StatusCode(500, new
            {
                isSuccess = false,
                message = "An error occurred while deleting the logo"
            });
        }
    }

    /// <summary>
    /// Update company details
    /// </summary>
    [HttpPut("{id}")]
    [HasPermission("ManageCompany")]
    public async Task<IActionResult> UpdateCompany(Guid id, [FromBody] UpdateCompanyRequest request)
    {
        try
        {
            var company = await _companyRepository.GetByIdAsync(id);
            if (company == null)
            {
                return NotFound(new
                {
                    isSuccess = false,
                    message = "Company not found"
                });
            }

            // Update company details
            company.Name = request.Name;
            company.Description = request.Description;
            company.ContactEmail = request.ContactEmail;
            company.ContactPhone = request.ContactPhone;
            company.Address = request.Address;
            company.ManagerId = request.ManagerId;
            company.SecondaryManagerId = request.SecondaryManagerId;
            company.HrResponsibleId = request.HrResponsibleId;

            _companyRepository.Update(company);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Company updated successfully: {CompanyId}", id);

            return Ok(new
            {
                isSuccess = true,
                message = "Company updated successfully",
                data = new
                {
                    company.Id,
                    company.Name,
                    company.Description,
                    company.ContactEmail,
                    company.ContactPhone,
                    company.Address,
                    company.LogoUrl
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating company {CompanyId}", id);
            return StatusCode(500, new
            {
                isSuccess = false,
                message = "An error occurred while updating the company"
            });
        }
    }

    /// <summary>
    /// Get company settings
    /// </summary>
    [HttpGet("{id}/settings")]
    public async Task<IActionResult> GetCompanySettings(Guid id)
    {
        try
        {
            var company = await _companyRepository.GetByIdAsync(id);
            if (company == null)
            {
                return NotFound(new
                {
                    isSuccess = false,
                    message = "Company not found"
                });
            }

            return Ok(new
            {
                isSuccess = true,
                data = new
                {
                    companyId = company.Id,
                    companyName = company.Name,
                    isActive = company.IsActive,
                    contactEmail = company.ContactEmail,
                    contactPhone = company.ContactPhone
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving company settings for {CompanyId}", id);
            return StatusCode(500, new
            {
                isSuccess = false,
                message = "An error occurred while retrieving company settings"
            });
        }
    }
}

public class UpdateCompanyRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? Address { get; set; }
    public Guid? ManagerId { get; set; }
    public Guid? SecondaryManagerId { get; set; }
    public Guid? HrResponsibleId { get; set; }
}
