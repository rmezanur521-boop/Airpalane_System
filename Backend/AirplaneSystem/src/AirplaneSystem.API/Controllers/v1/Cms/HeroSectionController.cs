using AirplaneSystem.Application.DTOs.Admin;   // UploadFileRequest reuse
using AirplaneSystem.Application.DTOs.Cms;
using AirplaneSystem.Application.Services.Interfaces;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AirplaneSystem.API.Controllers.v1.Cms;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/cms/hero-sections")]
[Authorize(Roles = "Admin")]
public class HeroSectionController : ControllerBase
{
    private readonly IHeroSectionService _service;
    public HeroSectionController(IHeroSectionService service) => _service = service;

    [HttpGet]
    [ProducesResponseType(typeof(List<HeroSectionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok(await _service.GetAllAsync(ct));

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(HeroSectionDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        => Ok(await _service.GetByIdAsync(id, ct));

    [HttpPost]
    [ProducesResponseType(typeof(HeroSectionDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateHeroSectionDto dto, CancellationToken ct)
    {
        var result = await _service.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id, version = "1.0" }, result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(HeroSectionDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateHeroSectionDto dto, CancellationToken ct)
        => Ok(await _service.UpdateAsync(id, dto, ct));

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/image")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> UploadImage(Guid id, [FromForm] UploadFileRequest request, CancellationToken ct)
    {
        var url = await _service.UploadImageAsync(id, request.File, ct);
        return Ok(new { imageUrl = url });
    }
    [HttpPut("reorder")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Reorder([FromBody] ReorderRequestDto request, CancellationToken ct)
    {
        await _service.ReorderAsync(request, ct);
        return NoContent();
    }
}