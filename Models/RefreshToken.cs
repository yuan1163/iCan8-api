namespace iCAN8.Api.Models;

public class RefreshToken
{
    public string Token { get; set; } = default!;
    public string UserId { get; set; } = default!;
    public DateTime ExpiresAtUtc { get; set; }
    public bool Revoked { get; set; }
}
