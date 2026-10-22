using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NWSDB.API.DTOs;
using NWSDB.API.DTOs.Payments;
using NWSDB.API.Exceptions;
using NWSDB.API.Interfaces;

namespace NWSDB.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly ILogger<PaymentsController> _logger;

        public PaymentsController(IPaymentService paymentService, ILogger<PaymentsController> logger)
        {
            _paymentService = paymentService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<ResponseDto<IEnumerable<PaymentDto>>>> GetAllPayments()
        {
            _logger.LogInformation("Retrieving all payment records");
            var payments = await _paymentService.GetAllPaymentsAsync();

            return Ok(new ResponseDto<IEnumerable<PaymentDto>>
            {
                Success = true,
                Message = "Payment records retrieved successfully.",
                Data = payments
            });
        }

        [HttpGet("{accountNumber}")]
        public async Task<ActionResult<ResponseDto<IEnumerable<PaymentDto>>>> GetPayments(string accountNumber)
        {
            _logger.LogInformation("Retrieving payments for account: {AccountNumber}", accountNumber);
            var payments = await _paymentService.GetPaymentsAsync(accountNumber);

            return Ok(new ResponseDto<IEnumerable<PaymentDto>>
            {
                Success = true,
                Message = "Payment history retrieved successfully.",
                Data = payments
            });
        }

        [HttpGet("id/{paymentId}")]
        public async Task<ActionResult<ResponseDto<PaymentDto>>> GetPayment(int paymentId)
        {
            var payment = await _paymentService.GetPaymentAsync(paymentId);

            if (payment == null)
            {
                return NotFound(new ResponseDto<PaymentDto>
                {
                    Success = false,
                    Message = "Payment not found."
                });
            }

            return Ok(new ResponseDto<PaymentDto>
            {
                Success = true,
                Message = "Payment retrieved successfully.",
                Data = payment
            });
        }

        [HttpPost]
        public async Task<ActionResult<ResponseDto<PaymentDto>>> CreatePayment([FromBody] PaymentRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseDto<PaymentDto>
                {
                    Success = false,
                    Message = "Invalid input."
                });
            }

            _logger.LogInformation("Processing payment for BillId: {BillId}, Amount: {Amount}", request.BillId, request.Amount);

            try
            {
                var payment = await _paymentService.CreatePaymentAsync(request);
                return CreatedAtAction(nameof(GetPayment), new { paymentId = payment.PaymentId }, new ResponseDto<PaymentDto>
                {
                    Success = true,
                    Message = "Payment created successfully.",
                    Data = payment
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ResponseDto<PaymentDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (BankApiException ex)
            {
                _logger.LogWarning(ex, "Bank API rejected payment for BillId: {BillId}. Status: {StatusCode}", request.BillId, ex.StatusCode);
                var statusCode = (int)ex.StatusCode;
                return StatusCode(statusCode, new ResponseDto<PaymentDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment for BillId: {BillId}", request.BillId);
                return StatusCode(500, new ResponseDto<PaymentDto>
                {
                    Success = false,
                    Message = "An error occurred while processing the payment."
                });
            }
        }
    }
}
