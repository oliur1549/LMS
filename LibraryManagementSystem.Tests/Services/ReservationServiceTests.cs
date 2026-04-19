using LibraryManagementSystem.Data;
using LibraryManagementSystem.DTOs.Reservation;
using LibraryManagementSystem.Enums;
using LibraryManagementSystem.Entities;
using LibraryManagementSystem.Repositories;
using LibraryManagementSystem.Services.Reservations;
using LibraryManagementSystem.Tests.Helpers;
using Xunit;

namespace LibraryManagementSystem.Tests.Services
{
    public class ReservationServiceTests
    {
        private static ReservationService CreateService(ApplicationDbContext ctx) =>
            new(new Repository<Reservation>(ctx),
                new Repository<Book>(ctx),
                new Repository<Member>(ctx),
                ctx);

        private static async Task<(Member member, Book bookOutOfStock)> SeedAsync(
            ApplicationDbContext ctx, bool memberActive = true, int availableCopies = 0)
        {
            var mt = new MembershipType { Name = "Standard", MaxBooksAllowed = 3, BorrowDurationDays = 14 };
            var lib = new Library { Name = "Library", Address = "1 St", Phone = "555", Email = "lib@t.com" };
            ctx.MembershipTypes.Add(mt);
            ctx.Libraries.Add(lib);
            await ctx.SaveChangesAsync();

            var member = new Member
            {
                FullName = "Bob Smith", Email = "bob@test.com",
                IsActive = memberActive, MembershipTypeId = mt.Id
            };
            var book = new Book
            {
                Title = "The Pragmatic Programmer", Author = "Hunt",
                ISBN = "ISBN003", Genre = "Tech",
                TotalCopies = 2, AvailableCopies = availableCopies,
                LibraryId = lib.Id
            };
            ctx.Members.Add(member);
            ctx.Books.Add(book);
            await ctx.SaveChangesAsync();

            return (member, book);
        }

        // ── CreateAsync ──────────────────────────────────────────────────────

        [Fact]
        public async Task CreateAsync_MemberNotFound_ThrowsKeyNotFoundException()
        {
            using var ctx = DbContextFactory.Create();
            var service = CreateService(ctx);
            var dto = new CreateReservationDto { MemberId = 999, BookId = 1 };

            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.CreateAsync(dto));
        }

        [Fact]
        public async Task CreateAsync_MemberInactive_ThrowsInvalidOperationException()
        {
            using var ctx = DbContextFactory.Create();
            var (member, book) = await SeedAsync(ctx, memberActive: false);
            var service = CreateService(ctx);
            var dto = new CreateReservationDto { MemberId = member.Id, BookId = book.Id };

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(dto));
        }

        [Fact]
        public async Task CreateAsync_BookNotFound_ThrowsKeyNotFoundException()
        {
            using var ctx = DbContextFactory.Create();
            var (member, _) = await SeedAsync(ctx);
            var service = CreateService(ctx);
            var dto = new CreateReservationDto { MemberId = member.Id, BookId = 999 };

            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.CreateAsync(dto));
        }

        [Fact]
        public async Task CreateAsync_BookHasAvailableCopies_ThrowsInvalidOperationException()
        {
            using var ctx = DbContextFactory.Create();
            var (member, book) = await SeedAsync(ctx, availableCopies: 2);
            var service = CreateService(ctx);
            var dto = new CreateReservationDto { MemberId = member.Id, BookId = book.Id };

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(dto));
        }

        [Fact]
        public async Task CreateAsync_DuplicatePendingReservation_ThrowsInvalidOperationException()
        {
            using var ctx = DbContextFactory.Create();
            var (member, book) = await SeedAsync(ctx);
            ctx.Reservations.Add(new Reservation
            {
                MemberId = member.Id, BookId = book.Id,
                ReservedDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                Status = ReservationStatus.Pending
            });
            await ctx.SaveChangesAsync();
            var service = CreateService(ctx);
            var dto = new CreateReservationDto { MemberId = member.Id, BookId = book.Id };

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(dto));
        }

        [Fact]
        public async Task CreateAsync_ValidRequest_CreatesReservationWithPendingStatus()
        {
            using var ctx = DbContextFactory.Create();
            var (member, book) = await SeedAsync(ctx);
            var service = CreateService(ctx);
            var dto = new CreateReservationDto { MemberId = member.Id, BookId = book.Id };

            var result = await service.CreateAsync(dto);

            Assert.NotNull(result);
            Assert.Equal(member.Id, result.MemberId);
            Assert.Equal(book.Id, result.BookId);
            Assert.Equal("Pending", result.Status);
            Assert.True(result.ExpiryDate > DateTime.UtcNow.AddDays(6)); // 7-day expiry
        }

        // ── CancelAsync ──────────────────────────────────────────────────────

        [Fact]
        public async Task CancelAsync_ReservationNotFound_ReturnsNull()
        {
            using var ctx = DbContextFactory.Create();
            var service = CreateService(ctx);

            var result = await service.CancelAsync(999);

            Assert.Null(result);
        }

        [Fact]
        public async Task CancelAsync_AlreadyCancelled_ThrowsInvalidOperationException()
        {
            using var ctx = DbContextFactory.Create();
            var (member, book) = await SeedAsync(ctx);
            var reservation = new Reservation
            {
                MemberId = member.Id, BookId = book.Id,
                ReservedDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                Status = ReservationStatus.Cancelled
            };
            ctx.Reservations.Add(reservation);
            await ctx.SaveChangesAsync();
            var service = CreateService(ctx);

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.CancelAsync(reservation.Id));
        }

        // ── FulfilAsync ──────────────────────────────────────────────────────

        [Fact]
        public async Task FulfilAsync_ReservationNotFound_ReturnsNull()
        {
            using var ctx = DbContextFactory.Create();
            var service = CreateService(ctx);

            var result = await service.FulfilAsync(999);

            Assert.Null(result);
        }

        [Fact]
        public async Task FulfilAsync_AlreadyFulfilled_ThrowsInvalidOperationException()
        {
            using var ctx = DbContextFactory.Create();
            var (member, book) = await SeedAsync(ctx);
            var reservation = new Reservation
            {
                MemberId = member.Id, BookId = book.Id,
                ReservedDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                Status = ReservationStatus.Fulfilled
            };
            ctx.Reservations.Add(reservation);
            await ctx.SaveChangesAsync();
            var service = CreateService(ctx);

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.FulfilAsync(reservation.Id));
        }
    }
}
