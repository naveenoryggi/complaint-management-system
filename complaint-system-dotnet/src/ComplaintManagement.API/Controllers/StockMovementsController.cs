using ComplaintManagement.Application.DTOs.Asset;
using ComplaintManagement.Application.Interfaces.Services;
using ComplaintManagement.Domain.Entities.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ComplaintManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StockMovementsController : ControllerBase
{
    private readonly IStockMovementService _stockMovementService;

    public StockMovementsController(IStockMovementService stockMovementService)
    {
        _stockMovementService = stockMovementService;
    }

    private Guid GetCompanyId() =>
        Guid.Parse(User.FindFirstValue("CompanyId") ?? throw new UnauthorizedAccessException("Company ID not found in token"));

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException("User ID not found in token"));

    /// <summary>
    /// Get all stock movements with pagination and filtering
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] StockMovementType? movementType = null,
        [FromQuery] StockMovementStatus? status = null,
        [FromQuery] Guid? productId = null,
        [FromQuery] Guid? locationId = null,
        [FromQuery] Guid? stockCategoryId = null,
        [FromQuery] Guid? customerId = null,
        [FromQuery] string? searchTerm = null)
    {
        var filter = new StockMovementFilterRequest
        {
            FromDate = fromDate,
            ToDate = toDate,
            MovementType = movementType,
            Status = status,
            ProductId = productId,
            LocationId = locationId,
            StockCategoryId = stockCategoryId,
            CustomerId = customerId,
            SearchTerm = searchTerm
        };

        var result = await _stockMovementService.GetAllAsync(GetCompanyId(), filter, page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Get a single stock movement by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _stockMovementService.GetByIdAsync(id, GetCompanyId());
        if (!result.IsSuccess)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Get stock movement by movement number
    /// </summary>
    [HttpGet("by-number/{movementNumber}")]
    public async Task<IActionResult> GetByMovementNumber(string movementNumber)
    {
        var result = await _stockMovementService.GetByMovementNumberAsync(movementNumber, GetCompanyId());
        if (!result.IsSuccess)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Get movements for a specific stock item
    /// </summary>
    [HttpGet("by-stock-item/{stockItemId}")]
    public async Task<IActionResult> GetByStockItem(Guid stockItemId, [FromQuery] int limit = 50)
    {
        var result = await _stockMovementService.GetByStockItemAsync(stockItemId, GetCompanyId(), limit);
        return Ok(result);
    }

    /// <summary>
    /// Get movements for a specific asset
    /// </summary>
    [HttpGet("by-asset/{assetId}")]
    public async Task<IActionResult> GetByAsset(Guid assetId, [FromQuery] int limit = 50)
    {
        var result = await _stockMovementService.GetByAssetAsync(assetId, GetCompanyId(), limit);
        return Ok(result);
    }

    /// <summary>
    /// Get movements for a specific product
    /// </summary>
    [HttpGet("by-product/{productId}")]
    public async Task<IActionResult> GetByProduct(Guid productId, [FromQuery] int limit = 50)
    {
        var result = await _stockMovementService.GetByProductAsync(productId, GetCompanyId(), limit);
        return Ok(result);
    }

    /// <summary>
    /// Create a new stock movement
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateStockMovementRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _stockMovementService.CreateAsync(request, GetCompanyId(), GetUserId());
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    /// <summary>
    /// Approve a pending stock movement
    /// </summary>
    [HttpPost("{id}/approve")]
    public async Task<IActionResult> Approve(Guid id)
    {
        var result = await _stockMovementService.ApproveAsync(id, GetCompanyId(), GetUserId());
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Reject a pending stock movement
    /// </summary>
    [HttpPost("{id}/reject")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] RejectRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Reason))
        {
            return BadRequest("Reason is required for rejection");
        }

        var result = await _stockMovementService.RejectAsync(id, request.Reason, GetCompanyId(), GetUserId());
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Cancel a draft or pending stock movement
    /// </summary>
    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Reason))
        {
            return BadRequest("Reason is required for cancellation");
        }

        var result = await _stockMovementService.CancelAsync(id, request.Reason, GetCompanyId(), GetUserId());
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Reverse a completed stock movement
    /// </summary>
    [HttpPost("{id}/reverse")]
    public async Task<IActionResult> Reverse(Guid id, [FromBody] ReverseRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Reason))
        {
            return BadRequest("Reason is required for reversal");
        }

        var result = await _stockMovementService.ReverseAsync(id, request.Reason, GetCompanyId(), GetUserId());
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Get available movement types for dropdown
    /// </summary>
    [HttpGet("movement-types")]
    public IActionResult GetMovementTypes()
    {
        var types = Enum.GetValues<StockMovementType>()
            .Select(t => new
            {
                Value = (int)t,
                Name = t.ToString(),
                Label = GetMovementTypeLabel(t),
                Category = GetMovementCategory(t)
            })
            .ToList();

        return Ok(types);
    }

    /// <summary>
    /// Get available movement statuses for dropdown
    /// </summary>
    [HttpGet("statuses")]
    public IActionResult GetStatuses()
    {
        var statuses = Enum.GetValues<StockMovementStatus>()
            .Select(s => new
            {
                Value = (int)s,
                Name = s.ToString()
            })
            .ToList();

        return Ok(statuses);
    }

    private static string GetMovementTypeLabel(StockMovementType type)
    {
        return type switch
        {
            StockMovementType.Receipt => "Receipt (Purchase)",
            StockMovementType.Return => "Customer Return",
            StockMovementType.TransferIn => "Transfer In",
            StockMovementType.Production => "Production Output",
            StockMovementType.Adjustment_In => "Adjustment (+)",
            StockMovementType.Issue => "Issue to Customer/Project",
            StockMovementType.Sale => "Sale",
            StockMovementType.TransferOut => "Transfer Out",
            StockMovementType.Consumption => "Consumption",
            StockMovementType.Adjustment_Out => "Adjustment (-)",
            StockMovementType.Scrap => "Scrap/Dispose",
            StockMovementType.Loss => "Loss/Theft",
            StockMovementType.CategoryChange => "Category Change",
            StockMovementType.Reservation => "Reserve Stock",
            StockMovementType.Unreservation => "Release Reservation",
            StockMovementType.StockCount => "Stock Count",
            StockMovementType.Revaluation => "Cost Revaluation",
            _ => type.ToString()
        };
    }

    private static string GetMovementCategory(StockMovementType type)
    {
        return type switch
        {
            StockMovementType.Receipt or StockMovementType.Return or StockMovementType.TransferIn
                or StockMovementType.Production or StockMovementType.Adjustment_In => "Incoming",
            StockMovementType.Issue or StockMovementType.Sale or StockMovementType.TransferOut
                or StockMovementType.Consumption or StockMovementType.Adjustment_Out
                or StockMovementType.Scrap or StockMovementType.Loss => "Outgoing",
            StockMovementType.CategoryChange or StockMovementType.Reservation
                or StockMovementType.Unreservation => "Status Change",
            StockMovementType.StockCount or StockMovementType.Revaluation => "Inventory",
            _ => "Other"
        };
    }
}

public class RejectRequest
{
    public string Reason { get; set; } = string.Empty;
}

public class CancelRequest
{
    public string Reason { get; set; } = string.Empty;
}

public class ReverseRequest
{
    public string Reason { get; set; } = string.Empty;
}
