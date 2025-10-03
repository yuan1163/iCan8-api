using iCAN8.Api.Dtos;
using iCAN8.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace iCAN8.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CoursesController : ControllerBase
{
    private readonly ICourseService _svc;

    public CoursesController(ICourseService svc) => _svc = svc;

    /// <summary>查詢課程列表（學生看可選課程，老師看自己管理的課程）</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CourseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search([FromQuery] string? q, CancellationToken ct)
    {
        var list = await _svc.SearchAsync(q, ct);
        return Ok(list);
    }

    /// <summary>老師新增課程</summary>
    [HttpPost]
    [Authorize(Roles = "Teacher")]
    [ProducesResponseType(typeof(CourseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] CourseCreateDto dto, CancellationToken ct)
    {
        var teacherId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "T001"; // demo：真實環境請從 JWT 取
        var created = await _svc.CreateAsync(dto, teacherId, ct);

        // 201 + Location header（Swagger 會顯示）
        return CreatedAtAction(nameof(GetById), new { courseId = created.Id }, created);
    }

    /// <summary>取得課程</summary>
    [HttpGet("{courseId}")]
    [ProducesResponseType(typeof(CourseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] string courseId, CancellationToken ct)
    {
        var item = await _svc.FindByIdAsync(courseId, ct);
        return item is null ? NotFound() : Ok(item);
    }

    /// <summary>老師更新課程</summary>
    [HttpPut("{courseId}")]
    [Authorize(Roles = "Teacher")]
    [ProducesResponseType(typeof(CourseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update([FromRoute] string courseId, [FromBody] CourseUpdateDto dto, CancellationToken ct)
    {
        var teacherId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "T001";
        var updated = await _svc.UpdateAsync(courseId, dto, teacherId, ct);
        if (updated is null)
        {
            // 可能是不存在或非擁有者
            var exists = await _svc.FindByIdAsync(courseId, ct);
            return exists is null ? NotFound() : Forbid(); // 更精準的語意
        }
        return Ok(updated);
    }

    /// <summary>老師刪除課程</summary>
    [HttpDelete("{courseId}")]
    [Authorize(Roles = "Teacher")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] string courseId, CancellationToken ct)
    {
        var teacherId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "T001";
        var ok = await _svc.DeleteAsync(courseId, teacherId, ct);
        if (!ok)
        {
            var exists = await _svc.FindByIdAsync(courseId, ct);
            return exists is null ? NotFound() : Forbid();
        }
        return NoContent();
    }

    /// <summary>取得課程大綱/進度</summary>
    [HttpGet("{courseId}/outline")]
    [ProducesResponseType(typeof(CourseOutlineDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOutline([FromRoute] string courseId, CancellationToken ct)
    {
        var outline = await _svc.GetOutlineAsync(courseId, ct);
        return outline is null ? NotFound() : Ok(outline);
    }

    /// <summary>老師更新課程大綱/進度</summary>
    [HttpPut("{courseId}/outline")]
    [Authorize(Roles = "Teacher")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateOutline([FromRoute] string courseId, [FromBody] CourseOutlineDto dto, CancellationToken ct)
    {
        var teacherId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "T001";
        var exists = await _svc.FindByIdAsync(courseId, ct);
        if (exists is null) return NotFound();

        var ok = await _svc.UpdateOutlineAsync(courseId, dto, teacherId, ct);
        return ok ? Ok() : Forbid();
    }

    /// <summary>列出課程教材</summary>
    [HttpGet("{courseId}/materials")]
    [ProducesResponseType(typeof(IEnumerable<MaterialDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ListMaterials([FromRoute] string courseId, CancellationToken ct)
    {
        var course = await _svc.FindByIdAsync(courseId, ct);
        if (course is null) return NotFound();
        var items = await _svc.ListMaterialsAsync(courseId, ct);
        return Ok(items);
    }

    /// <summary>老師新增教材（文字或影音連結）</summary>
    [HttpPost("{courseId}/materials")]
    [Authorize(Roles = "Teacher")]
    [ProducesResponseType(typeof(MaterialDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddMaterial([FromRoute] string courseId, [FromBody] MaterialCreateDto dto, CancellationToken ct)
    {
        // 以路由為準，避免 Body 被竄改課程 Id
        dto.CourseId = courseId;

        var exists = await _svc.FindByIdAsync(courseId, ct);
        if (exists is null) return NotFound();

        var teacherId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "T001";
        var created = await _svc.AddMaterialAsync(dto, teacherId, ct);
        if (created is null) return Forbid();

        // Tips：這裡也可回傳 CreatedAtAction 到 GET 單一教材端點（若你有做）
        return Created($"/api/courses/{courseId}/materials/{created.Id}", created);
    }
}
