using iCAN8.Api.Dtos;
using System.Collections.Concurrent;

namespace LearningPlatform.Api.Services;

public class InMemoryCourseService : ICourseService
{
    private readonly ConcurrentDictionary<string, CourseDto> _courses = new();
    private readonly ConcurrentDictionary<string, CourseOutlineDto> _outlines = new();
    private readonly ConcurrentDictionary<string, List<MaterialDto>> _materials = new();

    public Task<IEnumerable<CourseDto>> SearchAsync(string? q, CancellationToken ct)
    {
        var source = _courses.Values.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(q))
            source = source.Where(c => c.Title.Contains(q, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(source);
    }

    public Task<CourseDto?> FindByIdAsync(string courseId, CancellationToken ct)
        => Task.FromResult(_courses.TryGetValue(courseId, out var c) ? c : null);

    public Task<CourseDto> CreateAsync(CourseCreateDto dto, string teacherId, CancellationToken ct)
    {
        var id = $"C{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
        var created = new CourseDto(id, dto.Title, dto.Description, teacherId, null);
        _courses.TryAdd(id, created);
        return Task.FromResult(created);
    }

    public async Task<CourseDto?> UpdateAsync(string courseId, CourseUpdateDto dto, string teacherId, CancellationToken ct)
    {
        var old = await FindByIdAsync(courseId, ct);
        if (old is null || old.TeacherId != teacherId) return null;
        var updated = old with { Title = dto.Title, Description = dto.Description };
        _courses[courseId] = updated;
        return updated;
    }

    public async Task<bool> DeleteAsync(string courseId, string teacherId, CancellationToken ct)
    {
        var old = await FindByIdAsync(courseId, ct);
        if (old is null || old.TeacherId != teacherId) return false;
        return _courses.TryRemove(courseId, out _);
    }

    public Task<CourseOutlineDto?> GetOutlineAsync(string courseId, CancellationToken ct)
        => Task.FromResult(_outlines.TryGetValue(courseId, out var o) ? o : null);

    public async Task<bool> UpdateOutlineAsync(string courseId, CourseOutlineDto outline, string teacherId, CancellationToken ct)
    {
        var owner = await IsOwnerAsync(courseId, teacherId, ct);
        if (!owner) return false;
        _outlines[courseId] = outline;
        return true;
    }

    public Task<IEnumerable<MaterialDto>> ListMaterialsAsync(string courseId, CancellationToken ct)
    {
        _materials.TryGetValue(courseId, out var list);
        list ??= new List<MaterialDto>();
        return Task.FromResult(list.AsEnumerable());
    }

    public async Task<MaterialDto?> AddMaterialAsync(MaterialCreateDto dto, string teacherId, CancellationToken ct)
    {
        var owner = await IsOwnerAsync(dto.CourseId, teacherId, ct);
        if (!owner) return null;

        var m = new MaterialDto
        {
            Id = $"M{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
            CourseId = dto.CourseId,
            Type = dto.Type,
            Title = dto.Title,
            Content = dto.Content,
            VideoUrl = dto.VideoUrl,
            DurationSeconds = dto.DurationSeconds
        };
        var list = _materials.GetOrAdd(dto.CourseId, _ => new List<MaterialDto>());
        list.Add(m);
        return m;
    }

    public async Task<bool> IsOwnerAsync(string courseId, string teacherId, CancellationToken ct)
    {
        var c = await FindByIdAsync(courseId, ct);
        return c is not null && c.TeacherId == teacherId;
    }
}
