using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NWSDB.Web.Models;

namespace NWSDB.Web.Services
{
    public class NwsdbApiClient : INwsdbApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<NwsdbApiClient> _logger;

        public NwsdbApiClient(HttpClient httpClient, ILogger<NwsdbApiClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<CustomerSummaryModel?> GetCustomerSummaryAsync(string accountNumber, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ApiResponse<CustomerSummaryModel>>(
                    $"api/v1/customers/{accountNumber}/summary", cancellationToken);
                return response?.Data;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting customer summary for account: {AccountNumber}", accountNumber);
                return null;
            }
        }

        public async Task<CustomerModel?> GetCustomerAsync(string accountNumber, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ApiResponse<CustomerModel>>(
                    $"api/v1/customers/{accountNumber}", cancellationToken);
                return response?.Data;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting customer details for account: {AccountNumber}", accountNumber);
                return null;
            }
        }

        public async Task<IEnumerable<BillModel>> GetBillsAsync(string accountNumber, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ApiResponse<IEnumerable<BillModel>>>(
                    $"api/v1/bills/{accountNumber}", cancellationToken);
                return response?.Data ?? Array.Empty<BillModel>();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting bills for account: {AccountNumber}", accountNumber);
                return Array.Empty<BillModel>();
            }
        }

        public async Task<IEnumerable<BillModel>> GetUnpaidBillsAsync(string accountNumber, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ApiResponse<IEnumerable<BillModel>>>(
                    $"api/v1/bills/{accountNumber}/unpaid", cancellationToken);
                return response?.Data ?? Array.Empty<BillModel>();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting unpaid bills for account: {AccountNumber}", accountNumber);
                return Array.Empty<BillModel>();
            }
        }

        public async Task<BillModel?> GetBillAsync(int billId, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ApiResponse<BillModel>>(
                    $"api/v1/bills/id/{billId}", cancellationToken);
                return response?.Data;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting bill by ID: {BillId}", billId);
                return null;
            }
        }

        public async Task<IEnumerable<PaymentModel>> GetPaymentHistoryAsync(string accountNumber, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ApiResponse<IEnumerable<PaymentModel>>>(
                    $"api/v1/payments/{accountNumber}", cancellationToken);
                return response?.Data ?? Array.Empty<PaymentModel>();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting payment history for account: {AccountNumber}", accountNumber);
                return Array.Empty<PaymentModel>();
            }
        }

        public async Task<PaymentModel?> GetPaymentAsync(int paymentId, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ApiResponse<PaymentModel>>(
                    $"api/v1/payments/id/{paymentId}", cancellationToken);
                return response?.Data;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting payment by ID: {PaymentId}", paymentId);
                return null;
            }
        }

        public async Task<(bool Success, PaymentModel? Payment, string ErrorMessage)> MakePaymentAsync(
            PaymentRequestModel request, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/v1/payments", request, cancellationToken);
                var envelope = await response.Content.ReadFromJsonAsync<ApiResponse<PaymentModel>>(cancellationToken: cancellationToken);

                if (response.IsSuccessStatusCode && envelope != null && envelope.Success)
                {
                    return (true, envelope.Data, envelope.Message);
                }

                var message = envelope?.Message ?? "Payment failed. Please verify your details and try again.";
                return (false, null, message);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Network error communicating with NWSDB API.");
                return (false, null, "Unable to connect to NWSDB service. Please check your network and try again later.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error executing payment.");
                return (false, null, "An unexpected error occurred while processing your request.");
            }
        }

        public async Task<IEnumerable<CustomerModel>> GetAllCustomersAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ApiResponse<IEnumerable<CustomerModel>>>(
                    "api/v1/customers", cancellationToken);
                return response?.Data ?? Array.Empty<CustomerModel>();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting all customers.");
                return Array.Empty<CustomerModel>();
            }
        }

        public async Task<(bool Success, CustomerModel? Customer, string ErrorMessage, int StatusCode)> CreateCustomerAsync(
            CreateCustomerRequestModel request, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/v1/customers", request, cancellationToken);
                var envelope = await response.Content.ReadFromJsonAsync<ApiResponse<CustomerModel>>(cancellationToken: cancellationToken);
                int statusCode = (int)response.StatusCode;

                if (response.IsSuccessStatusCode && envelope != null && envelope.Success)
                {
                    return (true, envelope.Data, envelope.Message, statusCode);
                }

                var message = envelope?.Message ?? (response.StatusCode == HttpStatusCode.Conflict 
                    ? "Customer account number already exists." 
                    : "Failed to create customer. Please try again.");

                return (false, null, message, statusCode);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Network error communicating with NWSDB API during customer creation.");
                return (false, null, "Unable to connect to NWSDB service. Please try again later.", 500);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error creating customer.");
                return (false, null, "An unexpected error occurred while processing your request.", 500);
            }
        }

        public async Task<(bool Success, CustomerModel? Customer, string ErrorMessage, int StatusCode)> UpdateCustomerAsync(
            string accountNumber, UpdateCustomerRequestModel request, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/v1/customers/{accountNumber}", request, cancellationToken);
                var envelope = await response.Content.ReadFromJsonAsync<ApiResponse<CustomerModel>>(cancellationToken: cancellationToken);
                int statusCode = (int)response.StatusCode;

                if (response.IsSuccessStatusCode && envelope != null && envelope.Success)
                {
                    return (true, envelope.Data, envelope.Message, statusCode);
                }

                var message = envelope?.Message ?? (response.StatusCode == HttpStatusCode.NotFound 
                    ? "Customer not found." 
                    : "Failed to update customer. Please try again.");

                return (false, null, message, statusCode);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Network error communicating with NWSDB API during customer update.");
                return (false, null, "Unable to connect to NWSDB service. Please try again later.", 500);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error updating customer.");
                return (false, null, "An unexpected error occurred while processing your request.", 500);
            }
        }

        public async Task<string> GetNextAccountNumberAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ApiResponse<string>>(
                    "api/v1/customers/next-account-number", cancellationToken);
                return response?.Data ?? string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting next account number.");
                return string.Empty;
            }
        }

        public async Task<IEnumerable<BillModel>> GetAllBillsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ApiResponse<IEnumerable<BillModel>>>(
                    "api/v1/bills", cancellationToken);
                return response?.Data ?? Array.Empty<BillModel>();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting all bills.");
                return Array.Empty<BillModel>();
            }
        }

        public async Task<(bool Success, BillModel? Bill, string ErrorMessage, int StatusCode)> CreateBillAsync(
            CreateBillRequestModel request, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/v1/bills", request, cancellationToken);
                var envelope = await response.Content.ReadFromJsonAsync<ApiResponse<BillModel>>(cancellationToken: cancellationToken);
                int statusCode = (int)response.StatusCode;

                if (response.IsSuccessStatusCode && envelope != null && envelope.Success)
                {
                    return (true, envelope.Data, envelope.Message, statusCode);
                }

                var message = envelope?.Message ?? "Failed to create bill. Please check the account number and meter readings.";
                return (false, null, message, statusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error creating bill.");
                return (false, null, "An unexpected error occurred while processing your request.", 500);
            }
        }

        public async Task<(bool Success, BillModel? Bill, string ErrorMessage, int StatusCode)> UpdateBillAsync(
            int billId, UpdateBillRequestModel request, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/v1/bills/{billId}", request, cancellationToken);
                var envelope = await response.Content.ReadFromJsonAsync<ApiResponse<BillModel>>(cancellationToken: cancellationToken);
                int statusCode = (int)response.StatusCode;

                if (response.IsSuccessStatusCode && envelope != null && envelope.Success)
                {
                    return (true, envelope.Data, envelope.Message, statusCode);
                }

                var message = envelope?.Message ?? "Failed to update bill.";
                return (false, null, message, statusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error updating bill.");
                return (false, null, "An unexpected error occurred while processing your request.", 500);
            }
        }

        public async Task<IEnumerable<PaymentModel>> GetAllPaymentsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ApiResponse<IEnumerable<PaymentModel>>>(
                    "api/v1/payments", cancellationToken);
                return response?.Data ?? Array.Empty<PaymentModel>();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting all payments.");
                return Array.Empty<PaymentModel>();
            }
        }

        public async Task<AdminStatsModel?> GetAdminStatsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ApiResponse<AdminStatsModel>>(
                    "api/v1/admin/stats", cancellationToken);
                return response?.Data;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting admin stats.");
                return null;
            }
        }
    }
}
