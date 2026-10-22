using Bank.API.DTOs;
using Bank.API.DTOs.Accounts;
using Bank.API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Bank.API.Controllers
{
    /// <summary>
    /// Bank Accounts API – retrieves demo bank account details for verification and testing.
    /// </summary>
    [ApiController]
    [Route("api/v1/bank/accounts")]
    [Produces("application/json")]
    public class BankAccountsController : ControllerBase
    {
        private readonly IBankAccountService _accountService;
        private readonly ILogger<BankAccountsController> _logger;

        public BankAccountsController(
            IBankAccountService accountService,
            ILogger<BankAccountsController> logger)
        {
            _accountService = accountService;
            _logger = logger;
        }

        /// <summary>
        /// Retrieve demo bank account details by account number.
        /// </summary>
        /// <param name="accountNumber">The account number (e.g. DEMO-BANK-001).</param>
        /// <returns>Account information including holder name and available balance.</returns>
        /// <response code="200">Account found.</response>
        /// <response code="404">Account not found.</response>
        [HttpGet("{accountNumber}")]
        [ProducesResponseType(typeof(ResponseDto<BankAccountDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAccountByNumber(string accountNumber)
        {
            _logger.LogInformation("GET /api/v1/bank/accounts/{AccountNumber}", accountNumber);

            var account = await _accountService.GetAccountByNumberAsync(accountNumber);

            if (account == null)
            {
                return NotFound(new ResponseDto<object>
                {
                    Success = false,
                    Message = $"Bank account '{accountNumber}' was not found.",
                    Data    = null
                });
            }

            return Ok(new ResponseDto<BankAccountDto>
            {
                Success = true,
                Message = "Bank account retrieved successfully.",
                Data    = account
            });
        }

        /// <summary>
        /// Retrieve all demonstration bank accounts.
        /// </summary>
        /// <returns>List of demo bank accounts.</returns>
        /// <response code="200">Accounts retrieved successfully.</response>
        [HttpGet]
        [ProducesResponseType(typeof(ResponseDto<IEnumerable<BankAccountDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllAccounts()
        {
            _logger.LogInformation("GET /api/v1/bank/accounts");

            var accounts = await _accountService.GetAllAccountsAsync();

            return Ok(new ResponseDto<IEnumerable<BankAccountDto>>
            {
                Success = true,
                Message = "Bank accounts retrieved successfully.",
                Data    = accounts
            });
        }
    }
}
