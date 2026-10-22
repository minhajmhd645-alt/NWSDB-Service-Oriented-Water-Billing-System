namespace Bank.API.DTOs
{
    /// <summary>
    /// Standard API response envelope used by all Bank API endpoints.
    /// </summary>
    public class ResponseDto<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public string Timestamp { get; set; } = DateTime.UtcNow.ToString("o");
    }
}
