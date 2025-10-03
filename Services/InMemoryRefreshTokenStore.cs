using iCAN8.Api.Models;
using iCAN8.Api.Services;
using System.Collections.Concurrent;
using System.Security.Cryptography;

namespace iCAN8.Api.Services;

public class InMemoryRefreshTokenStore : IRefreshTokenStore
{
    private readonly ConcurrentDictionary<string, RefreshToken> _store = new();

    public Task<RefreshToken> CreateAsync(string userId, TimeSpan lifetime, CancellationToken ct)
    {
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        var rt = new RefreshToken
        {
            Token = token,
            UserId = userId,
            ExpiresAtUtc = DateTime.UtcNow.Add(lifetime),
            Revoked = false
        };
        _store[token] = rt;
        return Task.FromResult(rt);
    }

    public Task<RefreshToken?> FindAsync(string token, CancellationToken ct)
        => Task.FromResult(_store.TryGetValue(token, out var r) ? r : null);

    public Task RevokeAsync(string token, CancellationToken ct)
    {
        if (_store.TryGetValue(token, out var r)) r.Revoked = true;
        return Task.CompletedTask;
    }

    public Task RevokeAllForUserAsync(string userId, CancellationToken ct)
    {
        foreach (var kv in _store.Where(kv => kv.Value.UserId == userId)) kv.Value.Revoked = true;
        return Task.CompletedTask;
    }

    public Task RotateAsync(string oldToken, RefreshToken @new, CancellationToken ct)
    {
        if (_store.TryGetValue(oldToken, out var r)) r.Revoked = true;
        _store[@new.Token] = @new;
        return Task.CompletedTask;
    }
}
