using LegendsViewer.Backend.Contracts;
using LegendsViewer.Backend.Legends.Interfaces;
using LegendsViewer.Backend.Legends.Various;
using Microsoft.AspNetCore.Mvc;

namespace LegendsViewer.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CreatureInfoController(IWorld worldDataService) : ControllerBase
{
    private const int DefaultPageSize = 10;
    private const int DefaultPageNumber = 1;
    private readonly IWorld worldDataService = worldDataService;

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<PaginatedResponse<CreatureInfo>> Get(
        [FromQuery] int pageNumber = DefaultPageNumber,
        [FromQuery] int pageSize = DefaultPageSize,
        [FromQuery] string? search = null)
    {
        // Validate pagination parameters
        if (pageNumber <= 0 || pageSize <= 0)
        {
            return BadRequest("Page number and page size must be greater than zero.");
        }

        // Filter world objects
        var filteredCreatureInfos = string.IsNullOrWhiteSpace(search) ?
            worldDataService.CreatureInfos :
            worldDataService.CreatureInfos.Where(ci =>
                        ci.NameSingular.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        ci.NamePlural.Contains(search, StringComparison.OrdinalIgnoreCase));

        // Get total number of elements
        int totalElements = worldDataService.CreatureInfos.Count;

        // Get total number of filtered elements
        int totalFilteredElements = filteredCreatureInfos.Count();

        // Calculate how many elements to skip based on the page number and size
        var paginatedElements = filteredCreatureInfos
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        // Create a response object to include pagination metadata
        var response = new PaginatedResponse<CreatureInfo>
        {
            Items = paginatedElements,
            TotalCount = totalElements,
            TotalFilteredCount = totalFilteredElements,
            PageSize = pageSize,
            PageNumber = pageNumber,
            TotalPages = (int)Math.Ceiling(totalElements / (double)pageSize)
        };

        return Ok(response);
    }
}
