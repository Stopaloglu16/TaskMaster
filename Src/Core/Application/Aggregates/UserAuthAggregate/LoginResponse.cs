namespace Application.Aggregates.UserAuthAggregate;

public record LoginResponse
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
}
