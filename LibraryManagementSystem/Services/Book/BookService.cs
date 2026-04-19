using LibraryManagementSystem.Data;
using LibraryManagementSystem.DTOs.Book;
using LibraryManagementSystem.DTOs.Common;
using LibraryManagementSystem.Entities;
using LibraryManagementSystem.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Services.Books
{
    public class BookService : IBookService
    {
        private readonly IRepository<Book> _bookRepository;
        private readonly ApplicationDbContext _context;

        public BookService(IRepository<Book> bookRepository, ApplicationDbContext context)
        {
            _bookRepository = bookRepository;
            _context = context;
        }

        public async Task<PagedResult<BookDto>> GetAllAsync(BookFilterDto filter)
        {
            var query = _context.Books.Include(b => b.Library).AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.Title))
                query = query.Where(b => b.Title.Contains(filter.Title));

            if (!string.IsNullOrWhiteSpace(filter.Author))
                query = query.Where(b => b.Author.Contains(filter.Author));

            if (!string.IsNullOrWhiteSpace(filter.Genre))
                query = query.Where(b => b.Genre.Contains(filter.Genre));

            if (filter.LibraryId.HasValue)
                query = query.Where(b => b.LibraryId == filter.LibraryId.Value);

            if (filter.IsAvailable.HasValue)
                query = query.Where(b => filter.IsAvailable.Value ? b.AvailableCopies > 0 : b.AvailableCopies == 0);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return new PagedResult<BookDto>
            {
                Data = items.Select(MapToDto),
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }

        public async Task<BookDto?> GetByIdAsync(int id)
        {
            var book = await _context.Books.Include(b => b.Library)
                                           .FirstOrDefaultAsync(b => b.Id == id);
            return book == null ? null : MapToDto(book);
        }

        public async Task<BookDto> CreateAsync(CreateBookDto dto)
        {
            var isbnExists = await _context.Books.AnyAsync(b => b.ISBN == dto.ISBN);
            if (isbnExists)
                throw new InvalidOperationException($"A book with ISBN '{dto.ISBN}' already exists.");

            var book = new Book
            {
                Title = dto.Title,
                Author = dto.Author,
                ISBN = dto.ISBN,
                Genre = dto.Genre,
                PublishedYear = dto.PublishedYear,
                TotalCopies = dto.TotalCopies,
                AvailableCopies = dto.TotalCopies,
                LibraryId = dto.LibraryId
            };

            var created = await _bookRepository.AddAsync(book);
            return await GetByIdAsync(created.Id) ?? MapToDto(created);
        }

        public async Task<BookDto?> UpdateAsync(UpdateBookDto dto)
        {
            var book = await _bookRepository.GetByIdAsync(dto.Id);
            if (book == null) return null;

            var difference = dto.TotalCopies - book.TotalCopies;
            book.Title = dto.Title;
            book.Author = dto.Author;
            book.ISBN = dto.ISBN;
            book.Genre = dto.Genre;
            book.PublishedYear = dto.PublishedYear;
            book.TotalCopies = dto.TotalCopies;
            book.LibraryId = dto.LibraryId;
            book.AvailableCopies = Math.Max(0, book.AvailableCopies + difference);

            await _bookRepository.UpdateAsync(book);
            return await GetByIdAsync(book.Id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            if (!await _bookRepository.ExistsAsync(id)) return false;
            await _bookRepository.DeleteAsync(id);
            return true;
        }

        private static BookDto MapToDto(Book book) => new()
        {
            Id = book.Id,
            Title = book.Title,
            Author = book.Author,
            ISBN = book.ISBN,
            Genre = book.Genre,
            PublishedYear = book.PublishedYear,
            TotalCopies = book.TotalCopies,
            AvailableCopies = book.AvailableCopies,
            LibraryId = book.LibraryId,
            LibraryName = book.Library?.Name ?? string.Empty
        };
    }
}
