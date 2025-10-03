namespace iCAN8.Api.Models;

public class AppUser
{
    public string Id { get; set; } = default!;
    public string Username { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;
    public string[] Roles { get; set; } = Array.Empty<string>();
}
