using System.ComponentModel.DataAnnotations;

namespace iCAN8.Api.Dtos;

public class MaterialDto
{
    public string Id { get; set; } = default!;
    public string CourseId { get; set; } = default!;

    /// <summary>text 或 video</summary>
    public string Type { get; set; } = default!;
    public string Title { get; set; } = default!;
    public string? Content { get; set; }
    public Uri? VideoUrl { get; set; }
    public int? DurationSeconds { get; set; }
}

public class MaterialCreateDto
{
    [Required] public string CourseId { get; set; } = default!;
    [Required, RegularExpression("text|video")] public string Type { get; set; } = default!;
    [Required, StringLength(200)] public string Title { get; set; } = default!;

    // text
    public string? Content { get; set; }

    // video
    public Uri? VideoUrl { get; set; }
    public int? DurationSeconds { get; set; }
}
