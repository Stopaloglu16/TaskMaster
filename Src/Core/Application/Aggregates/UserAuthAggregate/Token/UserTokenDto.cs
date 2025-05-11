namespace Application.Aggregates.UserAuthAggregate.Token;

public class UserTokenDto
{
    public string AspId { get; set; }
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}
