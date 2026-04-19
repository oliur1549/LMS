using LibraryManagementSystem.Data;
using LibraryManagementSystem.DTOs.BorrowRecord;
using LibraryManagementSystem.DTOs.Common;
using LibraryManagementSystem.Enums;
using LibraryManagementSystem.Entities;
using LibraryManagementSystem.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Services.BorrowRecords
{
    public class BorrowService : IBorrowService
    {
        private readonly IRepository<BorrowRecord> _borrowRepository;
        private readonly IRepository<Book> _bookRepository;
        private readonly IRepository<Member> _memberRepository;
        private readonly ApplicationDbContext _context;

        public BorrowService(
            IRepository<BorrowRecord> borrowRepository,
            IRepository<Book> bookRepository,
            IRepository<Member> memberRepository,
            ApplicationDbContext context)
        {
            _borrowRepository = borrowRepository;
            _bookRepository = bookRepository;
            _memberRepository = memberRepository;
            _context = context;
        }

        public async Task<PagedResult<BorrowRecordDto>> GetAllAsync(BorrowRecordFilterDto filter)
        {
            var query = _context.BorrowRecords
                .Include(br => br.Book)
                .Include(br => br.Member)
                .AsQueryable();

            if (filter.MemberId.HasValue)
                query = query.Where(br => br.MemberId == filter.MemberId.Value);

            if (filter.BookId.HasValue)
                query = query.Where(br => br.BookId == filter.BookId.Value);

            if (filter.Status.HasValue)
                query = query.Where(br => br.Status == filter.Status.Value);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            foreach (var record in items) record.CheckAndUpdateOverdue();
            await _context.SaveChangesAsync();

            return new PagedResult<BorrowRecordDto>
            {
                Data = items.Select(MapToDto),
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }

        public async Task<BorrowRecordDto?> GetByIdAsync(int id)
        {
            var record = await _context.BorrowRecords
                                       .Include(br => br.Book)
                                       .Include(br => br.Member)
                                       .FirstOrDefaultAsync(br => br.Id == id);
            return record == null ? null : MapToDto(record);
        }

        public async Task<BorrowRecordDto> BorrowBookAsync(BorrowBookDto dto)
        {
            var book = await _bookRepository.GetByIdAsync(dto.BookId)
                ?? throw new KeyNotFoundException($"Book with ID {dto.BookId} not found.");

            // Load member with MembershipType to enforce borrow limits
            var member = await _context.Members
                .Include(m => m.MembershipType)
                .FirstOrDefaultAsync(m => m.Id == dto.MemberId && !m.IsDeleted)
                ?? throw new KeyNotFoundException($"Member with ID {dto.MemberId} not found.");

            if (!member.CanBorrow())
                throw new InvalidOperationException("Member is not active and cannot borrow books.");

            // Enforce MaxBooksAllowed
            var activeBorrows = await _context.BorrowRecords
                .CountAsync(br => br.MemberId == dto.MemberId &&
                                  (br.Status == BorrowStatus.Borrowed || br.Status == BorrowStatus.Overdue));

            if (activeBorrows >= member.MembershipType.MaxBooksAllowed)
                throw new InvalidOperationException(
                    $"Member has reached the borrow limit of {member.MembershipType.MaxBooksAllowed} book(s) allowed by their membership type.");

            book.BorrowCopy();

            var borrowRecord = new BorrowRecord
            {
                BookId = dto.BookId,
                MemberId = dto.MemberId,
                BorrowDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(member.MembershipType.BorrowDurationDays),
                Status = BorrowStatus.Borrowed
            };

            await _bookRepository.UpdateAsync(book);
            var created = await _borrowRepository.AddAsync(borrowRecord);
            return await GetByIdAsync(created.Id) ?? MapToDto(created);
        }

        public async Task<BorrowRecordDto?> ReturnBookAsync(int borrowRecordId)
        {
            var record = await _context.BorrowRecords
                                       .Include(br => br.Book)
                                       .Include(br => br.Member)
                                       .FirstOrDefaultAsync(br => br.Id == borrowRecordId);

            if (record == null) return null;
            if (record.Status == BorrowStatus.Returned)
                throw new InvalidOperationException("This book has already been returned.");

            record.MarkAsReturned();
            record.Book.ReturnCopy();

            await _context.SaveChangesAsync();
            return MapToDto(record);
        }

        private static BorrowRecordDto MapToDto(BorrowRecord record) => new()
        {
            Id = record.Id,
            BookId = record.BookId,
            BookTitle = record.Book?.Title ?? string.Empty,
            MemberId = record.MemberId,
            MemberName = record.Member?.FullName ?? string.Empty,
            BorrowDate = record.BorrowDate,
            DueDate = record.DueDate,
            ReturnDate = record.ReturnDate,
            Status = record.Status.ToString()
        };
    }
}
