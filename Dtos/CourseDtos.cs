using System.ComponentModel.DataAnnotations;

namespace iCAN8.Api.Dtos;

public record CourseDto(
    string Id,
    string Title,
    string? Description,
    string TeacherId,
    double? ProgressPct
);

public class CourseCreateDto
{
    /// <summary>課程名稱</summary>
    [Required, StringLength(100)]
    public string Title { get; set; } = default!;

    /// <summary>課程描述</summary>
    [StringLength(2000)]
    public string? Description { get; set; }
}

public class CourseUpdateDto : CourseCreateDto { }
