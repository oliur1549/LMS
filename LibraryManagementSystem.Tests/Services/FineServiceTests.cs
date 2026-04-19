using LibraryManagementSystem.Data;
using LibraryManagementSystem.DTOs.Fine;
using LibraryManagementSystem.Enums;
using LibraryManagementSystem.Entities;
using LibraryManagementSystem.Repositories;
using LibraryManagementSystem.Services.Fines;
using LibraryManagementSystem.Tests.Helpers;
using Xunit;

namespace LibraryManagementSystem.Tests.Services
{
    public class FineServiceTests
    {
        private static FineService CreateService(ApplicationDbContext ctx) =>
            new(new Repository<Fine>(ctx), ctx);

        private static async Task<(Member member, Book book, BorrowRecord record)> SeedOverdueRecordAsync(
            ApplicationDbContext ctx, int daysOverdue = 5)
        {
            var mt = new MembershipType { Name = "Standard", MaxBooksAllowed = 3, BorrowDurationDays = 14 };
            var lib = new Library { Name = "Library", Address = "1 St", Phone = "555", Email = "lib@t.com" };
            ctx.MembershipTypes.Add(mt);
            ctx.Libraries.Add(lib);
            await ctx.SaveChangesAsync();

            var member = new Member { FullName = "Alice", Email = "alice@test.com", IsActive = true, MembershipTypeId = mt.Id };
            var book = new Book
            {
                Title = "DDIA", Author = "Kleppmann", ISBN = "ISBN002",
                Genre = "Tech", TotalCopies = 2, AvailableCopies = 1, LibraryId = lib.Id
            };
            ctx.Members.Add(member);
            ctx.Books.Add(book);
            await ctx.SaveChangesAsync();

            var record = new BorrowRecord
            {
                BookId = book.Id, MemberId = member.Id,
                BorrowDate = DateTime.UtcNow.AddDays(-20),
                DueDate = DateTime.UtcNow.AddDays(-daysOverdue),
                Status = BorrowStatus.Overdue
            };
            ctx.BorrowRecords.Add(record);
            await ctx.SaveChangesAsync();

            return (member, book, record);
        }

        // ── IssueAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task IssueAsync_BorrowRecordNotFound_ThrowsKeyNotFoundException()
        {
            using var ctx = DbContextFactory.Create();
            var service = CreateService(ctx);
            var dto = new CreateFineDto { BorrowRecordId = 999, DailyRate = 1.50m };

            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.IssueAsync(dto));
        }

        [Fact]
        public async Task IssueAsync_ReturnedOnTime_ThrowsInvalidOperationException()
        {
            using var ctx = DbContextFactory.Create();
            var (member, book, _) = await SeedOverdueRecordAsync(ctx);
            // Replace with a record that was returned before its due date
            var onTimeRecord = new BorrowRecord
            {
                BookId = book.Id, MemberId = member.Id,
                BorrowDate = DateTime.UtcNow.AddDays(-7),
                DueDate = DateTime.UtcNow.AddDays(3),     // due in the future
                ReturnDate = DateTime.UtcNow.AddDays(-1), // returned yesterday → on time
                Status = BorrowStatus.Returned
            };
            ctx.BorrowRecords.Add(onTimeRecord);
            await ctx.SaveChangesAsync();
            var service = CreateService(ctx);
            var dto = new CreateFineDto { BorrowRecordId = onTimeRecord.Id, DailyRate = 1.50m };

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.IssueAsync(dto));
        }

        [Fact]
        public async Task IssueAsync_FineAlreadyExists_ThrowsInvalidOperationException()
        {
            using var ctx = DbContextFactory.Create();
            var (member, _, record) = await SeedOverdueRecordAsync(ctx);
            var existingFine = new Fine
            {
                BorrowRecordId = record.Id, MemberId = member.Id,
                OverdueDays = 5, DailyRate = 1.00m, Amount = 5.00m,
                IssuedDate = DateTime.UtcNow
            };
            ctx.Fines.Add(existingFine);
            await ctx.SaveChangesAsync();
            var service = CreateService(ctx);
            var dto = new CreateFineDto { BorrowRecordId = record.Id, DailyRate = 1.50m };

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.IssueAsync(dto));
        }

        [Fact]
        public async Task IssueAsync_ValidOverdueRecord_CreatesFineWithCorrectAmount()
        {
            using var ctx = DbContextFactory.Create();
            var (_, _, record) = await SeedOverdueRecordAsync(ctx, daysOverdue: 5);
            var service = CreateService(ctx);
            var dto = new CreateFineDto { BorrowRecordId = record.Id, DailyRate = 2.00m };

            var result = await service.IssueAsync(dto);

            Assert.NotNull(result);
            Assert.Equal(5, result.OverdueDays);
            Assert.Equal(10.00m, result.Amount); // 5 days × $2.00
            Assert.False(result.IsPaid);
        }

        // ── PayAsync ─────────────────────────────────────────────────────────

        [Fact]
        public async Task PayAsync_FineNotFound_ReturnsNull()
        {
            using var ctx = DbContextFactory.Create();
            var service = CreateService(ctx);

            var result = await service.PayAsync(999);

            Assert.Null(result);
        }

        [Fact]
        public async Task PayAsync_AlreadyPaid_ThrowsInvalidOperationException()
        {
            using var ctx = DbContextFactory.Create();
            var (member, _, record) = await SeedOverdueRecordAsync(ctx);
            var fine = new Fine
            {
                BorrowRecordId = record.Id, MemberId = member.Id,
                OverdueDays = 5, DailyRate = 1.00m, Amount = 5.00m,
                IsPaid = true, PaidDate = DateTime.UtcNow, IssuedDate = DateTime.UtcNow
            };
            ctx.Fines.Add(fine);
            await ctx.SaveChangesAsync();
            var service = CreateService(ctx);

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.PayAsync(fine.Id));
        }

        [Fact]
        public async Task PayAsync_ValidFine_MarksFineAsPaidWithPaidDate()
        {
            using var ctx = DbContextFactory.Create();
            var (member, _, record) = await SeedOverdueRecordAsync(ctx);
            var fine = new Fine
            {
                BorrowRecordId = record.Id, MemberId = member.Id,
                OverdueDays = 5, DailyRate = 1.00m, Amount = 5.00m,
                IsPaid = false, IssuedDate = DateTime.UtcNow
            };
            ctx.Fines.Add(fine);
            await ctx.SaveChangesAsync();
            var service = CreateService(ctx);

            var result = await service.PayAsync(fine.Id);

            Assert.NotNull(result);
            Assert.True(result.IsPaid);
            Assert.NotNull(result.PaidDate);
        }
    }
}
