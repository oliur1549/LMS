using LibraryManagementSystem.DTOs.Common;
using LibraryManagementSystem.DTOs.Library;
using LibraryManagementSystem.Services.Libraries;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LibrariesController : ControllerBase
    {
        private readonly ILibraryService _libraryService;

        public LibrariesController(ILibraryService libraryService)
        {
            _libraryService = libraryService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] LibraryFilterDto filter)
        {
            var result = await _libraryService.GetAllAsync(filter);
            return Ok(PagedApiResponse<LibraryDto>.SuccessResult(result));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var library = await _libraryService.GetByIdAsync(id);
            if (library == null)
                return NotFound(ApiResponse<LibraryDto>.FailResult($"Library with ID {id} not found."));
            return Ok(ApiResponse<LibraryDto>.SuccessResult(library));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateLibraryDto dto)
        {
            var library = await _libraryService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = library.Id },
                ApiResponse<LibraryDto>.SuccessResult(library, "Library created successfully."));
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateLibraryDto dto)
        {
            var library = await _libraryService.UpdateAsync(dto);
            if (library == null)
                return NotFound(ApiResponse<LibraryDto>.FailResult("Data not found."));
            return Ok(ApiResponse<LibraryDto>.SuccessResult(library, "Library updated successfully."));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _libraryService.DeleteAsync(id);
            if (!result)
                return NotFound(ApiResponse<object>.FailResult($"Library with ID {id} not found."));
            return NoContent();
        }
    }
}
