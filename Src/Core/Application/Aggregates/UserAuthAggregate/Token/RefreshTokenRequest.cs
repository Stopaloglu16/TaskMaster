namespace Application.Aggregates.UserAuthAggregate.Token;

public record RefreshTokenRequest
{
    public required string AccessToken { get; set; }
    public required string RefreshToken { get; set; }
}
