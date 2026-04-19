using LibraryManagementSystem.Data;
using LibraryManagementSystem.DTOs.Common;
using LibraryManagementSystem.DTOs.Reservation;
using LibraryManagementSystem.Enums;
using LibraryManagementSystem.Entities;
using LibraryManagementSystem.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Services.Reservations
{
    public class ReservationService : IReservationService
    {
        private readonly IRepository<Reservation> _reservationRepository;
        private readonly IRepository<Book> _bookRepository;
        private readonly IRepository<Member> _memberRepository;
        private readonly ApplicationDbContext _context;

        public ReservationService(
            IRepository<Reservation> reservationRepository,
            IRepository<Book> bookRepository,
            IRepository<Member> memberRepository,
            ApplicationDbContext context)
        {
            _reservationRepository = reservationRepository;
            _bookRepository = bookRepository;
            _memberRepository = memberRepository;
            _context = context;
        }

        public async Task<PagedResult<ReservationDto>> GetAllAsync(ReservationFilterDto filter)
        {
            var query = _context.Reservations
                .Include(r => r.Member)
                .Include(r => r.Book)
                .AsQueryable();

            if (filter.MemberId.HasValue)
                query = query.Where(r => r.MemberId == filter.MemberId.Value);

            if (filter.BookId.HasValue)
                query = query.Where(r => r.BookId == filter.BookId.Value);

            if (filter.Status.HasValue)
                query = query.Where(r => r.Status == filter.Status.Value);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            foreach (var r in items) r.CheckAndExpire();
            await _context.SaveChangesAsync();

            return new PagedResult<ReservationDto>
            {
                Data = items.Select(MapToDto),
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }

        public async Task<ReservationDto?> GetByIdAsync(int id)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Member)
                .Include(r => r.Book)
                .FirstOrDefaultAsync(r => r.Id == id);

            return reservation == null ? null : MapToDto(reservation);
        }

        public async Task<ReservationDto> CreateAsync(CreateReservationDto dto)
        {
            var member = await _memberRepository.GetByIdAsync(dto.MemberId)
                ?? throw new KeyNotFoundException($"Member with ID {dto.MemberId} not found.");

            if (!member.CanBorrow())
                throw new InvalidOperationException("Member is not active and cannot make reservations.");

            var book = await _bookRepository.GetByIdAsync(dto.BookId)
                ?? throw new KeyNotFoundException($"Book with ID {dto.BookId} not found.");

            if (book.AvailableCopies > 0)
                throw new InvalidOperationException("Book has available copies. Please borrow it directly instead of reserving.");

            var alreadyReserved = await _context.Reservations.AnyAsync(r =>
                r.MemberId == dto.MemberId &&
                r.BookId == dto.BookId &&
                r.Status == ReservationStatus.Pending);

            if (alreadyReserved)
                throw new InvalidOperationException("Member already has a pending reservation for this book.");

            var reservation = new Reservation
            {
                MemberId = dto.MemberId,
                BookId = dto.BookId,
                ReservedDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddDays(7)
            };

            var created = await _reservationRepository.AddAsync(reservation);
            return await GetByIdAsync(created.Id) ?? MapToDto(created);
        }

        public async Task<ReservationDto?> CancelAsync(int id)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Member)
                .Include(r => r.Book)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null) return null;

            reservation.Cancel();
            await _context.SaveChangesAsync();
            return MapToDto(reservation);
        }

        public async Task<ReservationDto?> FulfilAsync(int id)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Member)
                .Include(r => r.Book)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null) return null;

            reservation.Fulfil();
            await _context.SaveChangesAsync();
            return MapToDto(reservation);
        }

        private static ReservationDto MapToDto(Reservation r) => new()
        {
            Id = r.Id,
            MemberId = r.MemberId,
            MemberName = r.Member?.FullName ?? string.Empty,
            BookId = r.BookId,
            BookTitle = r.Book?.Title ?? string.Empty,
            ReservedDate = r.ReservedDate,
            ExpiryDate = r.ExpiryDate,
            Status = r.Status.ToString()
        };
    }
}
