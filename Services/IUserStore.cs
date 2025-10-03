using iCAN8.Api.Models;

namespace iCAN8.Api.Services;

public interface IUserStore
{
    Task<AppUser?> FindByUsernameAsync(string username, CancellationToken ct);
    Task<bool> VerifyPasswordAsync(AppUser user, string password, CancellationToken ct);
}
