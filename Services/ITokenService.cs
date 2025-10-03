using iCAN8.Api.Models;

namespace iCAN8.Api.Services;

public interface ITokenService
{
    string CreateAccessToken(AppUser user, out int expiresInSeconds);
}
