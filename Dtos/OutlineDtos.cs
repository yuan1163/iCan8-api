using System.ComponentModel.DataAnnotations;

namespace iCAN8.Api.Dtos;

public class CourseOutlineDto
{
    public List<ModuleDto> Modules { get; set; } = new();
}

public class ModuleDto
{
    [Required, StringLength(100)]
    public string Title { get; set; } = default!;

    /// <summary>排序序號（1-based）</summary>
    [Range(1, int.MaxValue)]
    public int Order { get; set; }

    public List<ModuleItemDto> Items { get; set; } = new();
}

public class ModuleItemDto
{
    [Required, StringLength(100)]
    public string Title { get; set; } = default!;

    /// <summary>項目類型</summary>
    [Required]
    [RegularExpression("material|quiz|assignment")]
    public string Type { get; set; } = default!;

    /// <summary>對應教材或考試的 Id（可為 null 代表尚未連結）</summary>
    public string? RefId { get; set; }
}
