using LibraryManagementSystem.DTOs.Common;
using LibraryManagementSystem.DTOs.Fine;
using LibraryManagementSystem.Services.Fines;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FinesController : ControllerBase
    {
        private readonly IFineService _fineService;

        public FinesController(IFineService fineService)
        {
            _fineService = fineService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] FineFilterDto filter)
        {
            var result = await _fineService.GetAllAsync(filter);
            return Ok(PagedApiResponse<FineDto>.SuccessResult(result));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var fine = await _fineService.GetByIdAsync(id);
            if (fine == null)
                return NotFound(ApiResponse<FineDto>.FailResult("Data not found."));
            return Ok(ApiResponse<FineDto>.SuccessResult(fine));
        }

        [HttpPost("issue")]
        public async Task<IActionResult> Issue([FromBody] CreateFineDto dto)
        {
            var fine = await _fineService.IssueAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = fine.Id },
                ApiResponse<FineDto>.SuccessResult(fine, "Fine issued successfully."));
        }

        [HttpPatch("{id}/pay")]
        public async Task<IActionResult> Pay(int id)
        {
            var fine = await _fineService.PayAsync(id);
            if (fine == null)
                return NotFound(ApiResponse<FineDto>.FailResult("Data not found."));
            return Ok(ApiResponse<FineDto>.SuccessResult(fine, "Fine paid successfully."));
        }
    }
}
