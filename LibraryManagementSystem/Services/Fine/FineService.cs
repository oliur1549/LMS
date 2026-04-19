using LibraryManagementSystem.Data;
using LibraryManagementSystem.DTOs.Common;
using LibraryManagementSystem.DTOs.Fine;
using LibraryManagementSystem.Enums;
using LibraryManagementSystem.Entities;
using LibraryManagementSystem.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Services.Fines
{
    public class FineService : IFineService
    {
        private readonly IRepository<Fine> _fineRepository;
        private readonly ApplicationDbContext _context;

        public FineService(IRepository<Fine> fineRepository, ApplicationDbContext context)
        {
            _fineRepository = fineRepository;
            _context = context;
        }

        public async Task<PagedResult<FineDto>> GetAllAsync(FineFilterDto filter)
        {
            var query = _context.Fines
                .Include(f => f.Member)
                .Include(f => f.BorrowRecord).ThenInclude(br => br.Book)
                .AsQueryable();

            if (filter.MemberId.HasValue)
                query = query.Where(f => f.MemberId == filter.MemberId.Value);

            if (filter.IsPaid.HasValue)
                query = query.Where(f => f.IsPaid == filter.IsPaid.Value);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return new PagedResult<FineDto>
            {
                Data = items.Select(MapToDto),
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }

        public async Task<FineDto?> GetByIdAsync(int id)
        {
            var fine = await _context.Fines
                .Include(f => f.Member)
                .Include(f => f.BorrowRecord).ThenInclude(br => br.Book)
                .FirstOrDefaultAsync(f => f.Id == id);

            return fine == null ? null : MapToDto(fine);
        }

        public async Task<FineDto> IssueAsync(CreateFineDto dto)
        {
            var borrowRecord = await _context.BorrowRecords
                .Include(br => br.Book)
                .Include(br => br.Member)
                .FirstOrDefaultAsync(br => br.Id == dto.BorrowRecordId)
                ?? throw new KeyNotFoundException($"Borrow record with ID {dto.BorrowRecordId} not found.");

            if (borrowRecord.Status == BorrowStatus.Returned && borrowRecord.ReturnDate <= borrowRecord.DueDate)
                throw new InvalidOperationException("This borrow record has no overdue days. Fine cannot be issued.");

            var alreadyHasFine = await _context.Fines.AnyAsync(f => f.BorrowRecordId == dto.BorrowRecordId);
            if (alreadyHasFine)
                throw new InvalidOperationException("A fine has already been issued for this borrow record.");

            var referenceDate = borrowRecord.ReturnDate ?? DateTime.UtcNow;
            var overdueDays = Math.Max(0, (int)(referenceDate - borrowRecord.DueDate).TotalDays);

            if (overdueDays == 0)
                throw new InvalidOperationException("No overdue days found. Fine cannot be issued.");

            var fine = new Fine
            {
                BorrowRecordId = dto.BorrowRecordId,
                MemberId = borrowRecord.MemberId,
                OverdueDays = overdueDays,
                DailyRate = dto.DailyRate,
                Amount = overdueDays * dto.DailyRate,
                IssuedDate = DateTime.UtcNow
            };

            var created = await _fineRepository.AddAsync(fine);
            return await GetByIdAsync(created.Id) ?? MapToDto(created);
        }

        public async Task<FineDto?> PayAsync(int id)
        {
            var fine = await _context.Fines
                .Include(f => f.Member)
                .Include(f => f.BorrowRecord).ThenInclude(br => br.Book)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (fine == null) return null;

            fine.MarkAsPaid();
            await _context.SaveChangesAsync();
            return MapToDto(fine);
        }

        private static FineDto MapToDto(Fine f) => new()
        {
            Id = f.Id,
            BorrowRecordId = f.BorrowRecordId,
            MemberId = f.MemberId,
            MemberName = f.Member?.FullName ?? string.Empty,
            BookTitle = f.BorrowRecord?.Book?.Title ?? string.Empty,
            OverdueDays = f.OverdueDays,
            DailyRate = f.DailyRate,
            Amount = f.Amount,
            IssuedDate = f.IssuedDate,
            PaidDate = f.PaidDate,
            IsPaid = f.IsPaid
        };
    }
}
