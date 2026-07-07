using AirplaneSystem.Application.Common.Interfaces;
using AirplaneSystem.Application.DTOs.Flights;
using AirplaneSystem.Application.Exceptions;
using AirplaneSystem.Application.Repositories;
using AirplaneSystem.Domain.Entities.Flights;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AirplaneSystem.API.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/airlines")]
public class AirlinesController : ControllerBase
{
    private const string LogoSubFolder = "airlines/logos";
    private const string GallerySubFolder = "airlines/gallery";

    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly IFileStorageService _fileStorage;

    public AirlinesController(IUnitOfWork uow, IMapper mapper, IFileStorageService fileStorage)
    {
        _uow = uow;
        _mapper = mapper;
        _fileStorage = fileStorage;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AirlineDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var airlines = await _uow.Airlines.GetAllWithImagesAsync(ct);
        return Ok(airlines.Select(a => _mapper.Map<AirlineDto>(a)));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AirlineDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var airline = await _uow.Airlines.GetByIdWithImagesAsync(id, ct);
        if (airline == null) return NotFound();
        return Ok(_mapper.Map<AirlineDto>(airline));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(AirlineDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromForm] AirlineFormRequest request, CancellationToken ct)
    {
        var existing = await _uow.Airlines.GetByIataCodeAsync(request.IataCode, ct);
        if (existing != null)
            throw new ValidationException("iataCode", $"An airline with IATA code '{request.IataCode}' already exists.");

        var airline = new Airline
        {
            IataCode = request.IataCode.ToUpperInvariant(),
            Name = request.Name,
            Country = request.Country,
            ContactEmail = request.ContactEmail,
            ContactPhone = request.ContactPhone,
        };

        if (request.Logo != null)
            airline.LogoUrl = await _fileStorage.SaveAsync(request.Logo, LogoSubFolder, ct);

        await _uow.Airlines.AddAsync(airline, ct);

        if (request.Images is { Count: > 0 })
        {
            for (var i = 0; i < request.Images.Count; i++)
            {
                var url = await _fileStorage.SaveAsync(request.Images[i], GallerySubFolder, ct);
                airline.Images.Add(new AirlineImage
                {
                    AirlineId = airline.Id,
                    ImageUrl = url,
                    IsPrimary = i == 0,
                    SortOrder = i,
                });
            }
        }

        await _uow.SaveChangesAsync(ct);

        var created = await _uow.Airlines.GetByIdWithImagesAsync(airline.Id, ct);
        return StatusCode(StatusCodes.Status201Created, _mapper.Map<AirlineDto>(created));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(AirlineDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromForm] AirlineFormRequest request, CancellationToken ct)
    {
        var airline = await _uow.Airlines.GetByIdWithImagesAsync(id, ct);
        if (airline == null) return NotFound();

        airline.Name = request.Name;
        airline.Country = request.Country;
        airline.ContactEmail = request.ContactEmail;
        airline.ContactPhone = request.ContactPhone;

        // Replace logo only if a new one is uploaded — keeps existing logo otherwise.
        if (request.Logo != null)
        {
            var oldLogoUrl = airline.LogoUrl;
            airline.LogoUrl = await _fileStorage.SaveAsync(request.Logo, LogoSubFolder, ct);
            await _fileStorage.DeleteAsync(oldLogoUrl, ct);
        }

        // Optionally accept new gallery images on the same request for convenience.
        if (request.Images is { Count: > 0 })
        {
            var nextSort = airline.Images.Count == 0 ? 0 : airline.Images.Max(i => i.SortOrder) + 1;
            var hasPrimary = airline.Images.Any(i => i.IsPrimary);
            foreach (var file in request.Images)
            {
                var url = await _fileStorage.SaveAsync(file, GallerySubFolder, ct);
                airline.Images.Add(new AirlineImage
                {
                    AirlineId = airline.Id,
                    ImageUrl = url,
                    IsPrimary = !hasPrimary,
                    SortOrder = nextSort++,
                });
                hasPrimary = true;
            }
        }

        _uow.Airlines.Update(airline);
        await _uow.SaveChangesAsync(ct);

        var updated = await _uow.Airlines.GetByIdWithImagesAsync(id, ct);
        return Ok(_mapper.Map<AirlineDto>(updated));
    }

    /// <summary>Add one or more gallery images to an existing airline.</summary>
    [HttpPost("{id:guid}/images")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(AirlineDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddImages(Guid id, [FromForm] AddAirlineImagesRequest request, CancellationToken ct)
    {
        var airline = await _uow.Airlines.GetByIdWithImagesAsync(id, ct);
        if (airline == null) return NotFound();

        if (request.Images is not { Count: > 0 })
            throw new ValidationException("images", "At least one image file is required.");

        var nextSort = airline.Images.Count == 0 ? 0 : airline.Images.Max(i => i.SortOrder) + 1;
        var hasPrimary = airline.Images.Any(i => i.IsPrimary);

        foreach (var file in request.Images)
        {
            var url = await _fileStorage.SaveAsync(file, GallerySubFolder, ct);
            airline.Images.Add(new AirlineImage
            {
                AirlineId = airline.Id,
                ImageUrl = url,
                IsPrimary = !hasPrimary,
                SortOrder = nextSort++,
            });
            hasPrimary = true;
        }

        await _uow.SaveChangesAsync(ct);

        var updated = await _uow.Airlines.GetByIdWithImagesAsync(id, ct);
        return Ok(_mapper.Map<AirlineDto>(updated));
    }

    /// <summary>Delete a single gallery image from an airline.</summary>
    [HttpDelete("{id:guid}/images/{imageId:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(AirlineDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteImage(Guid id, Guid imageId, CancellationToken ct)
    {
        var airline = await _uow.Airlines.GetByIdWithImagesAsync(id, ct);
        if (airline == null) return NotFound();

        var image = airline.Images.FirstOrDefault(i => i.Id == imageId);
        if (image == null) return NotFound(new { message = "Image not found for this airline." });

        await _fileStorage.DeleteAsync(image.ImageUrl, ct);
        airline.Images.Remove(image);

        // Promote the next image (if any) to primary so the gallery always has one.
        if (image.IsPrimary)
        {
            var next = airline.Images.OrderBy(i => i.SortOrder).FirstOrDefault();
            if (next != null) next.IsPrimary = true;
        }

        await _uow.SaveChangesAsync(ct);

        var updated = await _uow.Airlines.GetByIdWithImagesAsync(id, ct);
        return Ok(_mapper.Map<AirlineDto>(updated));
    }
}

public class AirlineFormRequest
{
    public string IataCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public IFormFile? Logo { get; set; }
    public List<IFormFile>? Images { get; set; }
}

public class AddAirlineImagesRequest
{
    public List<IFormFile>? Images { get; set; }
}