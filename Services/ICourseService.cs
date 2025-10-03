using iCAN8.Api.Dtos;

namespace iCAN8.Api.Services;

public interface ICourseService
{
    Task<IEnumerable<CourseDto>> SearchAsync(string? q, CancellationToken ct);
    Task<CourseDto?> FindByIdAsync(string courseId, CancellationToken ct);
    Task<CourseDto> CreateAsync(CourseCreateDto dto, string teacherId, CancellationToken ct);
    Task<CourseDto?> UpdateAsync(string courseId, CourseUpdateDto dto, string teacherId, CancellationToken ct);
    Task<bool> DeleteAsync(string courseId, string teacherId, CancellationToken ct);

    Task<CourseOutlineDto?> GetOutlineAsync(string courseId, CancellationToken ct);
    Task<bool> UpdateOutlineAsync(string courseId, CourseOutlineDto outline, string teacherId, CancellationToken ct);

    Task<IEnumerable<MaterialDto>> ListMaterialsAsync(string courseId, CancellationToken ct);
    Task<MaterialDto?> AddMaterialAsync(MaterialCreateDto dto, string teacherId, CancellationToken ct);

    /// <summary>檢查是否為課程擁有者（老師）</summary>
    Task<bool> IsOwnerAsync(string courseId, string teacherId, CancellationToken ct);
}
