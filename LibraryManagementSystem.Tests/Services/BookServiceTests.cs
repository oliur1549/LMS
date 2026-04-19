using LibraryManagementSystem.Data;
using LibraryManagementSystem.DTOs.Book;
using LibraryManagementSystem.Entities;
using LibraryManagementSystem.Repositories;
using LibraryManagementSystem.Services.Books;
using LibraryManagementSystem.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LibraryManagementSystem.Tests.Services
{
    public class BookServiceTests
    {
        private static BookService CreateService(ApplicationDbContext ctx) =>
            new(new Repository<Book>(ctx), ctx);

        private static async Task<Library> SeedLibraryAsync(ApplicationDbContext ctx)
        {
            var lib = new Library { Name = "Main Library", Address = "1 St", Phone = "555", Email = "lib@t.com" };
            ctx.Libraries.Add(lib);
            await ctx.SaveChangesAsync();
            return lib;
        }

        private static CreateBookDto BuildDto(int libraryId, string isbn = "9781234567890") => new()
        {
            Title = "Clean Code",
            Author = "Robert C. Martin",
            ISBN = isbn,
            Genre = "Tech",
            PublishedYear = 2008,
            TotalCopies = 3,
            LibraryId = libraryId
        };

        // ── CreateAsync ──────────────────────────────────────────────────────

        [Fact]
        public async Task CreateAsync_ValidBook_ReturnsBookDtoWithCorrectData()
        {
            using var ctx = DbContextFactory.Create();
            var lib = await SeedLibraryAsync(ctx);
            var service = CreateService(ctx);
            var dto = BuildDto(lib.Id);

            var result = await service.CreateAsync(dto);

            Assert.NotNull(result);
            Assert.Equal(dto.Title, result.Title);
            Assert.Equal(dto.ISBN, result.ISBN);
            Assert.Equal(dto.TotalCopies, result.TotalCopies);
            Assert.Equal(dto.TotalCopies, result.AvailableCopies); // AvailableCopies = TotalCopies on create
        }

        [Fact]
        public async Task CreateAsync_DuplicateISBN_ThrowsInvalidOperationException()
        {
            using var ctx = DbContextFactory.Create();
            var lib = await SeedLibraryAsync(ctx);
            var service = CreateService(ctx);
            var dto = BuildDto(lib.Id, isbn: "DUPLICATE-ISBN");

            await service.CreateAsync(dto); // first create succeeds

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(dto));
        }

        // ── GetAllAsync ──────────────────────────────────────────────────────

        [Fact]
        public async Task GetAllAsync_WithTitleFilter_ReturnsOnlyMatchingBooks()
        {
            using var ctx = DbContextFactory.Create();
            var lib = await SeedLibraryAsync(ctx);
            ctx.Books.AddRange(
                new Book { Title = "Clean Code", Author = "Martin", ISBN = "ISBN-A", Genre = "Tech", TotalCopies = 1, AvailableCopies = 1, LibraryId = lib.Id },
                new Book { Title = "Clean Architecture", Author = "Martin", ISBN = "ISBN-B", Genre = "Tech", TotalCopies = 1, AvailableCopies = 1, LibraryId = lib.Id },
                new Book { Title = "Domain-Driven Design", Author = "Evans", ISBN = "ISBN-C", Genre = "Tech", TotalCopies = 1, AvailableCopies = 1, LibraryId = lib.Id }
            );
            await ctx.SaveChangesAsync();
            var service = CreateService(ctx);
            var filter = new BookFilterDto { Title = "Clean", Page = 1, PageSize = 10 };

            var result = await service.GetAllAsync(filter);

            Assert.Equal(2, result.TotalCount);
            Assert.All(result.Data, b => Assert.Contains("Clean", b.Title));
        }

        [Fact]
        public async Task GetAllAsync_IsAvailableFilter_ReturnsOnlyAvailableBooks()
        {
            using var ctx = DbContextFactory.Create();
            var lib = await SeedLibraryAsync(ctx);
            ctx.Books.AddRange(
                new Book { Title = "Available Book", Author = "A", ISBN = "ISBN-X", Genre = "Tech", TotalCopies = 2, AvailableCopies = 2, LibraryId = lib.Id },
                new Book { Title = "Out of Stock", Author = "B", ISBN = "ISBN-Y", Genre = "Tech", TotalCopies = 2, AvailableCopies = 0, LibraryId = lib.Id }
            );
            await ctx.SaveChangesAsync();
            var service = CreateService(ctx);
            var filter = new BookFilterDto { IsAvailable = true, Page = 1, PageSize = 10 };

            var result = await service.GetAllAsync(filter);

            Assert.Equal(1, result.TotalCount);
            Assert.Equal("Available Book", result.Data.First().Title);
        }

        // ── UpdateAsync ──────────────────────────────────────────────────────

        [Fact]
        public async Task UpdateAsync_BookNotFound_ReturnsNull()
        {
            using var ctx = DbContextFactory.Create();
            var service = CreateService(ctx);
            var dto = new UpdateBookDto { Id = 999, Title = "X", Author = "X", ISBN = "X", Genre = "X", TotalCopies = 1, LibraryId = 1 };

            var result = await service.UpdateAsync(dto);

            Assert.Null(result);
        }

        // ── DeleteAsync ──────────────────────────────────────────────────────

        [Fact]
        public async Task DeleteAsync_BookNotFound_ReturnsFalse()
        {
            using var ctx = DbContextFactory.Create();
            var service = CreateService(ctx);

            var result = await service.DeleteAsync(999);

            Assert.False(result);
        }

        [Fact]
        public async Task DeleteAsync_ExistingBook_SoftDeletesAndReturnsTrue()
        {
            using var ctx = DbContextFactory.Create();
            var lib = await SeedLibraryAsync(ctx);
            var book = new Book
            {
                Title = "To Delete", Author = "Author", ISBN = "ISBN-DEL",
                Genre = "Tech", TotalCopies = 1, AvailableCopies = 1, LibraryId = lib.Id
            };
            ctx.Books.Add(book);
            await ctx.SaveChangesAsync();
            var service = CreateService(ctx);

            var result = await service.DeleteAsync(book.Id);

            Assert.True(result);
            // Verify soft delete — IgnoreQueryFilters bypasses the IsDeleted global filter
            var deleted = await ctx.Books.IgnoreQueryFilters().FirstOrDefaultAsync(b => b.Id == book.Id);
            Assert.NotNull(deleted);
            Assert.True(deleted!.IsDeleted);
        }
    }
}
