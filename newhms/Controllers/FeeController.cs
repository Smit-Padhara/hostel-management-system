using Microsoft.AspNetCore.Mvc;
using SmartHostelManagementSystem.DTOs.Requests;
using SmartHostelManagementSystem.DTOs.Responses;
using SmartHostelManagementSystem.Services.Interfaces;

namespace SmartHostelManagementSystem.Controllers;

/// <summary>
/// API Controller for fee and payment management
/// Handles fee tracking, payment recording, and receipt generation
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class FeeController : ControllerBase
{
    private readonly IFeeService _feeService;
    private readonly ILogger<FeeController> _logger;

    public FeeController(IFeeService feeService, ILogger<FeeController> logger)
    {
        _feeService = feeService;
        _logger = logger;
    }

    /// <summary>
    /// Record a new fee for a student
    /// POST: api/fee
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(FeeDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<FeeDto>> RecordFee([FromBody] RecordFeeRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var fee = await _feeService.RecordFeeAsync(request);
            _logger.LogInformation($"Fee recorded for student {request.StudentId}: ₹{request.Amount}");
            
            return CreatedAtAction(nameof(GetFeeById), new { id = fee.FeeId }, fee);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning($"Failed to record fee: {ex.Message}");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error recording fee: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while recording the fee" });
        }
    }

    /// <summary>
    /// Get fee by ID
    /// GET: api/fee/{id}
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(FeeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FeeDto>> GetFeeById(int id)
    {
        try
        {
            var fee = await _feeService.GetFeeByIdAsync(id);
            
            if (fee == null)
                return NotFound(new { message = $"Fee with ID {id} not found" });

            return Ok(fee);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error fetching fee: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while fetching the fee" });
        }
    }

    /// <summary>
    /// Get all fees for a student
    /// GET: api/fee/student/{studentId}
    /// </summary>
    [HttpGet("student/{studentId}")]
    [ProducesResponseType(typeof(IEnumerable<FeeDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<FeeDto>>> GetFeesByStudent(int studentId)
    {
        try
        {
            var fees = await _feeService.GetFeesByStudentAsync(studentId);
            return Ok(fees);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error fetching fees for student: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while fetching fees" });
        }
    }

    /// <summary>
    /// Get all fees in a hostel
    /// GET: api/fee/hostel/{hostelId}
    /// </summary>
    [HttpGet("hostel/{hostelId}")]
    [ProducesResponseType(typeof(IEnumerable<FeeDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<FeeDto>>> GetFeesByHostel(int hostelId)
    {
        try
        {
            var fees = await _feeService.GetFeesByHostelAsync(hostelId);
            return Ok(fees);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error fetching fees for hostel: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while fetching fees" });
        }
    }

    /// <summary>
    /// Get pending fees in a hostel
    /// GET: api/fee/pending/{hostelId}
    /// </summary>
    [HttpGet("pending/{hostelId}")]
    [ProducesResponseType(typeof(IEnumerable<FeeDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<FeeDto>>> GetPendingFees(int hostelId)
    {
        try
        {
            var fees = await _feeService.GetPendingFeesAsync(hostelId);
            return Ok(fees);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error fetching pending fees: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while fetching pending fees" });
        }
    }

    /// <summary>
    /// Get overdue fees in a hostel
    /// GET: api/fee/overdue/{hostelId}
    /// </summary>
    [HttpGet("overdue/{hostelId}")]
    [ProducesResponseType(typeof(IEnumerable<FeeDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<FeeDto>>> GetOverdueFees(int hostelId)
    {
        try
        {
            var fees = await _feeService.GetOverdueFeesAsync(hostelId);
            return Ok(fees);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error fetching overdue fees: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while fetching overdue fees" });
        }
    }

    /// <summary>
    /// Record a payment for a fee
    /// POST: api/fee/{id}/payment
    /// </summary>
    [HttpPost("{id}/payment")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> RecordPayment(
        int id, 
        [FromQuery] decimal amount,
        [FromQuery] DateTime? paymentDate = null)
    {
        try
        {
            if (amount <= 0)
                return BadRequest(new { message = "Payment amount must be greater than 0" });

            var date = paymentDate ?? DateTime.UtcNow;
            var (feeDto, receiptNumber) = await _feeService.RecordPaymentAsync(id, amount, date);

            _logger.LogInformation($"Payment recorded for fee {id}: ₹{amount}, Receipt: {receiptNumber}");

            return Ok(new 
            { 
                fee = feeDto,
                receipt = receiptNumber,
                message = "Payment recorded successfully"
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning($"Failed to record payment: {ex.Message}");
            return BadRequest(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error recording payment: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while recording the payment" });
        }
    }

    /// <summary>
    /// Get total pending fees amount for a hostel
    /// GET: api/fee/total-pending/{hostelId}
    /// </summary>
    [HttpGet("total-pending/{hostelId}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<ActionResult> GetTotalPendingFees(int hostelId)
    {
        try
        {
            var total = await _feeService.GetTotalPendingFeesAsync(hostelId);
            return Ok(new { hostelId = hostelId, totalPendingFees = total });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting total pending fees: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred" });
        }
    }

    /// <summary>
    /// Get total fees collected for a hostel
    /// GET: api/fee/total-collected/{hostelId}
    /// </summary>
    [HttpGet("total-collected/{hostelId}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<ActionResult> GetTotalCollected(int hostelId)
    {
        try
        {
            var total = await _feeService.GetTotalFeesCollectedAsync(hostelId);
            return Ok(new { hostelId = hostelId, totalCollected = total });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting total collected fees: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred" });
        }
    }

    /// <summary>
    /// Get count of students with pending fees
    /// GET: api/fee/students-with-pending/{hostelId}
    /// </summary>
    [HttpGet("students-with-pending/{hostelId}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<ActionResult> GetStudentsWithPendingFees(int hostelId)
    {
        try
        {
            var count = await _feeService.GetStudentsWithPendingFeesCountAsync(hostelId);
            return Ok(new { hostelId = hostelId, studentsWithPendingFees = count });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting students with pending fees: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred" });
        }
    }

    /// <summary>
    /// Generate receipt for a payment
    /// GET: api/fee/{id}/receipt?amount=500&date=2024-01-15
    /// </summary>
    [HttpGet("{id}/receipt")]
    [ProducesResponseType(typeof(Dictionary<string, object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Dictionary<string, object>>> GenerateReceipt(
        int id, 
        [FromQuery] decimal amount,
        [FromQuery] DateTime? date = null)
    {
        try
        {
            if (amount <= 0)
                return BadRequest(new { message = "Amount must be greater than 0" });

            var paymentDate = date ?? DateTime.UtcNow;
            var receipt = await _feeService.GenerateReceiptAsync(id, amount, paymentDate);

            _logger.LogInformation($"Receipt generated for fee {id}");
            return Ok(receipt);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error generating receipt: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while generating the receipt" });
        }
    }
}
