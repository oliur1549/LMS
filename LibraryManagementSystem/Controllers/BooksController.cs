using LibraryManagementSystem.DTOs.Book;
using LibraryManagementSystem.DTOs.Common;
using LibraryManagementSystem.Services.Books;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BooksController : ControllerBase
    {
        private readonly IBookService _bookService;

        public BooksController(IBookService bookService)
        {
            _bookService = bookService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] BookFilterDto filter)
        {
            var result = await _bookService.GetAllAsync(filter);
            return Ok(PagedApiResponse<BookDto>.SuccessResult(result));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var book = await _bookService.GetByIdAsync(id);
            if (book == null)
                return NotFound(ApiResponse<BookDto>.FailResult($"Book with ID {id} not found."));
            return Ok(ApiResponse<BookDto>.SuccessResult(book));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateBookDto dto)
        {
            var book = await _bookService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = book.Id },
                ApiResponse<BookDto>.SuccessResult(book, "Book created successfully."));
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateBookDto dto)
        {
            var book = await _bookService.UpdateAsync(dto);
            if (book == null)
                return NotFound(ApiResponse<BookDto>.FailResult("Data not found."));
            return Ok(ApiResponse<BookDto>.SuccessResult(book, "Book updated successfully."));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _bookService.DeleteAsync(id);
            if (!result)
                return NotFound(ApiResponse<object>.FailResult($"Book with ID {id} not found."));
            return NoContent();
        }
    }
}
