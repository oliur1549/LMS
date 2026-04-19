using LibraryManagementSystem.Data;
using LibraryManagementSystem.DTOs.BorrowRecord;
using LibraryManagementSystem.Enums;
using LibraryManagementSystem.Entities;
using LibraryManagementSystem.Repositories;
using LibraryManagementSystem.Services.BorrowRecords;
using LibraryManagementSystem.Tests.Helpers;
using Xunit;

namespace LibraryManagementSystem.Tests.Services
{
    public class BorrowServiceTests
    {
        private static BorrowService CreateService(ApplicationDbContext ctx) =>
            new(new Repository<BorrowRecord>(ctx),
                new Repository<Book>(ctx),
                new Repository<Member>(ctx),
                ctx);

        private static async Task<(Member member, Book book, MembershipType membershipType)> SeedAsync(
            ApplicationDbContext ctx, bool memberActive = true, int availableCopies = 2,
            int maxBooksAllowed = 3, int borrowDurationDays = 14)
        {
            var mt = new MembershipType { Name = "Standard", MaxBooksAllowed = maxBooksAllowed, BorrowDurationDays = borrowDurationDays };
            var lib = new Library { Name = "Library", Address = "1 St", Phone = "555", Email = "lib@t.com" };
            ctx.MembershipTypes.Add(mt);
            ctx.Libraries.Add(lib);
            await ctx.SaveChangesAsync();

            var member = new Member
            {
                FullName = "John Doe", Email = "john@test.com",
                IsActive = memberActive, MembershipTypeId = mt.Id
            };
            var book = new Book
            {
                Title = "Clean Code", Author = "Martin",
                ISBN = "ISBN001", Genre = "Tech",
                TotalCopies = 2, AvailableCopies = availableCopies,
                LibraryId = lib.Id
            };
            ctx.Members.Add(member);
            ctx.Books.Add(book);
            await ctx.SaveChangesAsync();

            return (member, book, mt);
        }

        // ── BorrowBookAsync ──────────────────────────────────────────────────

        [Fact]
        public async Task BorrowBookAsync_BookNotFound_ThrowsKeyNotFoundException()
        {
            using var ctx = DbContextFactory.Create();
            var service = CreateService(ctx);
            var dto = new BorrowBookDto { BookId = 999, MemberId = 1 };

            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.BorrowBookAsync(dto));
        }

        [Fact]
        public async Task BorrowBookAsync_MemberNotFound_ThrowsKeyNotFoundException()
        {
            using var ctx = DbContextFactory.Create();
            var (_, book, _) = await SeedAsync(ctx);
            var service = CreateService(ctx);
            var dto = new BorrowBookDto { BookId = book.Id, MemberId = 999 };

            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.BorrowBookAsync(dto));
        }

        [Fact]
        public async Task BorrowBookAsync_MemberInactive_ThrowsInvalidOperationException()
        {
            using var ctx = DbContextFactory.Create();
            var (member, book, _) = await SeedAsync(ctx, memberActive: false);
            var service = CreateService(ctx);
            var dto = new BorrowBookDto { BookId = book.Id, MemberId = member.Id };

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.BorrowBookAsync(dto));
        }

        [Fact]
        public async Task BorrowBookAsync_NoCopiesAvailable_ThrowsInvalidOperationException()
        {
            using var ctx = DbContextFactory.Create();
            var (member, book, _) = await SeedAsync(ctx, availableCopies: 0);
            var service = CreateService(ctx);
            var dto = new BorrowBookDto { BookId = book.Id, MemberId = member.Id };

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.BorrowBookAsync(dto));
        }

        [Fact]
        public async Task BorrowBookAsync_MaxBooksAllowedReached_ThrowsInvalidOperationException()
        {
            using var ctx = DbContextFactory.Create();
            var (member, book, _) = await SeedAsync(ctx, maxBooksAllowed: 1);
            ctx.BorrowRecords.Add(new BorrowRecord
            {
                BookId = book.Id, MemberId = member.Id,
                BorrowDate = DateTime.UtcNow.AddDays(-3),
                DueDate = DateTime.UtcNow.AddDays(11),
                Status = BorrowStatus.Borrowed
            });
            await ctx.SaveChangesAsync();
            var service = CreateService(ctx);
            var dto = new BorrowBookDto { BookId = book.Id, MemberId = member.Id };

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.BorrowBookAsync(dto));
        }

        [Fact]
        public async Task BorrowBookAsync_ValidRequest_UsesMembershipTypeBorrowDuration()
        {
            using var ctx = DbContextFactory.Create();
            var (member, book, mt) = await SeedAsync(ctx, borrowDurationDays: 30);
            var service = CreateService(ctx);
            var dto = new BorrowBookDto { BookId = book.Id, MemberId = member.Id };
            var before = DateTime.UtcNow;

            var result = await service.BorrowBookAsync(dto);

            Assert.NotNull(result);
            var expectedDue = before.AddDays(mt.BorrowDurationDays);
            Assert.True(result.DueDate >= expectedDue.AddSeconds(-5) && result.DueDate <= expectedDue.AddDays(1));
        }

        [Fact]
        public async Task BorrowBookAsync_ValidRequest_ReturnsBorrowRecordDto()
        {
            using var ctx = DbContextFactory.Create();
            var (member, book, _) = await SeedAsync(ctx);
            var service = CreateService(ctx);
            var dto = new BorrowBookDto { BookId = book.Id, MemberId = member.Id };

            var result = await service.BorrowBookAsync(dto);

            Assert.NotNull(result);
            Assert.Equal(book.Id, result.BookId);
            Assert.Equal(member.Id, result.MemberId);
            Assert.Equal("Borrowed", result.Status);
        }

        [Fact]
        public async Task BorrowBookAsync_ValidRequest_DecrementsAvailableCopies()
        {
            using var ctx = DbContextFactory.Create();
            var (member, book, _) = await SeedAsync(ctx, availableCopies: 2);
            var service = CreateService(ctx);
            var dto = new BorrowBookDto { BookId = book.Id, MemberId = member.Id };

            await service.BorrowBookAsync(dto);

            var updated = await ctx.Books.FindAsync(book.Id);
            Assert.Equal(1, updated!.AvailableCopies);
        }

        // ── ReturnBookAsync ──────────────────────────────────────────────────

        [Fact]
        public async Task ReturnBookAsync_RecordNotFound_ReturnsNull()
        {
            using var ctx = DbContextFactory.Create();
            var service = CreateService(ctx);

            var result = await service.ReturnBookAsync(999);

            Assert.Null(result);
        }

        [Fact]
        public async Task ReturnBookAsync_AlreadyReturned_ThrowsInvalidOperationException()
        {
            using var ctx = DbContextFactory.Create();
            var (member, book, _) = await SeedAsync(ctx);
            var record = new BorrowRecord
            {
                BookId = book.Id, MemberId = member.Id,
                BorrowDate = DateTime.UtcNow.AddDays(-7),
                DueDate = DateTime.UtcNow.AddDays(7),
                ReturnDate = DateTime.UtcNow,
                Status = BorrowStatus.Returned
            };
            ctx.BorrowRecords.Add(record);
            await ctx.SaveChangesAsync();
            var service = CreateService(ctx);

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.ReturnBookAsync(record.Id));
        }

        [Fact]
        public async Task ReturnBookAsync_ValidRequest_SetsStatusAndReturnDate()
        {
            using var ctx = DbContextFactory.Create();
            var (member, book, _) = await SeedAsync(ctx, availableCopies: 1); // 1 copy borrowed out
            var record = new BorrowRecord
            {
                BookId = book.Id, MemberId = member.Id,
                BorrowDate = DateTime.UtcNow.AddDays(-7),
                DueDate = DateTime.UtcNow.AddDays(7),
                Status = BorrowStatus.Borrowed
            };
            ctx.BorrowRecords.Add(record);
            await ctx.SaveChangesAsync();
            var service = CreateService(ctx);

            var result = await service.ReturnBookAsync(record.Id);

            Assert.NotNull(result);
            Assert.Equal("Returned", result.Status);
            Assert.NotNull(result.ReturnDate);
        }

        [Fact]
        public async Task ReturnBookAsync_ValidRequest_IncrementsAvailableCopies()
        {
            using var ctx = DbContextFactory.Create();
            var (member, book, _) = await SeedAsync(ctx, availableCopies: 1);
            book.AvailableCopies = 0;
            await ctx.SaveChangesAsync();

            var record = new BorrowRecord
            {
                BookId = book.Id, MemberId = member.Id,
                BorrowDate = DateTime.UtcNow.AddDays(-7),
                DueDate = DateTime.UtcNow.AddDays(7),
                Status = BorrowStatus.Borrowed
            };
            ctx.BorrowRecords.Add(record);
            await ctx.SaveChangesAsync();
            var service = CreateService(ctx);

            await service.ReturnBookAsync(record.Id);

            var updated = await ctx.Books.FindAsync(book.Id);
            Assert.Equal(1, updated!.AvailableCopies);
        }
    }
}
