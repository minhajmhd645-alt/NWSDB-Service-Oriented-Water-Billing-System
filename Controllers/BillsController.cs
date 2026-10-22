using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NWSDB.API.DTOs;
using NWSDB.API.DTOs.Bills;
using NWSDB.API.Interfaces;

namespace NWSDB.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class BillsController : ControllerBase
    {
        private readonly IBillService _billService;
        private readonly ILogger<BillsController> _logger;

        public BillsController(IBillService billService, ILogger<BillsController> logger)
        {
            _billService = billService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<ResponseDto<IEnumerable<BillDto>>>> GetAllBills()
        {
            _logger.LogInformation("Retrieving all bills");
            var bills = await _billService.GetAllBillsAsync();

            return Ok(new ResponseDto<IEnumerable<BillDto>>
            {
                Success = true,
                Message = "Bills retrieved successfully.",
                Data = bills
            });
        }

        [HttpGet("{accountNumber}")]
        public async Task<ActionResult<ResponseDto<IEnumerable<BillDto>>>> GetBills(string accountNumber)
        {
            _logger.LogInformation("Retrieving bills for account: {AccountNumber}", accountNumber);
            var bills = await _billService.GetBillsAsync(accountNumber);

            return Ok(new ResponseDto<IEnumerable<BillDto>>
            {
                Success = true,
                Message = "Bills retrieved successfully.",
                Data = bills
            });
        }

        [HttpGet("{accountNumber}/unpaid")]
        public async Task<ActionResult<ResponseDto<IEnumerable<BillDto>>>> GetUnpaidBills(string accountNumber)
        {
            _logger.LogInformation("Retrieving unpaid bills for account: {AccountNumber}", accountNumber);
            var bills = await _billService.GetUnpaidBillsAsync(accountNumber);

            return Ok(new ResponseDto<IEnumerable<BillDto>>
            {
                Success = true,
                Message = "Unpaid bills retrieved successfully.",
                Data = bills
            });
        }

        [HttpGet("id/{billId}")]
        public async Task<ActionResult<ResponseDto<BillDto>>> GetBill(int billId)
        {
            var bill = await _billService.GetBillAsync(billId);

            if (bill == null)
            {
                return NotFound(new ResponseDto<BillDto>
                {
                    Success = false,
                    Message = "Bill not found."
                });
            }

            return Ok(new ResponseDto<BillDto>
            {
                Success = true,
                Message = "Bill retrieved successfully.",
                Data = bill
            });
        }

        [HttpPost]
        public async Task<ActionResult<ResponseDto<BillDto>>> CreateBill([FromBody] CreateBillDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseDto<BillDto>
                {
                    Success = false,
                    Message = "Invalid request data."
                });
            }

            try
            {
                _logger.LogInformation("Creating new bill for account: {AccountNumber}", dto.AccountNumber);
                var createdBill = await _billService.CreateBillAsync(dto);

                return CreatedAtAction(
                    nameof(GetBill),
                    new { billId = createdBill.BillId },
                    new ResponseDto<BillDto>
                    {
                        Success = true,
                        Message = "Bill created successfully.",
                        Data = createdBill
                    });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ResponseDto<BillDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpPut("{billId}")]
        public async Task<ActionResult<ResponseDto<BillDto>>> UpdateBill(int billId, [FromBody] UpdateBillDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseDto<BillDto>
                {
                    Success = false,
                    Message = "Invalid request data."
                });
            }

            _logger.LogInformation("Updating bill: {BillId}", billId);
            var updatedBill = await _billService.UpdateBillAsync(billId, dto);

            if (updatedBill == null)
            {
                return NotFound(new ResponseDto<BillDto>
                {
                    Success = false,
                    Message = $"Bill with ID '{billId}' was not found."
                });
            }

            return Ok(new ResponseDto<BillDto>
            {
                Success = true,
                Message = "Bill updated successfully.",
                Data = updatedBill
            });
        }
    }
}
