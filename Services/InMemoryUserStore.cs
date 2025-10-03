using iCAN8.Api.Models;
using Microsoft.AspNetCore.Identity;
using System.Collections.Concurrent;

namespace iCAN8.Api.Services;

public class InMemoryUserStore : IUserStore
{
    private readonly ConcurrentDictionary<string, AppUser> _users = new();
    private readonly PasswordHasher<AppUser> _hasher = new();

    public InMemoryUserStore()
    {
        var t1 = new AppUser { Id = "T001", Username = "teacher1", Roles = new[] { "Teacher" } };
        t1.PasswordHash = _hasher.HashPassword(t1, "P@ssw0rd!");
        _users[t1.Username] = t1;

        var s1 = new AppUser { Id = "S001", Username = "student1", Roles = new[] { "Student" } };
        s1.PasswordHash = _hasher.HashPassword(s1, "P@ssw0rd!");
        _users[s1.Username] = s1;
    }

    public Task<AppUser?> FindByUsernameAsync(string username, CancellationToken ct)
        => Task.FromResult(_users.TryGetValue(username, out var u) ? u : null);

    public Task<bool> VerifyPasswordAsync(AppUser user, string password, CancellationToken ct)
        => Task.FromResult(_hasher.VerifyHashedPassword(user, user.PasswordHash, password) == PasswordVerificationResult.Success);
}
