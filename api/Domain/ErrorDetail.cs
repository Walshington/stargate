namespace StargateAPI.Domain;

public class ErrorDetail
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Details { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /* Factory method to create an error detail object */
    public static ErrorDetail Create(string code, string message, string? details = null)
    {
        return new ErrorDetail
        {
            Code = code,
            Message = message,
            Details = details,
            Timestamp = DateTime.UtcNow
        };
    }
}
