namespace Application.Aggregates.UserAuthAggregate.Token;

public record TokenRefreshRequest
{
    public required string AccessToken { get; init; }
    public required string RefreshToken { get; init; }

}
