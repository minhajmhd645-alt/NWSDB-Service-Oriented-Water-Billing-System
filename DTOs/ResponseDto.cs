namespace NWSDB.API.DTOs
{
    public class ResponseDto<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public string Timestamp { get; set; } = System.DateTime.UtcNow.ToString("o");
    }
}
