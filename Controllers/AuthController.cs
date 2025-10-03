using iCAN8.Api.Dtos;
using iCAN8.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace iCAN8.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IUserStore _users;
    private readonly ITokenService _tokens;
    private readonly IRefreshTokenStore _refresh;
    private readonly IConfiguration _cfg;

    public AuthController(IUserStore users, ITokenService tokens, IRefreshTokenStore refresh, IConfiguration cfg)
    {
        _users = users;
        _tokens = tokens;
        _refresh = refresh;
        _cfg = cfg;
    }

    /// <summary>登入取得 JWT 與 Refresh Token</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest req, CancellationToken ct)
    {
        var user = await _users.FindByUsernameAsync(req.Username, ct);
        if (user is null || !await _users.VerifyPasswordAsync(user, req.Password, ct))
            return Unauthorized();

        var access = _tokens.CreateAccessToken(user, out var expiresIn);
        var days = int.Parse(_cfg["Jwt:RefreshTokenDays"] ?? "14");
        var refresh = await _refresh.CreateAsync(user.Id, TimeSpan.FromDays(days), ct);

        return Ok(new LoginResponse
        {
            AccessToken = access,
            ExpiresIn = expiresIn,
            RefreshToken = refresh.Token
        });
    }

    /// <summary>用 Refresh Token 取得新 Access Token（會輪替 Refresh Token）</summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.RefreshToken)) return BadRequest();

        var old = await _refresh.FindAsync(req.RefreshToken, ct);
        if (old is null || old.Revoked || old.ExpiresAtUtc <= DateTime.UtcNow)
            return Unauthorized();

        // 這裡示範：用 userId 重新發 Token；實務請從 UserStore 取出完整使用者
        var user = await _users.FindByUsernameAsync("teacher1", ct); // DEMO：請替換成依 userId 取 user 的方法
        if (user is null || user.Id != old.UserId) return Unauthorized();

        var access = _tokens.CreateAccessToken(user, out var expiresIn);
        var days = int.Parse(_cfg["Jwt:RefreshTokenDays"] ?? "14");
        var @new = await _refresh.CreateAsync(user.Id, TimeSpan.FromDays(days), ct);
        await _refresh.RotateAsync(req.RefreshToken, @new, ct);

        return Ok(new LoginResponse
        {
            AccessToken = access,
            ExpiresIn = expiresIn,
            RefreshToken = @new.Token
        });
    }

    /// <summary>登出：註銷指定或全部 Refresh Token</summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest req, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!;
        if (!string.IsNullOrWhiteSpace(req.RefreshToken))
            await _refresh.RevokeAsync(req.RefreshToken!, ct);
        else
            await _refresh.RevokeAllForUserAsync(userId, ct);

        return NoContent();
    }

    /// <summary>取得目前登入者資訊（測試用）</summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public IActionResult Me()
    {
        var uid = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        var name = User.FindFirstValue(ClaimTypes.Name) ?? User.Identity?.Name;
        var roles = User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToArray();
        return Ok(new { userId = uid, username = name, roles });
    }
}
