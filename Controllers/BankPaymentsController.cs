using Bank.API.DTOs;
using Bank.API.DTOs.Payments;
using Bank.API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Bank.API.Controllers
{
    /// <summary>
    /// Bank Payments API – processes and retrieves demo payment transactions.
    /// </summary>
    [ApiController]
    [Route("api/v1/bank/payments")]
    [Produces("application/json")]
    public class BankPaymentsController : ControllerBase
    {
        private readonly IBankPaymentService _paymentService;
        private readonly ILogger<BankPaymentsController> _logger;

        public BankPaymentsController(
            IBankPaymentService paymentService,
            ILogger<BankPaymentsController> logger)
        {
            _paymentService = paymentService;
            _logger = logger;
        }

        /// <summary>
        /// Process a bank payment for a given demo account and bill reference.
        /// </summary>
        /// <param name="request">Payment details.</param>
        /// <returns>Transaction result including transaction ID and status.</returns>
        /// <response code="201">Payment processed successfully.</response>
        /// <response code="400">Validation failed (invalid account, inactive, insufficient balance, etc.).</response>
        /// <response code="409">Duplicate external reference detected.</response>
        /// <response code="500">Internal server error.</response>
        [HttpPost]
        [ProducesResponseType(typeof(ResponseDto<PaymentResultDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ProcessPayment([FromBody] PaymentRequestDto request)
        {
            _logger.LogInformation(
                "POST /api/v1/bank/payments – Account: {AccountNumber}, ExternalRef: {ExternalReference}",
                request.AccountNumber, request.ExternalReference);

            try
            {
                var result = await _paymentService.ProcessPaymentAsync(request);

                var response = new ResponseDto<PaymentResultDto>
                {
                    Success = true,
                    Message = "Payment processed successfully.",
                    Data    = result
                };

                return CreatedAtAction(nameof(GetTransactionById),
                    new { transactionId = result.TransactionId },
                    response);
            }
            catch (InvalidOperationException ex)
            {
                // Duplicate external reference
                _logger.LogWarning(ex, "Duplicate external reference: {ExternalReference}",
                    request.ExternalReference);

                return Conflict(new ResponseDto<object>
                {
                    Success = false,
                    Message = ex.Message,
                    Data    = null
                });
            }
            catch (ArgumentException ex)
            {
                // Validation failures (bad account, inactive, low balance, etc.)
                _logger.LogWarning(ex, "Payment validation failed.");

                return BadRequest(new ResponseDto<object>
                {
                    Success = false,
                    Message = ex.Message,
                    Data    = null
                });
            }
        }

        /// <summary>
        /// Retrieve a transaction by its bank transaction ID or external reference.
        /// </summary>
        /// <param name="transactionId">The bank transaction ID or external reference.</param>
        /// <returns>Full transaction details.</returns>
        /// <response code="200">Transaction found.</response>
        /// <response code="404">Transaction not found.</response>
        [HttpGet("{transactionId}")]
        [ProducesResponseType(typeof(ResponseDto<TransactionDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTransactionById(string transactionId)
        {
            _logger.LogInformation(
                "GET /api/v1/bank/payments/{TransactionId}", transactionId);

            var transaction = await _paymentService.GetTransactionByIdAsync(transactionId)
                ?? await _paymentService.GetTransactionByExternalReferenceAsync(transactionId);

            if (transaction == null)
            {
                return NotFound(new ResponseDto<object>
                {
                    Success = false,
                    Message = $"Transaction '{transactionId}' was not found.",
                    Data    = null
                });
            }

            return Ok(new ResponseDto<TransactionDto>
            {
                Success = true,
                Message = "Transaction retrieved successfully.",
                Data    = transaction
            });
        }

        /// <summary>
        /// Retrieve a transaction by its external reference (e.g. the reference supplied by NWSDB API).
        /// </summary>
        /// <param name="externalReference">The external reference string (e.g. NWSDB-PAY-001).</param>
        /// <returns>Full transaction details.</returns>
        /// <response code="200">Transaction found.</response>
        /// <response code="404">Transaction not found.</response>
        [HttpGet("reference/{externalReference}")]
        [ProducesResponseType(typeof(ResponseDto<TransactionDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTransactionByReference(string externalReference)
        {
            _logger.LogInformation(
                "GET /api/v1/bank/payments/reference/{ExternalReference}", externalReference);

            var transaction = await _paymentService
                .GetTransactionByExternalReferenceAsync(externalReference);

            if (transaction == null)
            {
                return NotFound(new ResponseDto<object>
                {
                    Success = false,
                    Message = $"No transaction found for external reference '{externalReference}'.",
                    Data    = null
                });
            }

            return Ok(new ResponseDto<TransactionDto>
            {
                Success = true,
                Message = "Transaction retrieved successfully.",
                Data    = transaction
            });
        }
    }
}
