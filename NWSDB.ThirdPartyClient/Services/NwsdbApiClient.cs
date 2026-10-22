using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NWSDB.ThirdPartyClient.Models;

namespace NWSDB.ThirdPartyClient.Services
{
    public class NwsdbApiClient : INwsdbApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<NwsdbApiClient> _logger;
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public NwsdbApiClient(HttpClient httpClient, ILogger<NwsdbApiClient> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ResponseDto<CustomerDto>?> GetCustomerAsync(string accountNumber, CancellationToken cancellationToken = default)
        {
            return await GetAsync<CustomerDto>($"/api/v1/customers/{Uri.EscapeDataString(accountNumber)}", cancellationToken);
        }

        public async Task<ResponseDto<CustomerSummaryDto>?> GetCustomerSummaryAsync(string accountNumber, CancellationToken cancellationToken = default)
        {
            return await GetAsync<CustomerSummaryDto>($"/api/v1/customers/{Uri.EscapeDataString(accountNumber)}/summary", cancellationToken);
        }

        public async Task<ResponseDto<IEnumerable<BillDto>>?> GetBillsAsync(string accountNumber, CancellationToken cancellationToken = default)
        {
            return await GetAsync<IEnumerable<BillDto>>($"/api/v1/bills/{Uri.EscapeDataString(accountNumber)}", cancellationToken);
        }

        public async Task<ResponseDto<IEnumerable<BillDto>>?> GetUnpaidBillsAsync(string accountNumber, CancellationToken cancellationToken = default)
        {
            return await GetAsync<IEnumerable<BillDto>>($"/api/v1/bills/{Uri.EscapeDataString(accountNumber)}/unpaid", cancellationToken);
        }

        public async Task<ResponseDto<WaterUsageDto>?> GetCurrentUsageAsync(string accountNumber, CancellationToken cancellationToken = default)
        {
            return await GetAsync<WaterUsageDto>($"/api/v1/usage/{Uri.EscapeDataString(accountNumber)}/current", cancellationToken);
        }

        public async Task<ResponseDto<IEnumerable<PaymentDto>>?> GetPaymentsAsync(string accountNumber, CancellationToken cancellationToken = default)
        {
            return await GetAsync<IEnumerable<PaymentDto>>($"/api/v1/payments/{Uri.EscapeDataString(accountNumber)}", cancellationToken);
        }

        public async Task<ResponseDto<PaymentDto>?> ProcessPaymentAsync(PaymentRequestDto request, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Sending payment request to NWSDB API for BillId: {BillId}", request.BillId);
                var response = await _httpClient.PostAsJsonAsync("/api/v1/payments", request, _jsonOptions, cancellationToken);
                
                var content = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!string.IsNullOrWhiteSpace(content))
                {
                    try
                    {
                        var result = JsonSerializer.Deserialize<ResponseDto<PaymentDto>>(content, _jsonOptions);
                        if (result != null)
                        {
                            return result;
                        }
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogWarning(ex, "Failed to deserialize JSON payment response content.");
                    }
                }

                if (response.IsSuccessStatusCode)
                {
                    return new ResponseDto<PaymentDto>
                    {
                        Success = true,
                        Message = "Payment processed successfully."
                    };
                }

                return new ResponseDto<PaymentDto>
                {
                    Success = false,
                    Message = $"Payment failed with HTTP status code {(int)response.StatusCode} ({response.ReasonPhrase})."
                };
            }
            catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "Timeout occurred during payment request to NWSDB API.");
                return new ResponseDto<PaymentDto>
                {
                    Success = false,
                    Message = "Request timed out while communicating with NWSDB API. Please try again."
                };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP connection error during payment request to NWSDB API.");
                return new ResponseDto<PaymentDto>
                {
                    Success = false,
                    Message = "NWSDB services are temporarily unavailable. Please try again later."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during payment request to NWSDB API.");
                return new ResponseDto<PaymentDto>
                {
                    Success = false,
                    Message = "An unexpected error occurred while communicating with NWSDB services."
                };
            }
        }

        private async Task<ResponseDto<T>?> GetAsync<T>(string endpoint, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Executing GET request to {Endpoint}", endpoint);
                var response = await _httpClient.GetAsync(endpoint, cancellationToken);
                var content = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!string.IsNullOrWhiteSpace(content))
                {
                    try
                    {
                        var result = JsonSerializer.Deserialize<ResponseDto<T>>(content, _jsonOptions);
                        if (result != null)
                        {
                            return result;
                        }
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogWarning(ex, "Failed to deserialize GET response content from {Endpoint}.", endpoint);
                    }
                }

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return new ResponseDto<T>
                    {
                        Success = false,
                        Message = "Customer account was not found."
                    };
                }

                return new ResponseDto<T>
                {
                    Success = false,
                    Message = $"Request failed with HTTP status code {(int)response.StatusCode}."
                };
            }
            catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "Timeout occurred while requesting {Endpoint}", endpoint);
                return new ResponseDto<T>
                {
                    Success = false,
                    Message = "Request timed out while communicating with NWSDB API."
                };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP connection error while requesting {Endpoint}", endpoint);
                return new ResponseDto<T>
                {
                    Success = false,
                    Message = "NWSDB services are temporarily unavailable. Please try again later."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while requesting {Endpoint}", endpoint);
                return new ResponseDto<T>
                {
                    Success = false,
                    Message = "An unexpected error occurred while communicating with NWSDB services."
                };
            }
        }
    }
}
