using LibraryManagementSystem.DTOs.BorrowRecord;
using LibraryManagementSystem.DTOs.Common;
using LibraryManagementSystem.Services.BorrowRecords;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BorrowRecordsController : ControllerBase
    {
        private readonly IBorrowService _borrowService;

        public BorrowRecordsController(IBorrowService borrowService)
        {
            _borrowService = borrowService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] BorrowRecordFilterDto filter)
        {
            var result = await _borrowService.GetAllAsync(filter);
            return Ok(PagedApiResponse<BorrowRecordDto>.SuccessResult(result));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var record = await _borrowService.GetByIdAsync(id);
            if (record == null)
                return NotFound(ApiResponse<BorrowRecordDto>.FailResult($"Borrow record with ID {id} not found."));
            return Ok(ApiResponse<BorrowRecordDto>.SuccessResult(record));
        }

        [HttpPost("borrow")]
        public async Task<IActionResult> BorrowBook([FromBody] BorrowBookDto dto)
        {
            var record = await _borrowService.BorrowBookAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = record.Id },
                ApiResponse<BorrowRecordDto>.SuccessResult(record, "Book borrowed successfully."));
        }

        [HttpPut("{id}/return")]
        public async Task<IActionResult> ReturnBook(int id)
        {
            var record = await _borrowService.ReturnBookAsync(id);
            if (record == null)
                return NotFound(ApiResponse<BorrowRecordDto>.FailResult($"Borrow record with ID {id} not found."));
            return Ok(ApiResponse<BorrowRecordDto>.SuccessResult(record, "Book returned successfully."));
        }
    }
}
