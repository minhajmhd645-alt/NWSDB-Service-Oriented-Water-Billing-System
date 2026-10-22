using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NWSDB.API.DTOs.Payments;
using NWSDB.API.Exceptions;
using NWSDB.API.Interfaces;

namespace NWSDB.API.HttpClients
{
    /// <summary>
    /// Typed HttpClient that communicates with Bank.API over REST/HTTP.
    /// Registered via IHttpClientFactory in Program.cs.
    /// Base address is configured in appsettings under "BankApi:BaseUrl".
    /// </summary>
    public class BankApiClient : IBankApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<BankApiClient> _logger;

        public BankApiClient(HttpClient httpClient, ILogger<BankApiClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<BankPaymentResult> ProcessPaymentAsync(BankPaymentRequest request)
        {
            _logger.LogInformation(
                "Calling Bank.API: POST /api/v1/bank/payments | ExternalRef: {ExternalReference} | Account: {AccountNumber} | Amount: {Amount}",
                request.ExternalReference, request.AccountNumber, request.Amount);

            HttpResponseMessage response;

            try
            {
                response = await _httpClient.PostAsJsonAsync("api/v1/bank/payments", request);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Network error communicating with Bank.API.");
                throw new BankApiException(
                    "Unable to reach the payment gateway. Please try again later.", ex, HttpStatusCode.BadGateway);
            }

            // Read the body regardless of status so we can surface Bank.API's error message
            BankApiResponse<BankPaymentResult>? envelope = null;
            try
            {
                envelope = await response.Content
                    .ReadFromJsonAsync<BankApiResponse<BankPaymentResult>>();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not parse JSON response body from Bank.API.");
            }

            if (!response.IsSuccessStatusCode || envelope == null || !envelope.Success)
            {
                var bankMessage = envelope?.Message ?? "Bank payment failed with no additional details.";

                _logger.LogWarning(
                    "Bank.API rejected payment. Status: {StatusCode} | Message: {BankMessage}",
                    (int)response.StatusCode, bankMessage);

                throw new BankApiException(bankMessage, response.StatusCode);
            }

            _logger.LogInformation(
                "Bank.API payment succeeded. TransactionId: {TransactionId} | Status: {Status}",
                envelope.Data!.TransactionId, envelope.Data.Status);

            return envelope.Data;
        }
    }
}
