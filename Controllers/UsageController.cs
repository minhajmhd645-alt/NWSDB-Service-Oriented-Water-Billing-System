using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NWSDB.API.DTOs;
using NWSDB.API.DTOs.WaterUsage;
using NWSDB.API.Interfaces;

namespace NWSDB.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class UsageController : ControllerBase
    {
        private readonly IWaterUsageService _usageService;
        private readonly ILogger<UsageController> _logger;

        public UsageController(IWaterUsageService usageService, ILogger<UsageController> logger)
        {
            _usageService = usageService;
            _logger = logger;
        }

        [HttpGet("{accountNumber}")]
        public async Task<ActionResult<ResponseDto<IEnumerable<WaterUsageDto>>>> GetUsageHistory(string accountNumber)
        {
            _logger.LogInformation("Retrieving usage history for account: {AccountNumber}", accountNumber);
            var history = await _usageService.GetUsageHistoryAsync(accountNumber);

            return Ok(new ResponseDto<IEnumerable<WaterUsageDto>>
            {
                Success = true,
                Message = "Usage history retrieved successfully.",
                Data = history
            });
        }

        [HttpGet("{accountNumber}/current")]
        public async Task<ActionResult<ResponseDto<WaterUsageDto>>> GetCurrentUsage(string accountNumber)
        {
            _logger.LogInformation("Retrieving current usage for account: {AccountNumber}", accountNumber);
            var usage = await _usageService.GetCurrentUsageAsync(accountNumber);

            if (usage == null)
            {
                return NotFound(new ResponseDto<WaterUsageDto>
                {
                    Success = false,
                    Message = "Current usage not found."
                });
            }

            return Ok(new ResponseDto<WaterUsageDto>
            {
                Success = true,
                Message = "Current usage retrieved successfully.",
                Data = usage
            });
        }
    }
}
