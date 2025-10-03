using iCAN8.Api.Models;

namespace iCAN8.Api.Services;

public interface IRefreshTokenStore
{
    Task<RefreshToken> CreateAsync(string userId, TimeSpan lifetime, CancellationToken ct);
    Task<RefreshToken?> FindAsync(string token, CancellationToken ct);
    Task RevokeAsync(string token, CancellationToken ct);
    Task RevokeAllForUserAsync(string userId, CancellationToken ct);
    Task RotateAsync(string oldToken, RefreshToken newToken, CancellationToken ct);
}
