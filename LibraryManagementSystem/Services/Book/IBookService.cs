using LibraryManagementSystem.DTOs.Book;
using LibraryManagementSystem.DTOs.Common;

namespace LibraryManagementSystem.Services.Books
{
    public interface IBookService
    {
        Task<PagedResult<BookDto>> GetAllAsync(BookFilterDto filter);
        Task<BookDto?> GetByIdAsync(int id);
        Task<BookDto> CreateAsync(CreateBookDto dto);
        Task<BookDto?> UpdateAsync(UpdateBookDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
