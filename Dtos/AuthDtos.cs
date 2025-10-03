namespace iCAN8.Api.Dtos;

public class LoginRequest
{
    public string Username { get; set; } = default!;
    public string Password { get; set; } = default!;
}

public class LoginResponse
{
    public string AccessToken { get; set; } = default!;
    public int ExpiresIn { get; set; } // seconds
    public string RefreshToken { get; set; } = default!;
}

public class RefreshRequest
{
    public string RefreshToken { get; set; } = default!;
}

public class LogoutRequest
{
    public string? RefreshToken { get; set; } // 指定要註銷的 refresh token；不填則註銷該使用者全部
}
