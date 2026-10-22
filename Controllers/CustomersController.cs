using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NWSDB.API.DTOs;
using NWSDB.API.DTOs.Customers;
using NWSDB.API.Interfaces;

namespace NWSDB.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerService _customerService;
        private readonly ILogger<CustomersController> _logger;

        public CustomersController(ICustomerService customerService, ILogger<CustomersController> logger)
        {
            _customerService = customerService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<ResponseDto<IEnumerable<CustomerDto>>>> GetAllCustomers()
        {
            _logger.LogInformation("Retrieving all customers");
            var customers = await _customerService.GetAllCustomersAsync();

            return Ok(new ResponseDto<IEnumerable<CustomerDto>>
            {
                Success = true,
                Message = "Customers retrieved successfully.",
                Data = customers
            });
        }

        [HttpGet("{accountNumber}")]
        public async Task<ActionResult<ResponseDto<CustomerDto>>> GetCustomer(string accountNumber)
        {
            _logger.LogInformation("Retrieving customer details for account: {AccountNumber}", accountNumber);
            var customer = await _customerService.GetCustomerAsync(accountNumber);

            if (customer == null)
            {
                return NotFound(new ResponseDto<CustomerDto>
                {
                    Success = false,
                    Message = "Customer not found."
                });
            }

            return Ok(new ResponseDto<CustomerDto>
            {
                Success = true,
                Message = "Customer retrieved successfully.",
                Data = customer
            });
        }

        [HttpGet("{accountNumber}/summary")]
        public async Task<ActionResult<ResponseDto<CustomerSummaryDto>>> GetCustomerSummary(string accountNumber)
        {
            _logger.LogInformation("Retrieving customer summary for account: {AccountNumber}", accountNumber);
            var summary = await _customerService.GetCustomerSummaryAsync(accountNumber);

            if (summary == null)
            {
                return NotFound(new ResponseDto<CustomerSummaryDto>
                {
                    Success = false,
                    Message = "Customer not found."
                });
            }

            return Ok(new ResponseDto<CustomerSummaryDto>
            {
                Success = true,
                Message = "Customer summary retrieved successfully.",
                Data = summary
            });
        }

        [HttpGet("next-account-number")]
        public async Task<ActionResult<ResponseDto<string>>> GetNextAccountNumber()
        {
            var nextAcc = await _customerService.GetNextAccountNumberAsync();
            return Ok(new ResponseDto<string>
            {
                Success = true,
                Message = "Next account number generated successfully.",
                Data = nextAcc
            });
        }

        [HttpPost]
        public async Task<ActionResult<ResponseDto<CustomerDto>>> CreateCustomer([FromBody] CreateCustomerDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseDto<CustomerDto>
                {
                    Success = false,
                    Message = "Invalid request data."
                });
            }

            if (!string.IsNullOrWhiteSpace(dto.AccountNumber))
            {
                var existingCustomer = await _customerService.GetCustomerAsync(dto.AccountNumber);
                if (existingCustomer != null)
                {
                    return Conflict(new ResponseDto<CustomerDto>
                    {
                        Success = false,
                        Message = $"Customer with account number '{dto.AccountNumber}' already exists."
                    });
                }
            }

            _logger.LogInformation("Creating new customer");
            var createdCustomer = await _customerService.CreateCustomerAsync(dto);

            return CreatedAtAction(
                nameof(GetCustomer),
                new { accountNumber = createdCustomer.AccountNumber },
                new ResponseDto<CustomerDto>
                {
                    Success = true,
                    Message = "Customer created successfully.",
                    Data = createdCustomer
                });
        }

        [HttpPut("{accountNumber}")]
        public async Task<ActionResult<ResponseDto<CustomerDto>>> UpdateCustomer(string accountNumber, [FromBody] UpdateCustomerDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseDto<CustomerDto>
                {
                    Success = false,
                    Message = "Invalid request data."
                });
            }

            _logger.LogInformation("Updating customer details for account: {AccountNumber}", accountNumber);

            var updatedCustomer = await _customerService.UpdateCustomerAsync(accountNumber, dto);
            if (updatedCustomer == null)
            {
                return NotFound(new ResponseDto<CustomerDto>
                {
                    Success = false,
                    Message = $"Customer with account number '{accountNumber}' was not found."
                });
            }

            return Ok(new ResponseDto<CustomerDto>
            {
                Success = true,
                Message = "Customer updated successfully.",
                Data = updatedCustomer
            });
        }
    }
}
